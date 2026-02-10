using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Creates a mirror surface, and generates + renders mirror reflections.
/// Supports multiple cameras, including scene view cameras.
/// </summary>
/// <remarks>
/// This version only works in URP, although it could easily be adapted for BiRP.
/// </remarks>
// [ExecuteInEditMode] // Too many warnings/errors in console and small issues related to scene preview, disabling it for now
public class Mirror : MonoBehaviour {
    /// <summary>
    /// Data for any camera that shows the mirror.
    /// </summary>
    private class CameraData {
        public Camera camera;

        /// <summary>
        /// The mirror reflection texture (in mono rendering).
        /// Or the mirror reflection texture for the left eye (in stereo rendering).
        /// </summary>
        public RenderTexture rtMain;

        /// <summary>
        /// The mirror reflection texture for the right eye (only used in stereo rendering).
        /// </summary>
        public RenderTexture rtAlt;

        /// <summary>
        /// Frees the resources allocated for the reflections visible in this camera.
        /// </summary>
        public void Release() {
            if (rtMain != null) {
                rtMain.Release();
            }

            if (rtAlt != null) {
                rtAlt.Release();
            }
        }
    }

    /// <summary>
    /// If <see cref="renderSize"/> is <see cref="Vector2Int.zero"/> then the resolution of the reflection
    /// will be that of the screen multiplied by <see cref="renderScale"/>.
    /// </summary>
    [Min(0.001f)]
    public float renderScale = 1f;

    /// <summary>
    /// (Optional) The exact resolution of the reflection.
    /// </summary>
    public Vector2Int renderSize = Vector2Int.zero;

    /// <summary>
    /// The MSAA level of the reflection.
    /// Valid values are 1, 2, 4, 8.
    /// </summary>
    public int antiAliasing = 4;

    /// <summary>
    /// The filtering mode of the mirror surface texture.
    /// </summary>
    public FilterMode filterMode = FilterMode.Bilinear;

    /// <summary>
    /// Scales the far plane of the reflection, for tuning performance.
    /// </summary>
    [Range(0.0001f, 1f)]
    public float farClipPlaneScale = 1f;

    /// <summary>
    /// Use this to control which objects are rendered in the reflection.
    /// </summary>
    public LayerMask reflectionMask = -1;

    /// <summary>
    /// Controls if this mirror should render shadows.
    /// </summary>
    public bool renderShadows = true;

    /// <summary>
    /// When true, mirror rendering is skipped when it's not inside the visible frustum.
    /// </summary>
    /// <remarks>
    /// The frame where the mirror enters the frustum can spike the frame on XR platforms,
    /// hence it's disabled by default.
    /// </remarks>
    public bool cullUsingFrustum = false;

    /// <summary>
    /// The material of the mirror's surface.
    /// </summary>
    public Material mirrorMaterial => _mirrorMaterial;

    /// <summary>
    /// The object that holds the mirror surface.
    /// </summary>
    private GameObject _mirrorObject;

    /// <summary>
    /// The renderer of the mirror surface.
    /// </summary>
    private MeshRenderer _mirrorMeshRenderer;

    /// <summary>
    /// The material of the mirror surface.
    /// </summary>
    private Material _mirrorMaterial;

    /// <summary>
    /// The object that holds the camera that renders the mirror reflections.
    /// </summary>
    private GameObject _mirrorCameraObject;

    /// <summary>
    /// The camera that renders the mirror reflections.
    /// </summary>
    private Camera _mirrorCamera;

    private float _previousRenderScale = 1f;
    private Vector2Int _previousRenderSize = Vector2Int.zero;
    private readonly List<CameraData> _visibleCameras = new List<CameraData>();
    private bool _hasRenderedMirrorsThisFrame;

    /// <summary>
    /// Corresponds to a texture property in the "VirtualBrightPlayz/Mirror" shader.
    /// The mirror reflection texture (in mono rendering).
    /// Or the mirror reflection texture for the left eye (in stereo rendering).
    /// </summary>
    private static readonly int __mainTexID = Shader.PropertyToID("_MainTex");

    /// <summary>
    /// Corresponds to a texture property in the "VirtualBrightPlayz/Mirror" shader.
    /// The mirror reflection texture for the right eye (only used in stereo rendering).
    /// </summary>
    private static readonly int __altTexID = Shader.PropertyToID("_AltTex");

#region Lifetime

