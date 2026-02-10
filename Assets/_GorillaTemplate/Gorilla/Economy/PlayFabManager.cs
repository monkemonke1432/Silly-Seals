using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Normal.GorillaTemplate {
    /// <summary>
    /// A class that handles PlayFab economy.
    /// </summary>
    public static class PlayFabManager {
        private static readonly string __configureMessage = "PlayFab is not configured. To enable PlayFab features, assign your Title ID using \"PlayFab > MakePlayFabSharedSettings\" in the Unity Editor.";

        private static bool __hasWarnedAboutConfiguration = false;
        private static Task __initializationTask;
        private static Dictionary<string, List<CatalogItem>> __catalogs = new Dictionary<string, List<CatalogItem>>();
        private static Dictionary<string, int> __virtualCurrencies = new Dictionary<string, int>();
        private static List<ItemInstance> __inventory = new List<ItemInstance>();

        /// <summary>
        /// Whether the PlayFab SDK has been configured for this application by assigning a Title ID.
        /// </summary>
        public static bool isConfigured => !string.IsNullOrWhiteSpace(PlayFabSettings.TitleId);

        /// <summary>
        /// Whether the user is logged in to PlayFab.
        /// </summary>
        public static bool isLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

        /// <summary>
        /// Whether the user is banned on PlayFab.
        /// </summary>
        public static bool isBanned { get; private set; } = false;

        /// <summary>
        /// A delegate for handling changes to a catalog.
        /// </summary>
        /// <param name="catalog">The catalog version that was updated.</param>
        /// <param name="items">The list of items in the catalog.</param>
        public delegate void CatalogUpdatedHandler(string catalog, List<CatalogItem> items);

        /// <summary>
        /// An event invoked when a catalog has finished refreshing.
        /// </summary>
        public static event CatalogUpdatedHandler onCatalogChanged;

        /// <summary>
        /// A delegate for handling changes to a virtual currency balance.
        /// </summary>
        /// <param name="currencyCode">The currency code of the virtual currency that changed.</param>
        /// <param name="newBalance">The new balance of the virtual currency.</param>
        public delegate void CurrencyBalanceUpdatedHandler(string currencyCode, int newBalance);

        /// <summary>
        /// An event invoked when the balance of a virtual currency has finished refreshing.
        /// </summary>
        public static event CurrencyBalanceUpdatedHandler onCurrencyBalanceChanged;

        /// <summary>
        /// A delegate for handling changes to the user's inventory.
        /// </summary>
        /// <param name="items">The list of items in the user's inventory.</param>
        public delegate void InventoryUpdatedHandler(List<ItemInstance> items);

        /// <summary>
        /// An event invoked when user's inventory has finished refreshing.
        /// </summary>
        public static event InventoryUpdatedHandler onInventoryChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() {
            _ = InitializeAsync();
        }

        /// <summary>
        /// Initializes the PlayFab client.
        /// </summary>
        /// <remarks>
        /// This is called automatically when the game starts, but can also be called manually if you need
        /// to ensure the client is initialized at a specific point in your code.
        /// </remarks>
        public static async Task InitializeAsync() {
            // If the PlayFab client is already initialized, we don't need to do anything.
            if (__initializationTask == null && isLoggedIn) {
                return;
            }

            // If the initialization task is already running, wait for it to complete, rather than initializing multiple times.
            if (__initializationTask == null) {
                __initializationTask = DoInitializeAsync();
            }

            await __initializationTask;
        }

        private static async Task DoInitializeAsync() {
            // Ensure the PlayFab settings are configured with a valid Title ID.
            if (!isConfigured) {
                if (!__hasWarnedAboutConfiguration) {
                    Debug.LogWarning(__configureMessage);
                    __hasWarnedAboutConfiguration = true;
                }
                return;
            }

            // Ensure the Meta Platform SDK is initialized before retrieving the user details.
            await MetaPlatformManager.InitializeAsync();

            if (!MetaPlatformManager.isConfigured) {
                Debug.LogError("Meta Platform SDK must be configured before using PlayFab.");
                return;
            }
            if (!MetaPlatformManager.isInitialized) {
                Debug.LogError("Meta Platform failed to initialize. Cannot proceed with PlayFab login.");
                return;
            }

            var user = MetaPlatformManager.user;

            if (user == null) {
                Debug.LogError("Meta Platform user is not logged in. Cannot proceed with PlayFab login.");
                return;
            }

            // Log the user in to PlayFab using their Oculus ID.
            await LoginAsync(user.OculusID, user.DisplayName);

            __initializationTask = null;
        }

        private static async Task LoginAsync(string userID, string userName) {
            Debug.Log($"Attempting to login to PlayFab using custom ID \"{userID}\"...");

            var taskCompletionSource = new TaskCompletionSource<bool>();

            try {
                PlayFabClientAPI.LoginWithCustomID(
                    new LoginWithCustomIDRequest {
                        CustomId = userID,
                        CreateAccount = true,
                    },
                    delegate (LoginResult result) {
                        if (result.NewlyCreated) {
                            Debug.Log($"Logged into PlayFab as new user \"{result.PlayFabId}\".");
                        } else {
                            Debug.Log($"Logged into PlayFab as existing user \"{result.PlayFabId}\".");
                        }

                        taskCompletionSource.SetResult(true);

                        // Set a display name for the user in the PlayFab dashboard, if one is provided.
                        if (!string.IsNullOrWhiteSpace(userName)) {
                            PlayFabClientAPI.UpdateUserTitleDisplayName(
                                new UpdateUserTitleDisplayNameRequest {
                                    DisplayName = userName,
                                },
                                delegate (UpdateUserTitleDisplayNameResult result) {
                                    Debug.Log($"PlayFab user display name set to \"{userName}\".");
                                },
                                delegate (PlayFabError error) {
                                    Debug.LogError($"Failed to set PlayFab user name: {error.GenerateErrorReport()}");
                                }
                            );
                        }

                        // Refresh the default catalog.
                        _ = RefreshCatalogAsync();

                        // Get the user's virtual currencies and inventory immediately after login so
                        // that we have the latest data available.
                        _ = RefreshInventoryAsync();
                    },
                    delegate (PlayFabError error) {
                        if (error.Error == PlayFabErrorCode.AccountBanned) {
                            isBanned = true;
                        }

                        Debug.LogError($"Failed to login to PlayFab: {error.GenerateErrorReport()}");
                        taskCompletionSource.SetResult(false);
                    }
                );
            } catch (Exception ex) {
                Debug.LogError($"Failed to login to PlayFab: {ex.Message}");
                taskCompletionSource.SetResult(false);
            }

            await taskCompletionSource.Task;
        }

        /// <summary>
        /// Gets the balance of the specified virtual currency for the user.
        /// </summary>
        /// <param name="currencyCode">The currency code of the currency balance to retrieve.</param>
        /// <returns>The currency balance, or zero if the user is not logged in or the currency does not exist.</returns>
        public static int GetCurrencyBalance(string currencyCode) {
            return __virtualCurrencies.GetValueOrDefault(currencyCode, 0);
        }

        /// <summary>
        /// Gets the user's inventory of items.
        /// </summary>
        /// <returns>The items in the user's inventory, or an empty list if the user is not logged in.</returns>
        public static List<ItemInstance> GetInventory() {
            return __inventory;
        }

        /// <summary>
        /// Checks if the user has a specific item in their inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to check for in the inventory.</param>
        /// <returns>Returns true if the item is found in the inventory, otherwise false.</returns>
        public static bool IsItemInInventory(string itemId) {
            return __inventory.Exists(item => item.ItemId == itemId);
        }

        /// <summary>
        /// Gets the catalog items for the specified catalog version.
        /// </summary>
        /// <param name="catalog">The catalog version to retrieve items from. If null, gets the default catalog.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of catalog items
        /// for the specified catalog version, or an empty list if the catalog does not exist.</returns>
        public static async Task<List<CatalogItem>> GetCatalogAsync(string catalog = null) {
            if (string.IsNullOrWhiteSpace(catalog)) {
                catalog = string.Empty;
            }

            // When accessing a catalog, we should refresh it from PlayFab if it isn't locally available yet.
            if (!__catalogs.TryGetValue(catalog, out var items)) {
                await RefreshCatalogAsync(catalog);
            }

            // If the catalog is still not available after the refresh, cache an empty list to avoid repeated refreshes.
            if (!__catalogs.TryGetValue(catalog, out items)) {
                items = new List<CatalogItem>();
                __catalogs[catalog] = items;
            }

            return items;
        }

        /// <summary>
        /// Gets an item from the catalog.
        /// </summary>
        /// <param name="itemId">The ID of the catalog item to retrieve.</param>
        /// <param name="catalog">The catalog version to retrieve the item from. If null, gets the default catalog.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the item from the catalog, or
        /// null if the item could not be found in the specified catalog.</returns>
        public static async Task<CatalogItem> GetItemFromCatalogAsync(string itemId, string catalog = null) {
            if (string.IsNullOrWhiteSpace(itemId)) {
                return null;
            }

            var items = await GetCatalogAsync(catalog);
            return items.FirstOrDefault(item => item.ItemId == itemId);
        }

        /// <summary>
        /// Refreshes the specified catalog from PlayFab.
        /// </summary>
        /// <param name="catalog">The catalog version to refresh. If null, refreshes the default catalog.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task RefreshCatalogAsync(string catalog = null) {
            if (!CheckConfiguredAndLoggedIn("Cannot refresh catalog: ")) {
                return;
            }

            // Null will refresh using the default catalog.
            if (string.IsNullOrWhiteSpace(catalog)) {
                catalog = null;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();

            try {
                PlayFabClientAPI.GetCatalogItems(
                    new GetCatalogItemsRequest {
                        CatalogVersion = catalog,
                    },
                    delegate (GetCatalogItemsResult result) {
                        // The default catalog is stored under the empty string.
                        catalog ??= string.Empty;

                        __catalogs[catalog] = result.Catalog;

                        OnCatalogChanged(catalog, result.Catalog);

                        taskCompletionSource.SetResult(true);
                    },
                    delegate (PlayFabError error) {
                        Debug.LogError($"Failed to refresh catalog \"{catalog}\": {error.GenerateErrorReport()}");
                        taskCompletionSource.SetResult(false);
                    }
                );
            } catch (Exception ex) {
                Debug.LogError($"Failed to refresh catalog \"{catalog}\": {ex.Message}");
                taskCompletionSource.SetResult(false);
            }

            await taskCompletionSource.Task;
        }

        /// <summary>
        /// Forces a refresh of the user's inventory and virtual currencies from PlayFab.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task RefreshInventoryAsync() {
            if (!CheckConfiguredAndLoggedIn("Cannot refresh inventory: ")) {
                return;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();

            try {
                PlayFabClientAPI.GetUserInventory(
                    new GetUserInventoryRequest(),
                    delegate (GetUserInventoryResult result) {
                        __virtualCurrencies = result.VirtualCurrency;
                        __inventory = result.Inventory;

                        foreach (var currency in __virtualCurrencies) {
                            OnCurrencyBalanceChanged(currency.Key, currency.Value);
                        }
                        OnInventoryChanged();

                        taskCompletionSource.SetResult(true);
                    },
                    delegate (PlayFabError error) {
                        Debug.LogError($"Failed to refresh user inventory: {error.GenerateErrorReport()}");
                        taskCompletionSource.SetResult(false);
                    }
                );
            } catch (Exception ex) {
                Debug.LogError($"Failed to refresh user inventory: {ex.Message}");
                taskCompletionSource.SetResult(false);
            }

            await taskCompletionSource.Task;
        }

        /// <summary>
        /// Add the specified amount of virtual currency to the user's account.
        /// </summary>
        /// <param name="currencyCode">The currency code of the virtual currency to add.</param>
        /// <param name="amount">The amount of virtual currency to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the currency was
        /// successfully added, otherwise false.</returns>
        public static async Task<bool> AddCurrencyAsync(string currencyCode, int amount) {
            if (!CheckConfiguredAndLoggedIn("Cannot purchase currency: ")) {
                return false;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();

            try {
                PlayFabClientAPI.AddUserVirtualCurrency(
                    new AddUserVirtualCurrencyRequest {
                        VirtualCurrency = currencyCode,
                        Amount = amount,
                    },
                    delegate (ModifyUserVirtualCurrencyResult result) {
                        Debug.Log($"Successfully added {amount} {currencyCode} to user inventory. New balance: {result.Balance}");

                        __virtualCurrencies[currencyCode] = result.Balance;

                        onCurrencyBalanceChanged?.Invoke(currencyCode, result.Balance);

                        taskCompletionSource.SetResult(true);
                    },
                    delegate (PlayFabError error) {
                        Debug.LogError($"Failed to add {amount} {currencyCode} to user inventory: {error.GenerateErrorReport()}");
                        taskCompletionSource.SetResult(false);
                    }
                );
            } catch (Exception ex) {
                Debug.LogError($"Failed to add {amount} {currencyCode} to user inventory: {ex.Message}");
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
        }

        /// <summary>
        /// Purchases an item from the PlayFab catalog.
        /// </summary>
        /// <param name="catalog">The catalog version to purchase the item from.</param>
        /// <param name="itemId">The ID of the item to purchase.</param>
        /// <param name="currencyCode">The currency code of the virtual currency to use for the purchase.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the purchase was
        /// successful, otherwise false.</returns>
        public static async Task<bool> PurchaseItemAsync(string catalog, string itemId, string currencyCode) {
            if (!CheckConfiguredAndLoggedIn("Failed to purchase item: ")) {
                return false;
            }

            // Ensure the item is in the catalog.
            var catalogItem = await GetItemFromCatalogAsync(itemId, catalog);

            if (catalogItem == null) {
                Debug.LogError($"Cannot purchase item: Item \"{itemId}\" not found in catalog \"{catalog}\".");
                return false;
            }

            // Get the price of the item in the specified catalog and currency.
            var price = (int)catalogItem.VirtualCurrencyPrices.GetValueOrDefault(currencyCode, 0u);
            var isFree = price <= 0;

            var taskCompletionSource = new TaskCompletionSource<bool>();

            try {
                PlayFabClientAPI.PurchaseItem(
                    new PurchaseItemRequest {
                        CatalogVersion = catalog,
                        ItemId = itemId,
                        VirtualCurrency = isFree ? null : currencyCode,
                        Price = price,
                    },
                    delegate (PurchaseItemResult result) {
                        Debug.Log($"Successfully purchased item \"{itemId}\" for {price} {currencyCode}.");

                        // Add the purchased item to the user's inventory and update the virtual currency balance.
                        __inventory.AddRange(result.Items);

                        if (!isFree) {
                            if (__virtualCurrencies.TryGetValue(currencyCode, out var balance)) {
                                __virtualCurrencies[currencyCode] = balance - price;
                            }

                            OnCurrencyBalanceChanged(currencyCode, __virtualCurrencies[currencyCode]);
                        }

                        OnInventoryChanged();

                        taskCompletionSource.SetResult(true);

                        // Refresh the full inventory to ensure everything is up-to-date with PlayFab.
                        // Strictly speaking, this is not necessary since we already updated the local
                        // inventory and currency balances, but it ensures that our local state changes
                        // match the true state.
                        _ = RefreshInventoryAsync();
                    },
                    delegate (PlayFabError error) {
                        Debug.LogError($"Failed to purchase item \"{itemId}\": {error.GenerateErrorReport()}");

                        // Trying to purchase an item using an out-of-date price will result in an error.
                        // In this case, we can force a fresh catalog refresh in order to get the latest prices.
                        _ = RefreshCatalogAsync(catalog);

                        taskCompletionSource.SetResult(false);
                    }
                );
            } catch (Exception ex) {
                Debug.LogError($"Failed to purchase item \"{itemId}\": {ex.Message}");
                taskCompletionSource.SetResult(false);
            }

            return await taskCompletionSource.Task;
        }

        private static bool CheckConfiguredAndLoggedIn(string prefix = "") {
            if (!isConfigured) {
                Debug.LogError(prefix + __configureMessage);
                return false;
            }
            if (!isLoggedIn) {
                Debug.LogError(prefix + $"User is not logged into PlayFab. Please call {nameof(PlayFabManager)}.{nameof(InitializeAsync)}() before using PlayFab.");
                return false;
            }
            return true;
        }

        private static void OnCatalogChanged(string catalog, List<CatalogItem> items) {
            try {
                onCatalogChanged?.Invoke(catalog, items);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static void OnCurrencyBalanceChanged(string currencyCode, int newBalance) {
            try {
                onCurrencyBalanceChanged?.Invoke(currencyCode, newBalance);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static void OnInventoryChanged() {
            try {
                onInventoryChanged?.Invoke(__inventory);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }
    }
}
