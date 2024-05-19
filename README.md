# Plugin.Maui.AppRating
[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.AppRating.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.AppRating/)

`Plugin.Maui.AppRating` gives developers a fast and easy way to ask users to rate the app on the stores.

## Platforms supported

|Platform|Version|
|-------------------|:------------------:|
|.Net MAUI Android|API 21+|
|.Net MAUI iOS|iOS 11.0+|
|Windows|10.0.17763+|
|Mac Catalyst|14.0+|

## Installation
`Plugin.Maui.AppRating` is available via NuGet, grab the latest package and install it in your solution:

    Install-Package Plugin.Maui.AppRating

In your `MauiProgram` class add the following using statement:

```csharp
using Plugin.Maui.AppRating;
```

Finally, add the default instance of the plugin as a singleton to inject it in your code late:

```csharp
builder.Services.AddSingleton<IAppRating>(AppRating.Default);
```

## Version 1.2.0
### New Features
- Added .Net8 support to all platforms.
- Updated libraries.
- Added `FakeReviewManager` to allow Android.
- Fixed Windows implementation.

## :warning: Considerations regarding new platform policies :warning:
### Android
- It's highly recommended to test this or any other store review plugin on a real device with Google Play installed instead of using an emulator.
- Due to new regulations from Google, the review dialogue will not be displayed on manual distribution or debug mode (apparently), only on apps published and distributed via Google Play Store, however, it is recommended to release your app under "Internal distribution" or "Internal App Sharing" to effectively test the store review popup. [Read here for more information](https://developer.android.com/guide/playcore/in-app-review/test). Additionally, you can debug the error using `adb logcat`.
- `FakeReviewManager` is a new feature released by Google, primarily designed for testing and unit testing purposes. It operates without a user interface (UI). For more information, visit the [official Android documentation](https://developer.android.com/reference/com/google/android/play/core/review/testing/FakeReviewManager).
- To integrate `FakeReviewManager` in your plugin, pass `true` to the method `PerformInAppRateAsync`. This feature is exclusive to Android.

### iOS
- During development, submitting a review is not possible, but the review popup dialog will still show on your simulator or device.
- :warning: The review dialogue **_will not be opened_** if the app is downloaded from TestFlight.

## API Usage
Call the injected interface in any page or viewmodel to gain access to the APIs.

There are two main methods in the plugin: `PerformInAppRateAsync` and `PerformRatingOnStoreAsync`.

### Android
```csharp
/// <summary>
/// Perform rating without leaving the app.
/// </summary>
Task PerformInAppRateAsync(bool isTestOrDebugMode = false);
```
> This method will open an in-app review dialogue, using the `packageName` declared on the `AndroidManifest` file.

```csharp
/// <summary>
/// Perform rating on the current OS store app or open the store page on browser.
/// </summary>
Task PerformRatingOnStoreAsync();
```
> This method will open the **_Google Play app_** on the store page of your current application. Otherwise, it will try to open the store page on the browser.

If neither the store page nor the browser store page works, it will display an alert announcing the error.

`packageName` **must** be provided as a named argument to open the store page on the store app or browser.

#### Example
```csharp
await _appRating.PerformRatingOnStoreAsync(packageName: "com.instagram.android");
```

### iOS | Mac Catalyst
```csharp
/// <summary>
/// Perform rating without leaving the app.
/// </summary>
Task PerformInAppRateAsync();
```
> if the device current OS version is _10.3+_ in **iOS**, or _14.0+_ in **Mac Catalyst**, this method will raise an in-app review popup of your current application, otherwise, it will display an alert announcing that it's not supported.

```csharp
/// <summary>
/// Perform rating on the current OS store app or open the store page on browser.
/// </summary>
Task PerformRatingOnStoreAsync();
```
> This method will open the **App Store app** on the store page of your current application. Otherwise, it will try to open the store page on the browser.

If the method fails. it will display an alert announcing the error.

`applicationId` property is the **_StoreId_** of your application and it **must** be provided as a named argument to open the store page on the store app or browser.

#### Example
```csharp
await _appRating.PerformRatingOnStoreAsync(applicationId: "id389801252");
```

### Windows
```csharp
/// <summary>
/// Perform rating without leaving the app.
/// </summary>
Task PerformInAppRateAsync();
```
> This method will raise an in-app review dialog of your current application, otherwise, it will display an alert announcing that it's not supported.

```csharp
/// <summary>
/// Perform rating on the current OS store app or open the store page on browser.
/// </summary>
Task PerformRatingOnStoreAsync();
```
> This method will open the **_Microsoft Store application_** on the store page of your current application. Otherwise, it will try to open the store page on the browser.

If this method fails, it will display an alert announcing the error.

`productId` property is the **ProductId** of your application and it **must** be provided as a named argument to open the store page on the store app or browser.

Example
```csharp
await _appRating.PerformRatingOnStoreAsync(productId: "9nblggh5l9xt");
```

## Usage
> :warning: **Warning** - You should be careful about **how and when** you ask users to rate your app, there may be penalties from stores. As for advice, I recommend using a counter on the app start and storage that count, then when the counter reaches a specific number, display a dialogue asking the users if they want to rate the app, if they decline the offer, reset the counter to ask them later, also leave the option to do it themselves.

```csharp
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
            Task.Run(() => CheckAppCountAndRate());
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
# if DEBUG
            await _appRating.PerformInAppRateAsync(true);
#else
            await _appRating.PerformInAppRateAsync();
#endif
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
```

## Sample
Take a look at the [AppRatingSample](https://github.com/FabriBertani/Plugin.Maui.AppRating/tree/main/samples/AppRatingSample) for fully detailed implementation of this plugin.

## Contributions
Please, feel free to open an [Issue](https://github.com/FabriBertani/Plugin.Maui.AppRating/issues) if you found any bugs or submit a PR.

## License
Plugin.Maui.AppRating is licensed under [MIT](https://github.com/FabriBertani/Plugin.Maui.AppRating/blob/main/LICENSE).