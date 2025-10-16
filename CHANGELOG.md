# Changelog

## 1.2.3 (2025/10/16)
[Full Changelog](https://github.com/FabriBertani/Plugin.Maui.AppRating/compare/v1.2.2...v1.2.3)

**Implemented enhancements:**
- Improved logging by implementing `System.Diagnostics.Trace`.
- Added better error handling and logging.
- Updated sample project.

## 1.2.2 (2025/09/09)
[Full Changelog](https://github.com/FabriBertani/Plugin.Maui.AppRating/compare/v1.2.1...v1.2.2)

**Implemented enhancements:**
- Updated `Xamarin.Google.Android.Play.Review.Ktx` package for Android.
- Updated iOS/MacCatalyst by the new `AppStore.RequestReview` from iOS and MacCatalyst version 16 and above.
- Added `ThrowErrors` property to allow users to catch exceptions with the new error handling implementation.
- Implemented improvements for error handling.
- Removed old error alerts.
- Applied code improvements to the library.
- Update sample project.

**Fixed bugs:**
- Fixed [#16 (code question)](https://github.com/FabriBertani/Plugin.Maui.AppRating/issues/16).
- Fixed [#17 (Dependency Conflicts with .NET MAUI 9.0.30 Due to AndroidX Version Constraints)](https://github.com/FabriBertani/Plugin.Maui.AppRating/issues/17).

## 1.2.1 (2025/01/21)
[Full Changelog](https://github.com/FabriBertani/Plugin.Maui.AppRating/compare/v1.2.0...v1.2.1)

**Implemented enhancements:**
- Removed .Net7 and added .Net9 support to all platforms.
- Replaced the old `Xamarin.Google.Android.Play.Core` package with the modern `Xamarin.Google.Android.Play.Review.Ktx` package that also supports .Net9, on Android.

**Fixed bugs:**
- Fixed [#11](https://github.com/FabriBertani/Plugin.Maui.AppRating/issues/11) where `Xamarin.Google.Android.Play.Core` interfiere with `Xamarin.Firebase.Auth` package because both use `StateUpdatedListenerImplementor` method.

## 1.2.0 (2024/05/19)
[Full Changelog](https://github.com/FabriBertani/Plugin.Maui.AppRating/compare/v1.1.0...v1.2.0)

**Implemented enhancements:**
- Added .Net8 support to all platforms.
- Updated libraries.
- Added `FakeReviewManager` to allow Android.
- Added an overload to `PerformRatingOnStoreAsync` method in order to take only one application identifier.
- Update the sample app.

**Fixed bugs:**
- Fixed Windows implementation.

## 1.1.0 (2023/06/05)
[Full Changelog](https://github.com/FabriBertani/Plugin.Maui.AppRating/compare/v1.0.0...v1.1.0)

**Implemented enhancements:**
- Added support for Windows and Mac Catalyst.
- Added Windows and Mac Catalyst to sample project.
- Fixes and improvements.
