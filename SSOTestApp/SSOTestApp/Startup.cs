using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SSOTestApp.Constants;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;

namespace SSOTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public const string OidcSession = nameof(OidcSession);
        public const string saml2 = nameof(saml2);

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication(o =>
                {
                    o.DefaultSignInScheme = ApplicationSamlConstants.External;
                })
                .AddCookie(OidcSession)
                .AddCookie(saml2, o => { o.Cookie.Name = saml2;})
                .AddSaml2(opt =>
                {
                    opt.SignInScheme = saml2;
                    
                    opt.SPOptions = new SPOptions
                    {
                        EntityId = new EntityId("https://localhost:44343")
                    };

                    opt.IdentityProviders.Add(new IdentityProvider(
                        new EntityId(Configuration["Saml2:IdpEntityId"]), opt.SPOptions)
                    {
                        MetadataLocation = Configuration["Saml2:IdpMetadata"]
                    });
                })
                .AddOpenIdConnect(opt =>
                {
                    opt.SignInScheme = OidcSession;

                    opt.Authority = Configuration["Oidc:Authority"];
                    opt.ClientId = Configuration["Oidc:ClientId"];
                    opt.ClientSecret = Configuration["Oidc:ClientSecret"];

                    opt.ResponseType = "code";
                    opt.UsePkce = true;

                    opt.Scope.Add("openid");
                    opt.Scope.Add("profile");
                    opt.Scope.Add("api");

                    opt.SaveTokens = true;
                });

#if DEBUG
            services.AddRazorPages().AddRazorRuntimeCompilation();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
