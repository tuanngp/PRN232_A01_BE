namespace Services.Interface
{
    public interface ISeedDataService
    {
        Task SeedAdminAccountAsync();
        Task SeedCategoriesAsync();
        Task SeedTagsAsync();
        Task SeedNewsArticlesAsync();
        Task SeedNewsArticleTagsAsync();
        Task SeedAllDataAsync();
    }
}
