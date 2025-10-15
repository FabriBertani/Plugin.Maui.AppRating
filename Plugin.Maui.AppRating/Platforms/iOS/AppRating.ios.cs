using Foundation;
using StoreKit;
using UIKit;

namespace Plugin.Maui.AppRating;

internal partial class AppRatingImplementation : IAppRating
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
            return MainThread.InvokeOnMainThreadAsync(PerformInAppRateOniOS16AndAboveAsync);
        else if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            return MainThread.InvokeOnMainThreadAsync(PerformInAppRateOniOS14AndAboveAsync);
        else if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
            return MainThread.InvokeOnMainThreadAsync(PerformInAppRateOniOS10AndAboveAsync);
        else
        {
            System.Diagnostics.Trace.TraceError("Your current iOS version doesn't support in-app rating.");

            if (ThrowErrors)
                throw new NotSupportedException("Your current iOS version doesn't support in-app rating.");

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
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows.</param>
    public async Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            var errorMessage = "Please provide the ApplicationId for Apple App Store";

            System.Diagnostics.Trace.TraceError(errorMessage);

            if (ThrowErrors)
                throw new ArgumentException(errorMessage, nameof(applicationId));

            return;
        }

        var url = GetAppStoreReviewUrl(applicationId);

        try
        {
            var success = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                    return await UIApplication.SharedApplication.OpenUrlAsync(url, new UIApplicationOpenUrlOptions());
                else
                    return UIApplication.SharedApplication.OpenUrl(url);
            });

            if (!success)
            {
                System.Diagnostics.Trace.TraceError("The App Store URL could not be opened. The system returned 'false'.");

                if (ThrowErrors)
                    throw new InvalidOperationException("Failed to open App Store URL. The system was unable to handle the request.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Cannot open rating because App Store was unable to launch.");

            if (ThrowErrors)
                throw;

            System.Diagnostics.Trace.TraceError($"Error message: {ex.Message}");
            System.Diagnostics.Trace.TraceError($"Stacktrace: {ex}");
        }
    }


    private static Task PerformInAppRateOniOS16AndAboveAsync()
    {
#if NET9_0_OR_GREATER && IOS16_0_OR_GREATER
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

    private static Task PerformInAppRateOniOS14AndAboveAsync()
    {
        if (UIApplication.SharedApplication?.ConnectedScenes?
            .OfType<UIScene>()?
            .FirstOrDefault(ws => ws.ActivationState == UISceneActivationState.ForegroundActive) is UIWindowScene windowScene)
        {
            SKStoreReviewController.RequestReview(windowScene);
        }

        return Task.CompletedTask;
    }

    private static Task PerformInAppRateOniOS10AndAboveAsync()
    {
        SKStoreReviewController.RequestReview();

        return Task.CompletedTask;
    }

    private static NSUrl GetAppStoreReviewUrl(string applicationId)
        => new($"itms-apps://itunes.apple.com/app/id{applicationId}?action=write-review");

    internal static Version ParseVersion(string version)
    {
        if (Version.TryParse(version, out var number))
            return number;

        if (int.TryParse(version, out var major))
            return new Version(major, 0);

        return new Version(0, 0);
    }
}