namespace AppRatingSample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!Preferences.Get("application_rated", false))
            {
                var counter = Preferences.Get("application_counter", 1);

                counter += 1;

                Preferences.Set("application_counter", counter);
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}