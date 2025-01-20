using Plugin.Maui.AppRating;

namespace AppRatingSample
{
    public partial class MainPage : ContentPage
    {
        private readonly IAppRating _appRating;

        // We are using the Instagram application as an example here
        private const string androidPackageName = "com.instagram.android";
        private const string iOSApplicationId = "id389801252";
        private const string windowsProductId = "9nblggh5l9xt";

        public MainPage(IAppRating appRating)
        {
            InitializeComponent();

            _appRating = appRating;

            if (!Preferences.Get("application_rated", false))
                Task.Run(CheckAppCountAndRate);
        }

        private async Task CheckAppCountAndRate()
        {
            if (Preferences.Get("application_counter",0) >= 5)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (!await DisplayAlert("Rate this App!", "Are you enjoying the so far? Would you like to leave a review in the store?", "Yes", "No"))
                    {
                        Preferences.Set("application_counter", 0);

                        return;
                    }
                });

                await RateApplicationInApp();
            }
        }

        private async Task RateApplicationInApp()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
#if DEBUG
                await _appRating.PerformInAppRateAsync(true);
#else
                await _appRating.PerformInAppRateAsync();
#endif
            });

            Preferences.Set("application_rated", true);
        }

        private async Task RateApplicationOnStore()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _appRating.PerformRatingOnStoreAsync(packageName: androidPackageName, applicationId: iOSApplicationId, productId: windowsProductId);
            });

            Preferences.Set("application_rated", true);
        }

        private void InAppRating_Clicked(object sender, EventArgs e)
        {
            Task.Run(RateApplicationInApp);
        }

        private void AppRateOnStore_Clicked(object sender, EventArgs e)
        {
            if (!Preferences.Get("application_rated", false))
                Task.Run(RateApplicationOnStore);
        }
    }
}