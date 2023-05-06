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
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows</param>
    Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "");
}