using Microsoft.UI.Dispatching;
using Windows.Services.Store;
using Launcher = Windows.System.Launcher;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : IAppRating
{
    /// <summary>
    /// If set to true, exceptions will be thrown when an error occurs.
    /// </summary>
    public bool ThrowErrors { get;  set; }

    /// <summary>
    /// Open an in-app review popup of your current application.
    /// </summary>
    /// <remarks>To use this method the <b>Target Version</b> must be 10.0.17763 or above.</remarks>
    public async Task PerformInAppRateAsync(bool isTestOrDebugMode)
    {
        var dispatcher = DispatcherQueue.GetForCurrentThread()
            ?? WindowStateManager.Default.GetActiveWindow()?.DispatcherQueue;

        if (dispatcher is null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: DispatcherQueue is not available.");

            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, async () =>
        {
            try
            {
                StoreContext storeContext = StoreContext.GetDefault();

                var hwnd = GetWindowHandle();

                WinRT.Interop.InitializeWithWindow.Initialize(storeContext, hwnd);

                var result = await storeContext.RequestRateAndReviewAppAsync();

                switch (result.Status)
                {
                    case StoreRateAndReviewStatus.Error:
                        System.Diagnostics.Debug.WriteLine("ERROR: There was an error trying to opening in-app rating.");

                        break;
                    case StoreRateAndReviewStatus.CanceledByUser:
                        System.Diagnostics.Debug.WriteLine("ACTION CANCELED: In-app rating action canceled by user.");

                        break;
                    case StoreRateAndReviewStatus.NetworkError:
                        System.Diagnostics.Debug.WriteLine("NETWORK ERROR: Please check your internet connection first.");

                        break;
                }

                tcs.SetResult(result.Status == StoreRateAndReviewStatus.Succeeded);
            }
            catch (Exception ex)
            {
                if (ThrowErrors)
                    throw;

                System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");

                tcs.SetResult(false);
            }
        });

        await tcs.Task;
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="appId">Identifier of the application, use <b>packageName</b> for Android,
    /// <b>applicationId</b> for iOS and/or <b>productId</b> for Windows</param>
    public Task PerformRatingOnStoreAsync(string appId)
    {
        return PerformRatingOnStoreAsync(productId: appId);
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows</param>
    public async Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        if (string.IsNullOrEmpty(productId))
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Please, provide the application ProductId for Microsoft Store.");

            return;
        }

        try
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={productId}"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Cannot open rating because Microsoft Store was unable to launch.");

            if (ThrowErrors)
                throw;

            System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
        }
    }

    private static nint GetWindowHandle() => ((MauiWinUIWindow)Application.Current?.Windows[0].Handler.PlatformView!).WindowHandle;
}