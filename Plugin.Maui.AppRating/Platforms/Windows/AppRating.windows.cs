using Microsoft.UI.Dispatching;
using Windows.Services.Store;
using Launcher = Windows.System.Launcher;

namespace Plugin.Maui.AppRating;

internal partial class AppRatingImplementation : IAppRating
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
            System.Diagnostics.Trace.TraceWarning("DispatcherQueue is not available.");

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
                        System.Diagnostics.Trace.TraceError("There was an error trying to opening in-app rating.");

                        break;
                    case StoreRateAndReviewStatus.CanceledByUser:
                        System.Diagnostics.Trace.TraceInformation("ACTION CANCELED: In-app rating action canceled by user.");

                        break;
                    case StoreRateAndReviewStatus.NetworkError:
                        System.Diagnostics.Trace.TraceError("Please check your internet connection first.");

                        break;
                }

                tcs.SetResult(result.Status == StoreRateAndReviewStatus.Succeeded);
            }
            catch (Exception ex)
            {
                if (ThrowErrors)
                    throw;

                System.Diagnostics.Trace.TraceError($"Error message: {ex.Message}");
                System.Diagnostics.Trace.TraceError($"Stacktrace: {ex}");

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
            System.Diagnostics.Trace.TraceWarning("Please, provide the application ProductId for Microsoft Store.");

            return;
        }

        try
        {
            var success = await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={productId}"));

            if (!success)
            {
                System.Diagnostics.Trace.TraceError("Microsoft Store URL could not be opened. The system returned 'false'.");

                if (ThrowErrors)
                    throw new InvalidOperationException("Failed to open Microsoft Store URL. The system was unable to handle the request.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Cannot open rating because Microsoft Store was unable to launch.");

            if (ThrowErrors)
                throw;

            System.Diagnostics.Trace.TraceError($"Error message: {ex.Message}");
            System.Diagnostics.Trace.TraceError($"Stacktrace: {ex}");
        }
    }

    private static nint GetWindowHandle() => ((MauiWinUIWindow)Application.Current?.Windows[0].Handler.PlatformView!).WindowHandle;
}