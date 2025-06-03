using System.Text;
using System.Text.Json;
using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NguyenGiaPhuongTuan_SE17D06_A01_BE.Middleware;
using Repositories;
using Repositories.Impl;
using Repositories.Interface;
using Services;
using Services.Auth;
using Services.Impl;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "FUNewsManagementSystem API",
                        Version = "v1",
                    }
                );
                c.AddSecurityDefinition(
                    "Bearer",
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Description =
                            @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                    }
                );
                c.AddSecurityRequirement(
                    new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                    {
                        {
                            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                            {
                                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                            },
                            new List<string>()
                        },
                    }
                );
            });
            builder.Services.AddDbContext<FUNewsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddScoped<FUNewsDbContext>();

            builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
            builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            builder.Services.AddScoped<INewsArticleTagRepository, NewsArticleTagRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<INewsArticleTagService, NewsArticleTagService>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ISeedDataService, SeedDataService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            builder
                .Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        // ClockSkew = TimeSpan.Zero // Mặc định là 5 phút, cho phép chênh lệch thời gian giữa server và client.
                        // Đặt Zero nếu muốn kiểm tra thời gian chính xác.
                    };

                    // Cấu hình để đọc token từ cookie
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Ưu tiên lấy token từ Authorization header trước
                            var token = context.Request.Headers["Authorization"]
                                .FirstOrDefault()?.Split(" ").LastOrDefault();

                            // Nếu không có trong header, thử lấy từ cookie
                            if (string.IsNullOrEmpty(token))
                            {
                                token = context.Request.Cookies["accessToken"];
                            }

                            if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception != null)
                            {
                                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffPolicy", policy => policy.RequireRole("Staff"));
            });
            builder
                .Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                })
                .AddOData(opt =>
                    opt.Select()
                        .Filter()
                        .Expand()
                        .OrderBy()
                        .SetMaxTop(100)
                        .Count()
                        .AddRouteComponents("odata", GetEdmModel())
                );
            var app = builder.Build();

            // Đăng ký Exception Handling Middleware (phải đầu tiên)
            app.UseExceptionHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Cấu hình để xử lý lỗi authentication
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 401)
                {
                    context.Response.Clear();
                    throw new UnauthorizedAccessException("Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn");
                }
            });

            // Đăng ký OData Response Wrapper (trước authentication)
            app.UseODataResponseWrapper();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Seed all data
            using (var scope = app.Services.CreateScope())
            {
                var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
                await seedDataService.SeedAllDataAsync();
            }

            app.Run();

            static Microsoft.OData.Edm.IEdmModel GetEdmModel()
            {
                var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();

                builder.EntitySet<SystemAccount>("SystemAccounts");
                builder.EntitySet<NewsArticle>("NewsArticles");
                builder.EntitySet<Category>("Categories");
                builder.EntitySet<Tag>("Tags");
                builder
                    .EntityType<NewsArticleTag>()
                    .HasKey(nt => new { nt.NewsArticleId, nt.TagId });
                builder.EntitySet<NewsArticleTag>("NewsArticleTags");

                return builder.GetEdmModel();
            }
        }
    }
}
