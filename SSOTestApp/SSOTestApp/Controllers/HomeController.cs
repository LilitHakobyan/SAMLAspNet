using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SSOTestApp.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using SSOTestApp.Constants;

namespace SSOTestApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
           // var authenticateResult = await HttpContext.AuthenticateAsync(ApplicationSamlConstants.Saml2);
            
            var model = new HomeViewModel
            {
                Saml2Authentication = await HttpContext.AuthenticateAsync(Startup.saml2) ,
                OidcAuthentication = await HttpContext.AuthenticateAsync(Startup.OidcSession)
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
