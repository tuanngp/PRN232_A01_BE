using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services.Auth
{
    public class SeedDataService : ISeedDataService
    {
        private readonly ISystemAccountService _systemAccountService;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(
            ISystemAccountService systemAccountService,
            IAuthService authService,
            IConfiguration configuration,
            ILogger<SeedDataService> logger
        )
        {
            _systemAccountService = systemAccountService;
            _authService = authService;
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
                var existingAccounts = await _systemAccountService.GetAllAsync();
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

                await _systemAccountService.AddAsync(adminAccount);
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
    }
}
