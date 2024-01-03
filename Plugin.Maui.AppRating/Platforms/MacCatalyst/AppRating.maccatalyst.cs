using Foundation;
using StoreKit;
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

        if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
        {
            var windowScene = UIApplication.SharedApplication?.ConnectedScenes?
                .ToArray<UIScene>()?
                .FirstOrDefault(ws => ws.ActivationState == UISceneActivationState.ForegroundActive) as UIWindowScene;

            if (windowScene != null)
            {
                SKStoreReviewController.RequestReview(windowScene);

                tcs.SetResult(true);

                return tcs.Task;
            }
        }
        else
        {
            DisplayErrorAlert("Your current Mac Catalyst version doesn't support in-app rating.");

            tcs.SetResult(false);
        }

        return tcs.Task;
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS/MacCatalyst</param>
    /// <param name="productId">Use <b>productId</b> for Windows.</param>
    public Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        var tcs = new  TaskCompletionSource<bool>();

        if (string.IsNullOrEmpty(applicationId))
        {
            DisplayErrorAlert("Please provide the ApplicationId for Apple App Store.");

            tcs.SetResult(false);

            return tcs.Task;
        }

        var url = $"itms-apps://itunes.apple.com/app/id{applicationId}?action=write-review";

        var nativeUrl = new NSUrl(url);

        try
        {
            UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions());

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
                                                      actionPositive => alert.DismissViewController(true, null));

            alert.AddAction(positiveAction);

            var window = UIApplication.SharedApplication.ConnectedScenes
                .OfType<UIWindowScene>()
                .SelectMany(s => s.Windows)
                .FirstOrDefault(w => w.IsKeyWindow);

            window?.RootViewController?.PresentViewController(alert, true, null);
        });
    }
}