namespace Plugin.Maui.AppRating;

public interface IAppRating
{
    /// <summary>
    /// Perform rating without leaving the app.
    /// </summary>
    Task PerformInAppRateAsync(bool isTestOrDebugMode = false);

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="packageName">Use <b>packageName</b> for Android.</param>
    /// <param name="applicationId">Use <b>applicationId</b> for iOS.</param>
    /// <param name="productId">Use <b>productId</b> for Windows</param>
    Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "");

    /// <summary>
    /// Perform rating on the current OS store app or open the store page on browser.
    /// </summary>
    /// <param name="appId">Identifier of the application, use <b>packageName</b> for Android,
    /// <b>applicationId</b> for iOS and/or <b>productId</b> for Windows</param>
    Task PerformRatingOnStoreAsync(string appId);

    /// <summary>
    /// If set to true, exceptions will be thrown when an error occurs.
    /// </summary>
    bool ThrowErrors { set; }
}