using Foundation;
using StoreKit;
using UIKit;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : IAppRating
{
    /// <summary>
    /// If set to true, exceptions will be thrown when an error occurs.
    /// </summary>
    public bool ThrowErrors { get; set; }

    /// <summary>
    /// Open in-app review popup of your current application.
    /// </summary>
    public Task PerformInAppRateAsync(bool isTestOrDebugMode)
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            return MainThread.InvokeOnMainThreadAsync(PerformInAppRateOnMacCatalyst16AndAboveAsync);
        else if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            return MainThread.InvokeOnMainThreadAsync(PerformInAppRateOnMacCatalyst14AndAboveAsync);
        else
        {
            var errorMessage = "ERROR: Your current Mac Catalyst version doesn't support in-app rating.";

            System.Diagnostics.Debug.WriteLine(errorMessage);

            if (ThrowErrors)
                throw new NotSupportedException(errorMessage);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="appId">Identifier of the application, use <b>packageName</b> for Android,
    /// <b>applicationId</b> for iOS and/or <b>productId</b> for Windows</param>
    public async Task PerformRatingOnStoreAsync(string appId)
    {
        await PerformRatingOnStoreAsync(applicationId: appId);
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS/MacCatalyst</param>
    /// <param name="productId">Use <b>productId</b> for Windows.</param>
    public async Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            var errorMessage = "ERROR: Please provide the ApplicationId for Apple App Store.";

            System.Diagnostics.Debug.WriteLine(errorMessage);

            if (ThrowErrors)
                throw new ArgumentException(errorMessage, nameof(applicationId));

            return;
        }

        var url = GetAppStoreReviewUrl(applicationId);

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await UIApplication.SharedApplication.OpenUrlAsync(url, new UIApplicationOpenUrlOptions());
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Cannot open rating because App Store was unable to launch.");

            if (ThrowErrors)
                throw;

            System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
        }
    }

    private static Task PerformInAppRateOnMacCatalyst16AndAboveAsync()
    {
#if NET9_0_OR_GREATER && MACCATALYST16_1_OR_GREATER
        if (UIApplication.SharedApplication?.ConnectedScenes?
            .OfType<UIScene>()?
            .FirstOrDefault(ws => ws.ActivationState == UISceneActivationState.ForegroundActive) is UIWindowScene windowScene)
        {
#pragma warning disable APL0004, CA1416
            AppStore.RequestReview(windowScene);
#pragma warning restore APL0004, CA1416
        }
#endif

        return Task.CompletedTask;
    }

    private static Task PerformInAppRateOnMacCatalyst14AndAboveAsync()
    {
        if (UIApplication.SharedApplication?.ConnectedScenes?
            .OfType<UIScene>()?
            .FirstOrDefault(ws => ws.ActivationState == UISceneActivationState.ForegroundActive) is UIWindowScene windowScene)
        {
            SKStoreReviewController.RequestReview(windowScene);
        }

        return Task.CompletedTask;
    }

    private static NSUrl GetAppStoreReviewUrl(string applicationId)
    {
        return new NSUrl($"itms-apps://itunes.apple.com/app/id{applicationId}?action=write-review");
    }
}