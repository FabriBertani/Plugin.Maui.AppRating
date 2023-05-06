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
                if (!await DisplayAlert("Rate this App!", "Are you enjoying the so far? Would you like to leave a review in the store?", "Yes", "No"))
                {
                    Preferences.Set("application_counter", 0);

                    return;
                }

                await RateApplicationInApp();
            }
        }

        private Task RateApplicationInApp()
        {
            Dispatcher.Dispatch(async () =>
            {
                await _appRating.PerformInAppRateAsync();
            });

            Preferences.Set("application_rated", true);

            return Task.CompletedTask;
        }

        private Task RateApplicationOnStore()
        {
            Dispatcher.Dispatch(async () =>
            {
                await _appRating.PerformRatingOnStoreAsync(packageName: androidPackageName, applicationId: iOSApplicationId, productId: windowsProductId);
            });

            Preferences.Set("application_rated", true);

            return Task.CompletedTask;
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