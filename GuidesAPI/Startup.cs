using GuidesApi;
using GuidesApi.Data;
using GuidesApi.Data.Models;
using GuidesApi.Extensions;
using Lamar;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MedSelectApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection"))); // TODO: Remove big timeout value when migration has run

            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });


            //services.Configure<PasswordStrengthOptions>(options =>
            //{
            //    options.MinLength = 10;
            //    options.RequireNumeric = false;
            //    options.RequireUppercase = true;
            //    options.RequireLowercase = true;
            //    options.RequireSpecial = false;
            //    options.ErrorMessage = "Your password must be at least 10 characters long, and contain a combination of uppercase and lowercase letters";
            //});

            services.AddRazorPages();
            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToFolder("/Public");
            });

            services.Configure<FormOptions>(options => options.ValueCountLimit = 4096);

            //services.AddTransient<IConfirmationEmailGenerator, ConfirmationEmailGenerator>();

            services.AddMvc();

            services.AddJoshMvc<Startup>(Configuration)
                .RegisterDefaultDependencies<Startup, User, Permission>(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}