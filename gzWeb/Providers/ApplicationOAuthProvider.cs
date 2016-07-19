using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using NLog;

namespace gzWeb.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly Func<ApplicationUserManager> _userManagerFactory;
        private readonly string _publicClientId;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ApplicationOAuthProvider(string publicClientId, Func<ApplicationUserManager> userManagerFactory)
        {
            if (publicClientId == null)
                throw new ArgumentNullException("publicClientId");

            _publicClientId = publicClientId;
            _userManagerFactory = userManagerFactory;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            _logger.Info("Authentication attempt for {0}", context.UserName);

            var userManager = _userManagerFactory();
            var user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                _logger.Info("Authentication attempt for user '{0}' failed. Invalid username or password. Checking for email.", context.UserName);

                const string ERROR_TYPE = "invalid_grant";
                const string ERROR_MSG = "The username/email or password is incorrect.";
                user = await userManager.FindByEmailAsync(context.UserName);
                if (user == null)
                {
                    context.SetError(ERROR_TYPE, ERROR_MSG);
                    _logger.Info("Authentication attempt for user '{0}' failed. Invalid email.", context.UserName);
                    return;
                }

                user = await userManager.CheckPasswordAsync(user, context.Password) ? user : null;
                if (user == null)
                {
                    context.SetError(ERROR_TYPE, ERROR_MSG);
                    _logger.Info("Authentication attempt for user '{0}' failed. Invalid password.", context.UserName);
                    return;
                }
            }

            // TODO: in case of ...
            //if (!user.EmailConfirmed)
            //{
            //    context.SetError("invalid_grant", "Account pending approval.");
            //    return;
            //}

            var oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType);
            var cookiesIdentity = await user.GenerateUserIdentityAsync(userManager, CookieAuthenticationDefaults.AuthenticationType);

            var properties = CreateProperties(user);
            var ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                var expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(ApplicationUser user)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
                                               {
                                                       {"userName", user.UserName},
                                                       {"firstname", user.FirstName},
                                                       {"lastname", user.LastName},
                                                       {"currency", user.Currency}
                                               };
            return new AuthenticationProperties(data);
        }
    }
}