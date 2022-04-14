using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SSOTestApp.Models
{
    public class HomeViewModel
    {
        public AuthenticateResult Saml2Authentication { get; set; }
        public AuthenticateResult OidcAuthentication { get; internal set; }
    }
}
