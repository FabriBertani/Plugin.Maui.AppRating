using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Google.Android.Play.Core.Review;
using Xamarin.Google.Android.Play.Core.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : Java.Lang.Object, IAppRating, IOnCompleteListener
{
    private static volatile Handler? handler;

    private TaskCompletionSource<bool>? inAppRateTcs;

    private IReviewManager? reviewManager;

    private Xamarin.Google.Android.Play.Core.Tasks.Task? launchTask;

    private bool forceReturn;

    /// <summary>
    /// Open Android in-app review popup of your current application.
    /// </summary>
    public async Task PerformInAppRateAsync()
    {
        inAppRateTcs?.TrySetCanceled();

        inAppRateTcs = new TaskCompletionSource<bool>();

        reviewManager = ReviewManagerFactory.Create(Android.App.Application.Context);

        forceReturn = false;

        var request = reviewManager.RequestReviewFlow();

        request.AddOnCompleteListener(this);

        await inAppRateTcs.Task;

        reviewManager.Dispose();

        request.Dispose();
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

        if (!string.IsNullOrEmpty(packageName))
        {
            var context = Android.App.Application.Context;
            var url = $"market:details?id={packageName}";

            try
            {
                Intent intent = new(Intent.ActionView, Android.Net.Uri.Parse(url));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);

                context.StartActivity(intent);

                tcs.SetResult(true);
            }
            catch (PackageManager.NameNotFoundException)
            {
                ShowAlertMessage("Error", "Cannot open rating because Google Play is not installed.");

                tcs.SetResult(false);
            }
            catch (ActivityNotFoundException)
            {
                // If Google Play fails to load, open the App link on the browser.
                var playStoreUrl = $"https://play.google.com/store/apps/details?id={packageName}";

                var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(playStoreUrl));
                browserIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);

                context.StartActivity(browserIntent);

                tcs.SetResult(true);
            }
        }
        else
        {
            ShowAlertMessage("Error", "Please, provide the application PackageName for Google Play Store.");

            tcs.SetResult(false);
        }

        return tcs.Task;
    }

    public void OnComplete(Xamarin.Google.Android.Play.Core.Tasks.Task task)
    {
        if (!task.IsSuccessful || forceReturn)
        {
            inAppRateTcs?.TrySetResult(forceReturn);

            launchTask?.Dispose();

            return;
        }

        try
        {
            ReviewInfo reviewInfo = (ReviewInfo)task.GetResult(Java.Lang.Class.FromType(typeof(ReviewInfo)));

            forceReturn = true;

            launchTask = reviewManager?.LaunchReviewFlow(Platform.CurrentActivity, reviewInfo);

            launchTask?.AddOnCompleteListener(this);
        }
        catch (Exception ex)
        {
            ShowAlertMessage("Error", "There was an error launching in-app review. Please try again.");

            inAppRateTcs?.TrySetResult(false);

            System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
        }
    }

    private static void ShowAlertMessage(string title, string message)
    {
        if (handler?.Looper != Looper.MainLooper)
            handler = new Handler(Looper.MainLooper);

        handler?.Post(() =>
        {
            var dialog = new AlertDialog.Builder(Platform.CurrentActivity);
            dialog.SetTitle(title);
            dialog.SetMessage(message);

            dialog.SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null);

            var alert = dialog.Create();

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                alert?.Window?.SetType(Android.Views.WindowManagerTypes.SystemAlert);

            alert?.Show();
        });

        handler?.Dispose();
    }
}