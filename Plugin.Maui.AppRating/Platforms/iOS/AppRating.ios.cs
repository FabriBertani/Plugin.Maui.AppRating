using Foundation;
using StoreKit;
using System.Runtime.Versioning;
using UIKit;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : IAppRating
{
    /// <summary>
    /// Open in-app review popup of your current application.
    /// </summary>
    public Task PerformInAppRateAsync(bool isTestOrDebugMode)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            {
                if (UIApplication.SharedApplication?.ConnectedScenes?
                    .ToArray<UIScene>()?
                    .FirstOrDefault(ws => ws.ActivationState == UISceneActivationState.ForegroundActive) is UIWindowScene windowScene)
                {
                    SKStoreReviewController.RequestReview(windowScene);

                    tcs.SetResult(true);

                    return tcs.Task;
                }
            }

#pragma warning disable CA1422
            SKStoreReviewController.RequestReview();
#pragma warning restore CA1422

            tcs.SetResult(true);
        }
        else
        {
            DisplayErrorAlert("Your current iOS version doesn't support in-app rating.");

            tcs.SetResult(false);
        }

        return tcs.Task;
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="appId">Identifier of the application, use <b>packageName</b> for Android,
    /// <b>applicationId</b> for iOS and/or <b>productId</b> for Windows</param>
    public Task PerformRatingOnStoreAsync(string appId)
    {
        return PerformRatingOnStoreAsync(applicationId: appId);
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows.</param>
    public Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        var tcs = new TaskCompletionSource<bool>();

        if (string.IsNullOrEmpty(applicationId))
        {
            DisplayErrorAlert("Please provide the ApplicationId for Apple App Store");

            tcs.SetResult(false);

            return tcs.Task;
        }

        var url = new NSUrl($"itms-apps://itunes.apple.com/app/id{applicationId}?action=write-review");

        try
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                UIApplication.SharedApplication.OpenUrlAsync(url, new UIApplicationOpenUrlOptions());
            else
                UIApplication.SharedApplication.OpenUrl(url);

            tcs.SetResult(true);
        }
        catch (Exception)
        {
            DisplayErrorAlert("Cannot open rating because App Store was unable to launch.");

            tcs.SetResult(false);
        }

        return tcs.Task;
    }

    private static void DisplayErrorAlert(string errorMessage)
    {
        NSRunLoop.Main.InvokeOnMainThread(() =>
        {
            var alert = UIAlertController.Create("Error",
                                                 errorMessage,
                                                 UIAlertControllerStyle.Alert);

            var positiveAction = UIAlertAction.Create("OK",
                                                      UIAlertActionStyle.Default,
                                                      actonPositive => alert.DismissViewController(true, null));

            alert.AddAction(positiveAction);

            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                var window = UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(s => s.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);

                window?.RootViewController?.PresentViewController(alert, true, null);
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
            {
                var window = UIApplication.SharedApplication.Windows.FirstOrDefault(o => o.IsKeyWindow);

                window?.RootViewController?.PresentViewController(alert, true, null);
            }
            else
                UIApplication.SharedApplication.KeyWindow?.RootViewController?.PresentViewController(alert, true, null);
        });
    }

    internal static Version ParseVersion(string version)
    {
        if (Version.TryParse(version, out var number))
            return number;

        if (int.TryParse(version, out var major))
            return new Version(major, 0);

        return new Version(0, 0);
    }
}