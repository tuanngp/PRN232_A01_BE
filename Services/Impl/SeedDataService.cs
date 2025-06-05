using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Interface;

namespace Services.Auth
{
    public class SeedDataService : ISeedDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<SeedDataService> logger
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAdminAccountAsync()
        {
            try
            {
                var adminConfig = _configuration.GetSection("AdminAccount");
                var adminEmail = adminConfig["Email"];
                var adminPassword = adminConfig["Password"];

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    _logger.LogWarning("Admin account configuration is missing in appsettings");
                    return;
                }

                // Kiểm tra xem admin account đã tồn tại chưa
                var existingAccounts = await _unitOfWork.SystemAccounts.GetAllAsync();
                var existingAdmin = existingAccounts.FirstOrDefault(x =>
                    x.AccountEmail.Equals(adminEmail, StringComparison.OrdinalIgnoreCase)
                );

                if (existingAdmin != null)
                {
                    _logger.LogInformation("Admin account already exists");
                    return;
                }

                // Tạo admin account
                var adminAccount = new SystemAccount
                {
                    AccountName = "System Administrator",
                    AccountEmail = adminEmail,
                    AccountPassword = adminPassword,
                    AccountRole = AccountRole.Admin,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                };

                await _unitOfWork.SystemAccounts.AddAsync(adminAccount);
                _logger.LogInformation(
                    "Admin account seeded successfully with email: {Email}",
                    adminEmail
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding admin account");
            }
        }

        public async Task SeedCategoriesAsync()
        {
            try
            {
                var existingCategories = await _unitOfWork.Categories.GetAllAsync();
                if (existingCategories.Any())
                {
                    _logger.LogInformation("Categories already exist");
                    return;
                }

                var categories = new List<Category>
                {
                    new Category
                    {
                        CategoryName = "Tin tức",
                        CategoryDescription = "Tin tức chung về FPT University",
                    },
                    new Category
                    {
                        CategoryName = "Sự kiện",
                        CategoryDescription = "Các sự kiện diễn ra tại FPT University",
                    },
                    new Category
                    {
                        CategoryName = "Đời sống sinh viên",
                        CategoryDescription = "Đời sống và hoạt động của sinh viên",
                    },
                    new Category
                    {
                        CategoryName = "Học thuật",
                        CategoryDescription = "Thông tin về học thuật và nghiên cứu",
                    },
                };

                foreach (var category in categories)
                {
                    await _unitOfWork.Categories.AddAsync(category);
                }

                _logger.LogInformation("Categories seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding categories");
            }
        }

        public async Task SeedTagsAsync()
        {
            try
            {
                var existingTags = await _unitOfWork.Tags.GetAllAsync();
                if (existingTags.Any())
                {
                    _logger.LogInformation("Tags already exist");
                    return;
                }

                var tags = new List<Tag>
                {
                    new Tag { TagName = "FPT", Note = "Tin liên quan đến FPT" },
                    new Tag { TagName = "Tuyển sinh", Note = "Thông tin tuyển sinh" },
                    new Tag { TagName = "Công nghệ", Note = "Tin tức về công nghệ" },
                    new Tag { TagName = "Hoạt động", Note = "Các hoạt động ngoại khóa" },
                    new Tag { TagName = "Học bổng", Note = "Thông tin học bổng" },
                };

                foreach (var tag in tags)
                {
                    await _unitOfWork.Tags.AddAsync(tag);
                }

                _logger.LogInformation("Tags seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding tags");
            }
        }

        public async Task SeedNewsArticlesAsync()
        {
            try
            {
                var existingArticles = await _unitOfWork.NewsArticles.GetAllAsync();
                if (existingArticles.Any())
                {
                    _logger.LogInformation("News articles already exist");
                    return;
                }

                var categories = await _unitOfWork.Categories.GetAllAsync();
                if (!categories.Any())
                {
                    _logger.LogWarning("No categories found. Please seed categories first");
                    return;
                }

                var articles = new List<NewsArticle>
                {
                    new NewsArticle
                    {
                        NewsTitle = "FPT University thông báo tuyển sinh 2025",
                        NewsContent =
                            "FPT University thông báo tuyển sinh năm học 2025 với nhiều chương trình đào tạo mới...",
                        Headline = "Thông tin tuyển sinh năm 2025",
                        CategoryId = categories.First(c => c.CategoryName == "Tin tức").CategoryId,
                        NewsStatus = NewsStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                    },
                    new NewsArticle
                    {
                        NewsTitle = "Tuần lễ công nghệ FPT TechWeek 2025",
                        NewsContent =
                            "FPT University tổ chức tuần lễ công nghệ với nhiều hoạt động thú vị...",
                        Headline = "Sự kiện công nghệ lớn nhất năm",
                        CategoryId = categories.First(c => c.CategoryName == "Sự kiện").CategoryId,
                        NewsStatus = NewsStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                    },
                };

                foreach (var article in articles)
                {
                    await _unitOfWork.NewsArticles.AddAsync(article);
                }

                _logger.LogInformation("News articles seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding news articles");
            }
        }

        public async Task SeedNewsArticleTagsAsync()
        {
            try
            {
                var existingArticleTags = await _unitOfWork.NewsArticles.GetAllAsync();
                if (existingArticleTags.Any())
                {
                    _logger.LogInformation("News article tags already exist");
                    return;
                }

                var articles = await _unitOfWork.NewsArticles.GetAllAsync();
                var tags = await _unitOfWork.Tags.GetAllAsync();

                if (!articles.Any() || !tags.Any())
                {
                    _logger.LogWarning(
                        "No articles or tags found. Please seed articles and tags first"
                    );
                    return;
                }

                var articleTags = new List<NewsArticleTag>();

                // Gán tag cho bài viết tuyển sinh
                var recruitmentArticle = articles.First(a => a.NewsTitle.Contains("tuyển sinh"));
                var recruitmentTags = tags.Where(t =>
                    new[] { "FPT", "Tuyển sinh" }.Contains(t.TagName)
                );
                articleTags.AddRange(
                    recruitmentTags.Select(tag => new NewsArticleTag
                    {
                        NewsArticleId = recruitmentArticle.NewsArticleId,
                        TagId = tag.TagId,
                    })
                );

                // Gán tag cho bài viết về sự kiện công nghệ
                var techArticle = articles.First(a => a.NewsTitle.Contains("TechWeek"));
                var techTags = tags.Where(t =>
                    new[] { "FPT", "Công nghệ", "Hoạt động" }.Contains(t.TagName)
                );
                articleTags.AddRange(
                    techTags.Select(tag => new NewsArticleTag
                    {
                        NewsArticleId = techArticle.NewsArticleId,
                        TagId = tag.TagId,
                    })
                );

                foreach (var articleTag in articleTags)
                {
                    await _unitOfWork.NewsArticleTags.AddAsync(articleTag);
                }

                _logger.LogInformation("News article tags seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding news article tags");
            }
        }

        public async Task SeedAllDataAsync()
        {
            await SeedAdminAccountAsync();
            await SeedCategoriesAsync();
            await SeedTagsAsync();
            await SeedNewsArticlesAsync();
            await SeedNewsArticleTagsAsync();
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
