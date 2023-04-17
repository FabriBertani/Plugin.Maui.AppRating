namespace Plugin.Maui.AppRating;

public static class AppRating
{
    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    static IAppRating? defaultImplementation;

    public static IAppRating? Default => defaultImplementation ??= new AppRatingImplementation();

    internal static void SetDefault(IAppRating? implementation) => defaultImplementation = implementation;
}