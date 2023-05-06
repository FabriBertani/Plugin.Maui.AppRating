using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.Services.Store;
using Windows.UI.Core;
using Launcher = Windows.System.Launcher;

namespace Plugin.Maui.AppRating;

partial class AppRatingImplementation : IAppRating
{
    /// <summary>
    /// Open an in-app review popup of your current application.
    /// </summary>
    /// <remarks>To use this method the <b>Target Version</b> must be 10.0.17763 or above.</remarks>
    public async Task PerformInAppRateAsync()
    {
        var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;

        await dispatcher?.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
            StoreContext storeContext = StoreContext.GetDefault();

            var result = await storeContext.RequestRateAndReviewAppAsync();

            switch (result.Status)
            {
                case StoreRateAndReviewStatus.Error:
                    await ShowErrorMessage("ERROR", "There was an error trying to opening in-app rating.");

                    break;
                case StoreRateAndReviewStatus.CanceledByUser:
                    await ShowErrorMessage("ACTION CANCELED", "In-app rating action canceled by user.");

                    break;
                case StoreRateAndReviewStatus.NetworkError:
                    await ShowErrorMessage("ERROR", "Please check your internet connection first.");

                    break;
            }
        });
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
            await ShowErrorMessage("ERROR", "Please, provide the application ProductId for Microsoft Store.");

            return;
        }            
            
        try
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={productId}"));
        }
        catch (Exception)
        {
            await ShowErrorMessage("ERROR", "Cannot open rating because Microsoft Store was unable to launch.");
        }
    }

    private static async Task ShowErrorMessage(string title, string message)
    {
        // To create a native dialog there is an issue with the XamlRoot on WinUI,
        // so we have to call the MainPage of the MAUI application and use the
        // DisplayAlert method.
        // In case something goes wrong, we throw an exception.
        try
        {
            if (Application.Current != null)
            {
                if (Application.Current.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}