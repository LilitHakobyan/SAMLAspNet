using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ITfoxtec.Identity.Saml2.Schemas;

namespace SAML2SP
{
    public static class Saml2ServiceCollectionExtensions
    {
        /// <summary>
        /// Add SAML 2.0 configuration.
        /// </summary>
        public static IServiceCollection AddSaml2(this IServiceCollection services, string loginPath = "/Auth/Login", bool slidingExpiration = false, string accessDeniedPath = null, ITicketStore sessionStore = null, SameSiteMode cookieSameSite = SameSiteMode.Lax, string cookieDomain = null, CookieSecurePolicy cookieSecurePolicy = CookieSecurePolicy.SameAsRequest)
        {
            services.AddAuthentication(Saml2Constants.AuthenticationScheme)
                .AddCookie(Saml2Constants.AuthenticationScheme, o =>
                {
                    o.Cookie.Name = "SAMLCookie";
                    o.LoginPath = new PathString(loginPath);
                    o.SlidingExpiration = slidingExpiration;
                    if (!string.IsNullOrEmpty(accessDeniedPath))
                    {
                        o.AccessDeniedPath = new PathString(accessDeniedPath);
                    }
                    if (sessionStore != null)
                    {
                        o.SessionStore = sessionStore;
                    }
                    o.Cookie.SameSite = cookieSameSite;
                    o.Cookie.SecurePolicy = cookieSecurePolicy;
                    if (!string.IsNullOrEmpty(cookieDomain))
                    {
                        o.Cookie.Domain = cookieDomain;
                    }
                });

            return services;
        }
    }
}
