using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Google.Android.Play.Core.Review;
using Xamarin.Google.Android.Play.Core.Review.Testing;

namespace Plugin.Maui.AppRating;

internal partial class AppRatingImplementation : Java.Lang.Object, IAppRating, Android.Gms.Tasks.IOnCompleteListener
{
#if NET9_0_OR_GREATER
    private readonly Lock _rateLock = new();
#else
    private readonly object _rateLock = new();
#endif

    private TaskCompletionSource<bool>? _inAppRateTcs;

    private IReviewManager? _reviewManager;

    private Android.Gms.Tasks.Task? _launchTask;

    private bool _forceReturn;

    /// <summary>
    /// If set to true, exceptions will be thrown when an error occurs.
    /// </summary>
    public bool ThrowErrors { get; set; }

    /// <summary>
    /// Open Android in-app review popup of your current application.
    /// </summary>
    public async Task PerformInAppRateAsync(bool isTestOrDebugMode = false)
    {
#if NET9_0_OR_GREATER
        lock (_rateLock)
#else
        lock (_rateLock)
#endif
        {
            if (_inAppRateTcs is not null && !_inAppRateTcs.Task.IsCompleted)
                throw new Exception("In-app rating flow is already in progress.");

            _inAppRateTcs?.TrySetCanceled();

            _inAppRateTcs = new();
        }

        _reviewManager = isTestOrDebugMode
            ? new FakeReviewManager(Android.App.Application.Context)
            : ReviewManagerFactory.Create(Android.App.Application.Context);

        _forceReturn = false;

        var request = _reviewManager.RequestReviewFlow();

        request.AddOnCompleteListener(this);

        try
        {
            await _inAppRateTcs.Task;
        }
        finally
        {
            _reviewManager?.Dispose();

            request?.Dispose();

            _launchTask?.Dispose();
        }
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

                return Task.CompletedTask;
            }
            catch (PackageManager.NameNotFoundException nameNotFoundEx)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Cannot open rating because Google Play is not installed.");

                if (ThrowErrors)
                    throw;

                System.Diagnostics.Debug.WriteLine($"Error message: {nameNotFoundEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Stacktrace: {nameNotFoundEx}");
            }
            catch (ActivityNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("INFO: Google Play fails to load");

                // If Google Play fails to load, open the App link on the browser.
                var playStoreUrl = $"https://play.google.com/store/apps/details?id={packageName}";

                var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(playStoreUrl));
                browserIntent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);

                context.StartActivity(browserIntent);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Unexpected error when opening rating.");

                if (ThrowErrors)
                    throw;

                System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: Please, provide the application PackageName for Google Play Store.");
        }

        return Task.CompletedTask;
    }

    public void OnComplete(Android.Gms.Tasks.Task task)
    {
#if NET9_0_OR_GREATER
        lock (_rateLock)
#else
        lock (_rateLock)
#endif
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
                System.Diagnostics.Debug.WriteLine("ERROR: There was an error launching in-app review. Please try again.");

                if (ThrowErrors)
                    throw;

                System.Diagnostics.Debug.WriteLine($"Error message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stacktrace: {ex}");
                _inAppRateTcs?.TrySetResult(false);
            }
        }
    }
}