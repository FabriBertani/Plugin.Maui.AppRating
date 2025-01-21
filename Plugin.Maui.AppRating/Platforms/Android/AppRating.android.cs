using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Google.Android.Play.Core.Review;
using Xamarin.Google.Android.Play.Core.Review.Testing;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : Java.Lang.Object, IAppRating, Android.Gms.Tasks.IOnCompleteListener
{
    private static volatile Handler? _handler;

    private TaskCompletionSource<bool>? _inAppRateTcs;

    private IReviewManager? _reviewManager;

    private Android.Gms.Tasks.Task? _launchTask;

    private bool _forceReturn;

    /// <summary>
    /// Open Android in-app review popup of your current application.
    /// </summary>
    public async Task PerformInAppRateAsync(bool isTestOrDebugMode = false)
    {
        _inAppRateTcs?.TrySetCanceled();

        _inAppRateTcs = new();

        _reviewManager = isTestOrDebugMode
            ? new FakeReviewManager(Android.App.Application.Context)
            : ReviewManagerFactory.Create(Android.App.Application.Context);

        _forceReturn = false;

        var request = _reviewManager.RequestReviewFlow();

        request.AddOnCompleteListener(this);

        await _inAppRateTcs.Task;

        _reviewManager.Dispose();

        request.Dispose();
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="appId">Identifier of the application, use <b>packageName</b> for Android,
    /// <b>applicationId</b> for iOS and/or <b>productId</b> for Windows</param>
    public Task PerformRatingOnStoreAsync(string appId)
    {
        return PerformRatingOnStoreAsync(packageName: appId);
    }

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows.</param>
    public Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        TaskCompletionSource<bool> tcs = new();

        if (!string.IsNullOrEmpty(packageName))
        {
            var context = Platform.AppContext;
            var url = $"market://details?id={packageName}";

            try
            {
                Intent intent = new(Intent.ActionView, Android.Net.Uri.Parse(url));
                intent.AddFlags(ActivityFlags.NoHistory);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    intent.AddFlags(ActivityFlags.NewDocument);
                else
                    intent.AddFlags(ActivityFlags.ClearWhenTaskReset);
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

    public void OnComplete(Android.Gms.Tasks.Task task)
    {
        if (!task.IsSuccessful || _forceReturn)
        {
            _inAppRateTcs?.TrySetResult(_forceReturn);

            _launchTask?.Dispose();

            return;
        }

        try
        {
            ReviewInfo reviewInfo = (ReviewInfo)task.GetResult(Java.Lang.Class.FromType(typeof(ReviewInfo)));

            _forceReturn = true;

            _launchTask = _reviewManager?.LaunchReviewFlow(Platform.CurrentActivity, reviewInfo);

            _launchTask?.AddOnCompleteListener(this);
        }
        catch (Exception ex)
        {
            ShowAlertMessage("Error", "There was an error launching in-app review. Please try again.");

            _inAppRateTcs?.TrySetResult(false);

            System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
        }
    }

    private static void ShowAlertMessage(string title, string message)
    {
        if (_handler?.Looper != Looper.MainLooper)
            _handler = new Handler(Looper.MainLooper!);

        _handler?.Post(() =>
        {
            var dialog = new AlertDialog.Builder(Platform.CurrentActivity);
            dialog.SetTitle(title);
            dialog.SetMessage(message);

            dialog.SetPositiveButton("OK", (s, e) => { });

            var alert = dialog.Create();

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                alert?.Window?.SetType(Android.Views.WindowManagerTypes.SystemAlert);

            alert?.Show();
        });

        _handler?.Dispose();
    }
}