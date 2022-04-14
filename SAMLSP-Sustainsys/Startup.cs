using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SAMLSP_Sustainsys.Helpers;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using SAMLSP_Sustainsys.Constants;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;

namespace SAMLSP_Sustainsys
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public const string Saml2Session = nameof(Saml2Session);

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // SameSiteMode.None is required to support SAML SSO.
                options.MinimumSameSitePolicy = SameSiteMode.None;

                options.CheckConsentNeeded = context => false;

                // Some older browsers don't support SameSiteMode.None.
                options.OnAppendCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            //  services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SAMLSP_Sustainsys", Version = "v1" });
                c.DescribeAllEnumsAsStrings();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Name = "Authorization",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                services.AddCors();
            });
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = ApplicationSamlConstants.Application;
                o.DefaultSignInScheme = ApplicationSamlConstants.External;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters()
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(this.Configuration["Jwt:ExpireInMinutes"])),
                   ValidIssuer = this.Configuration["Jwt:Issuer"],
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Jwt:Key"])),
               };
           })
           .AddCookie(Saml2Session)
           .AddSaml2(options =>
           {
               options.SPOptions.EntityId = new EntityId(this.Configuration["Saml:SPEntityId"]);
               options.IdentityProviders.Add(
                   new IdentityProvider(
                       new EntityId(this.Configuration["Saml:IDPEntityId"]), options.SPOptions)
                   {
                       LoadMetadata = true,
                   });
               options.SPOptions.ServiceCertificates.Add(new X509Certificate2(this.Configuration["Saml:CertificateFileName"]));
               //options.SPOptions.EntityId = new EntityId(this.Configuration["Saml:SPEntityId"]);
               //options.IdentityProviders.Add(
               //    new IdentityProvider(
               //        new EntityId(this.Configuration["Saml:IDPEntityId"]), options.SPOptions)
               //    {
               //        LoadMetadata = true,
               //    });
               //options.SPOptions.ServiceCertificates.Add(new X509Certificate2(this.Configuration["Saml:CertificateFileName"]));
           });

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            services.AddMvc((options) =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
                options.EnableEndpointRouting = false;
            })
            .AddDataAnnotationsLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
              //  c.InjectStylesheet("/swagger-ui/custom.css");
            });

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    template: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { format = "json" });
            });
        }
    }
}
