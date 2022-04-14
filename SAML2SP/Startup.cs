using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace SAML2SP
{
    public class Startup
    {
        public static IWebHostEnvironment AppEnvironment { get; private set; }

        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configs = Configuration.GetSection("Saml2");
            services.Configure<Saml2Configuration>(configs);
            //services.Configure<Saml2Configuration>(saml2Configuration =>
            //{
            //    //saml2Configuration.SignAuthnRequest = true;
            //    saml2Configuration.SigningCertificate = CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"]);

            //    //saml2Configuration.SignatureValidationCertificates.Add(CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SignatureValidationCertificateFile"])));
            //    saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

            //    var entityDescriptor = new EntityDescriptor();
            //    entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri(Configuration["Saml2:IdPMetadata"]));
            //    if (entityDescriptor.IdPSsoDescriptor != null)
            //    {
            //        saml2Configuration.AllowedIssuer = entityDescriptor.EntityId;
            //        saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
            //        saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
            //        saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
            //        if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
            //        {
            //            saml2Configuration.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
            //        }
            //    }
            //    else
            //    {
            //        throw new Exception("IdPSsoDescriptor not loaded from metadata.");
            //    }
            //});

            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);

                var entityDescriptor = new EntityDescriptor();
                entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri(Configuration["Saml2:IdPMetadata"]));
                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                    saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                    saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                }
                else
                {
                    throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                }
            });

            services.AddSaml2(slidingExpiration: true);

            services.AddControllersWithViews();
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

            app.UseSaml2();
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
