namespace Normal.GorillaTemplate {
    /// <summary>
    /// Configures <see cref="Grabber.bodyRoot"/> for a gorilla player.
    /// </summary>
    public class GorillaGrabber : Grabber {
        private void Start() {
            // This ensures that we correctly convert XR controller velocities into world-space coordinates
            var playerRoot = GorillaLocalRig.instance.playerRootTransform;
            bodyRoot = playerRoot.transform;
        }
    }
}
