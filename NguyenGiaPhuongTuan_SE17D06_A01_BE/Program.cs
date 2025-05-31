using System.Text;
using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Impl;
using Services;
using Services.Impl;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
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

            builder.Services.AddScoped<SystemAccountRepository>();
            builder.Services.AddScoped<NewsArticleRepository>();
            builder.Services.AddScoped<NewsArticleTagRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<TagRepository>();

            builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<INewsArticleTagService, NewsArticleTagService>();

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
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffPolicy", policy => policy.RequireRole("Staff"));
            });
            builder
                .Services.AddControllers()
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

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            static Microsoft.OData.Edm.IEdmModel GetEdmModel()
            {
                var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();

                builder.EntitySet<SystemAccount>("SystemAccounts");
                builder.EntitySet<NewsArticle>("NewsArticles");
                builder.EntitySet<Category>("Categories");
                builder.EntitySet<Tag>("Tags");
                builder.EntitySet<NewsArticleTag>("NewsArticleTags");

                return builder.GetEdmModel();
            }
        }
    }
}
