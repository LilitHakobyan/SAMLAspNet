using System;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace SAMP2Idp.Models
{
    public class LoginRequestSAML
    {
        public Saml2Id Saml2Id { get; set; }
        public string Issuer { get; set; }

        public Uri SingleSignOnDestination { get; set; }
        public string RelayState { get; set; }
    }
}