    private void OnEnable() {
        // Create the mirror surface
        if (_mirrorObject == null)
            _mirrorObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _mirrorObject.name = $"{name}_Quad";
        _mirrorObject.transform.SetParent(transform, false);
        // _mirrorObject.hideFlags = HideFlags.HideAndDontSave;
        _mirrorMeshRenderer = _mirrorObject.GetComponent<MeshRenderer>();

        // Create the material for the mirror surface
        if (_mirrorMaterial == null)
            _mirrorMaterial = new Material(Shader.Find("VirtualBrightPlayz/Mirror"));
        _mirrorMeshRenderer.material = _mirrorMaterial;

        // Create the camera that renders the mirror reflections
        if (_mirrorCameraObject == null)
            _mirrorCameraObject = new GameObject($"{name}_Camera", typeof(Camera));
        _mirrorCameraObject.transform.SetParent(transform, false);
        // _mirrorCameraObject.hideFlags = HideFlags.HideAndDontSave;
        _mirrorCamera = _mirrorCameraObject.GetComponent<Camera>();
        _mirrorCamera.enabled = false;

        _previousRenderScale = renderScale;
        _previousRenderSize = renderSize;

        RenderPipelineManager.beginCameraRendering += OnCameraRender;

        // Too many warnings/errors in console and small issues related to scene preview, disabling it for now
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.update += EditorUpdate;
// #endif
    }

    private void OnDisable() {
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.update -= EditorUpdate;
// #endif

        RenderPipelineManager.beginCameraRendering -= OnCameraRender;

        foreach (var data in _visibleCameras) {
            data.Release();
        }

        _visibleCameras.Clear();
    }

    private void OnDestroy() {
        DestroyImmediate(_mirrorObject);
        DestroyImmediate(_mirrorCameraObject);
    }

#endregion

#region Visibility

    // https://forum.unity.com/threads/camera-current-returns-null-when-calling-it-in-onwillrenderobject-with-universalrp.929880/
    private static readonly Plane[] __frustumPlanes = new Plane[6];

    private static bool IsVisible(Camera camera, Bounds bounds) {
        GeometryUtility.CalculateFrustumPlanes(camera, __frustumPlanes);
        return GeometryUtility.TestPlanesAABB(__frustumPlanes, bounds);
    }

    /// <summary>
    /// Returns true if the specified camera can see the mirror.
    /// This lets us skip rendering the reflection if the mirror isn't visible.
    /// </summary>
    private bool IsMirrorVisibleInCamera(Camera cam) {
        // Don't treat the mirror camera as a camera that can see the mirror
        if (cam == _mirrorCamera) {
            return false;
        }

        if (cullUsingFrustum) {
            return IsVisible(cam, _mirrorMeshRenderer.bounds);
        } else {
            return true;
        }
    }

#endregion

#region Event loop

    /// <summary>
    /// Called by the Scriptable Render Pipeline for each camera, including the camera rendering the mirror reflections.
    /// </summary>
    private void OnCameraRender(ScriptableRenderContext ctx, Camera cam) {
        if (!_hasRenderedMirrorsThisFrame) {
            _hasRenderedMirrorsThisFrame = true;

            // We could technically call this in LateUpdate, but Camera.GetStereoViewMatrix would be a frame late.
            // It's a URP-specific quirk.
            // https://discussions.unity.com/t/rendersinglecamera-is-obsolete-but-the-suggested-solution-has-error/898627/19
            RenderMirrorReflections();
        }

        if (IsMirrorVisibleInCamera(cam)) {
            // In BiRP this would be done in OnWillRenderObject instead
            ShowReflectionForCamera(cam);
        }
    }

    /// <summary>
    /// Render a reflection for each camera that can see the mirror.
    /// </summary>
    private void RenderMirrorReflections() {
        if (_previousRenderScale != renderScale || _previousRenderSize != renderSize) {
            OnDisable();
            OnEnable();
        }

        foreach (var cam in Camera.allCameras) {
            if (IsMirrorVisibleInCamera(cam)) {
                RenderReflectionForCamera(cam);
            }
        }
    }

