using Audit.Core;
using Dapper;
using FluentValidation.AspNetCore;
using GuidesApi.Data.Models;
using Lamar;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GuidesApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ServiceRegistry AddJoshMvc<TStartup>(this ServiceRegistry services, IConfiguration configuration, Action<RazorPagesOptions> razorPagesOptions = null) where TStartup : class
        {
            services.AddServices<TStartup>(configuration, razorPagesOptions)
                .AddAuthentication(configuration);

            return services;
        }

        public static IServiceProvider RegisterDefaultDependencies<TStartup, TUser, TPermission>(this ServiceRegistry services, IConfiguration configuration) where TUser : User, new() where TPermission : Permission, new() where TStartup : class
        {
            var container = new Container(x =>
            {
                x.For<Permission>().Use(new TPermission()).Singleton();

                x.For<IConfiguration>().Use(configuration).Singleton();

                x.For<IHttpContextAccessor>().Use<HttpContextAccessor>().Singleton();

                x.For<ServiceFactory>().Use<ServiceFactory>();

            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                var urlHelperFactory = factory.GetService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
                return urlHelper;
            });

            services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

            services.Scan(_ =>
            {
                _.AssemblyContainingType<TStartup>();
                _.AssemblyContainingType<TUser>();
                _.WithDefaultConventions(Lamar.Scanning.Conventions.OverwriteBehavior.Never);
            });

#if DEBUG
            Debug.Write(container.WhatDidIScan());
            Debug.Write(container.WhatDoIHave());
#endif

            return container.GetInstance<IServiceProvider>();
        }

        public static AuthenticationBuilder AddAuthentication(this ServiceRegistry services, IConfiguration configuration)
        {
            return services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = configuration["Authentication:CookieLoginPath"];
                    options.AccessDeniedPath = configuration["Authentication:AccessDeniedPath"];
                    options.ExpireTimeSpan =
                        TimeSpan.FromMinutes(Convert.ToInt32(configuration["Authentication:CookieExpireMinutes"]));
                    options.SlidingExpiration = Convert.ToBoolean(configuration["Authentication:CookieSlidingWindow"]);
                    //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Authentication:JwtIssuer"],
                    ValidAudience = configuration["Authentication:JwtIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:JwtKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }


        public static ServiceRegistry AddServices<TStartup>(this ServiceRegistry services, IConfiguration configuration, Action<RazorPagesOptions> razorPagesOptions = null) where TStartup : class
        {
            // Add Base Message Service If Typeof IMessageService not already defined
            //if (services.None(x => x.ServiceType.Name == "IMessageService"))
            //{
            //    services.AddTransient<IMessageService, MessageService>();
            //}

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddDataProtection();

            //services.ConfigureOptions(typeof(UiConfigureOptions));
            //services.ConfigureOptions(typeof(DefaultRazorPagesOptions));

            services.AddMvc()
                .AddControllersAsServices()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;

                })
                .AddFluentValidation(x =>
                {
                    x.RegisterValidatorsFromAssemblyContaining<TStartup>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMemoryCache();

            InitializeLogging(configuration);
            var accessor = services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

            Audit.Core.Configuration.Setup()
                .UseEntityFramework(x => x
                    .AuditTypeMapper(t => typeof(AuditItem))
                    .AuditEntityAction<AuditItem>((ev, entry, entity) =>
                    {
                        entity.RequestId = Activity.Current?.Id ?? accessor.HttpContext?.TraceIdentifier;
                        entity.Table = entry.Table;
                        entity.PK = entry.PrimaryKey.Keys.Count == 1 ? entry.PrimaryKey.First().Value.ToString() : JsonConvert.SerializeObject(entry.PrimaryKey);
                        entity.Environment = ev.Environment.ToJson();
                        entity.Duration = ev.Duration;
                        entity.Action = entry.Action;
                        entity.Data = entry.ToJson();
                        entity.EntityType = entry.EntityType.Name;
                        entity.Date = DateTime.UtcNow;
                        entity.UserId = accessor?.HttpContext?.User?.GetUserId();
                        entity.User = accessor?.HttpContext?.User?.Identity?.Name ?? "System";
                    })
                    .IgnoreMatchedProperties());

            return services;
        }

        private static void InitializeLogging(IConfiguration configuration)
        {
            try
            {
                using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
                {
                    var query = @"
                        IF OBJECT_ID(N'[dbo].[Logs]', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[Logs](
	                            [Id] [nvarchar](450) NOT NULL,
	                            [Timestamp] [datetimeoffset](7) NOT NULL,
	                            [Level] [nvarchar](max) NOT NULL,
	                            [Message] [nvarchar](max) NOT NULL,
	                            [Exception] [nvarchar](max) NULL,
	                            [User] [nvarchar](max) NOT NULL,
                             CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED 
                            (
	                            [Id] ASC
                            )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                            ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                        END

                        IF COL_LENGTH ('[dbo].[Logs]', 'CorrelationId') IS NULL
                        BEGIN
	                        ALTER TABLE [dbo].[Logs]
	                        ADD [CorrelationId] NVARCHAR(MAX) NULL
                        END

                        IF COL_LENGTH ('[dbo].[Logs]', 'UserId') IS NULL
                        BEGIN
	                        ALTER TABLE [dbo].[Logs]
	                        ADD [UserId] NVARCHAR(MAX) NULL
                        END
                    ";

                    connection.Execute(query);
                }
            }
            catch (SqlException)
            { }
        }
    }
}
