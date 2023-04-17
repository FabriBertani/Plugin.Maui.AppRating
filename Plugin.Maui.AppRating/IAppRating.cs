namespace Plugin.Maui.AppRating;

public interface IAppRating
{
    /// <summary>
    /// Perform rating without leaving the app.
    /// </summary>
    Task PerformInAppRateAsync();

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use this for Android.</param>
    /// <param name="applicationId">Use this for iOS.</param>
    Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "");
}