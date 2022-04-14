using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Xml.Linq;
using SSOTestApp.Models;

namespace SSOTestApp.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        [Route("Login/{scheme}")]
        public IActionResult Login(string scheme)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("ExternalLoginCallback"),

            };

            return Challenge(props, scheme);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>

        [Route("Login/ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var httpRequest = Request.Form["SAMLResponse"];
            // read external identity from the temporary cookie
            //var nameIdentifier = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single();
            var samlResponseXml = Encoding.UTF8.GetString(Convert.FromBase64String(httpRequest));
            var nameId = ReadSamlResponce(samlResponseXml);

            var d = new SAMLProcessor(samlResponseXml);
            return Redirect("~/");
        }

        [Route("Logout/{scheme}")]
        public async Task<IActionResult> Logout(string scheme)
        {
            await HttpContext.SignOutAsync(scheme);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private XElement ReadSamlResponce(string samlResponce)
        {
            XDocument xmlDoc = XDocument.Parse(samlResponce);
            var nameId = (from element in xmlDoc.Descendants()
                    .Where(x => x.Name.LocalName.Contains("NameID"))
                          select element).ToList().FirstOrDefault();

            return nameId;
        }
    }
}