    private void Update() {
        _hasRenderedMirrorsThisFrame = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Render a reflection for each scene camera that can see the mirror.
    /// </summary>
    private void EditorUpdate() {
        foreach (var cam in UnityEditor.SceneView.GetAllSceneCameras()) {
            if (IsMirrorVisibleInCamera(cam)) {
                RenderReflectionForCamera(cam);
            }
        }
    }
#endif

#endregion

    /// <summary>
    /// Resolves the reflection data for the specified camera.
    /// </summary>
    private CameraData GetCameraData(Camera cam) {
        var idx = _visibleCameras.FindIndex(x => x.camera == cam);
        if (idx == -1) {
            _visibleCameras.Add(new CameraData() {
                camera = cam,
                rtMain = CreateRenderTexture($"{name}_Main"),
                rtAlt = CreateRenderTexture($"{name}_Alt"),
            });

            idx = _visibleCameras.Count - 1;
        }

        var cameraData = _visibleCameras[idx];
        return cameraData;
    }

    /// <summary>
    /// Displays the reflection that was rendered for the specified camera on the mirror surface.
    /// Call this just before that camera draws its view of the scene so
    /// that the reflection appears on the mirror geometry.
    /// </summary>
    private void ShowReflectionForCamera(Camera cam) {
        var cameraData = GetCameraData(cam);

        if (cam.stereoEnabled) {
            _mirrorMaterial.SetTexture(__mainTexID, cameraData.rtMain);
            _mirrorMaterial.SetTexture(__altTexID, cameraData.rtAlt);
        } else {
            _mirrorMaterial.SetTexture(__mainTexID, cameraData.rtMain);
        }
    }

    /// <summary>
    /// Renders the mirror reflection from the specified camera's view.
    /// </summary>
    private void RenderReflectionForCamera(Camera cam) {
        if (_mirrorCamera == null || _mirrorObject == null) {
            return;
        }

        var additionalCameraData = _mirrorCamera.GetComponent<UniversalAdditionalCameraData>();
        if (additionalCameraData == null) {
            additionalCameraData = _mirrorCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }
        additionalCameraData.renderShadows = renderShadows;

        // Apply reflection mask
        _mirrorCamera.cullingMask = reflectionMask;

        // Apply far clip plane
        _mirrorCamera.farClipPlane = cam.farClipPlane;

        // This fixes shadow caster culling (in VR and non-VR) and has no downsides. Not sure why.
        // Found by a lengthy process of trial and error.
        var vrAspectBoost = cam.stereoEnabled ? 1.25f : 1.0f; // An extra 1.25 for VR specifically found by trial and error
        _mirrorCamera.aspect = cam.aspect * vrAspectBoost;

        var cameraData = GetCameraData(cam);

        if (cam.stereoEnabled) {
            var viewMatrixLeft = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            var viewMatrixRight = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;

            var projectionMatrixLeft = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            var projectionMatrixRight = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

            // Swap the eyes for the projection matrices. This somehow works out in the math.
            RenderReflectionForEye(viewMatrixLeft, projectionMatrixRight, cameraData.rtMain);
            RenderReflectionForEye(viewMatrixRight, projectionMatrixLeft, cameraData.rtAlt);
        } else {
            RenderReflectionForEye(cam.cameraToWorldMatrix, cam.projectionMatrix, cameraData.rtMain);
        }
    }

    /// <summary>
    /// Renders the mirror reflection for a specific eye (stereo or mono).
    /// </summary>
    private void RenderReflectionForEye(Matrix4x4 viewMat, Matrix4x4 projectionMat, RenderTexture rt) {
        // Copy the projection matrix (it will be used by the Camera.CalculateObliqueMatrix call)
        _mirrorCamera.projectionMatrix = projectionMat;
        // The farClipPlane is derived from projectionMat, from the assignment above
        _mirrorCamera.farClipPlane *= farClipPlaneScale;

        // Decompose the view matrix
        var position = viewMat.GetPosition();
        var forward = new Vector3(-viewMat[0, 2], -viewMat[1, 2], -viewMat[2, 2]);
        var up = new Vector3(viewMat[0, 1], viewMat[1, 1], viewMat[2, 1]);
        forward.Normalize();
        up.Normalize();

        // Mirror the camera pose
        _mirrorCamera.transform.localRotation = Quaternion.LookRotation(Vector3.Reflect(_mirrorObject.transform.InverseTransformDirection(forward), Vector3.forward), Vector3.Reflect(_mirrorObject.transform.InverseTransformDirection(up), Vector3.forward));
        _mirrorCamera.transform.localPosition = Vector3.Reflect(_mirrorObject.transform.InverseTransformPoint(position), Vector3.forward);
        _mirrorCamera.ResetWorldToCameraMatrix();

        // Magic math for the clip plane
        var clipPlane = _mirrorObject.transform;
        var dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, (clipPlane.position - _mirrorCamera.transform.position)));
        var camSpacePos = (_mirrorCamera.worldToCameraMatrix).MultiplyPoint(clipPlane.position);
        var camSpaceNormal = (_mirrorCamera.worldToCameraMatrix).MultiplyVector(clipPlane.forward) * dot;
        var camSpaceDist = -Vector3.Dot(camSpacePos, camSpaceNormal);
        var clipPlaneCamSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDist);

        // Find the oblique projection matrix
        var renderMat = _mirrorCamera.CalculateObliqueMatrix(clipPlaneCamSpace);
        _mirrorCamera.projectionMatrix = renderMat;

        _mirrorCamera.forceIntoRenderTexture = true;
        _mirrorCamera.allowMSAA = true;

        // Render the reflection
        RenderPipeline.SubmitRenderRequest(_mirrorCamera, new UniversalRenderPipeline.SingleCameraRequest() {
            destination = rt,
        });
    }

    /// <summary>
    /// Creates a render texture that will hold a mirror reflection.
    /// </summary>
    /// <param name="rtName">The name of the texture, for debugging purposes.</param>
    private RenderTexture CreateRenderTexture(string rtName) {
        Vector2Int size;
        if (renderSize.x <= 0 || renderSize.y <= 0) {
            // Use a ratio of the screen size
            size = new Vector2Int(Mathf.CeilToInt(Screen.width * renderScale), Mathf.CeilToInt(Screen.height * renderScale));
        } else {
            size = renderSize;
        }

        var rtd = new RenderTextureDescriptor(size.x, size.y, RenderTextureFormat.Default, 24) {
            sRGB = true,
        };

        var rt = new RenderTexture(rtd) {
            name = rtName,
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = antiAliasing,
        };

        rt.Create();

        return rt;
    }
}
