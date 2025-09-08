
namespace Plugin.Maui.AppRating;

internal partial class AppRatingImplementation : IAppRating
{
    public bool ThrowErrors { get; set; }

    public Task PerformInAppRateAsync(bool isTestOrDebugMode)
    {
        throw new NotImplementedException();
    }

    public Task PerformRatingOnStoreAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task PerformRatingOnStoreAsync(string packageName = "", string applicationId = "", string productId = "")
    {
        throw new NotImplementedException();
    }
}