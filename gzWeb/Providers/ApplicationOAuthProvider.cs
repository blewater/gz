using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNet.Identity;
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
        private readonly TelemetryClient telemetry;

        public ApplicationOAuthProvider(string publicClientId, Func<ApplicationUserManager> userManagerFactory)
        {
            telemetry = new TelemetryClient();
            if (publicClientId == null) {
                var ex = new ArgumentNullException(nameof(publicClientId));

                telemetry.TrackException(ex);
                throw ex;
            }

            _publicClientId = publicClientId;
            _userManagerFactory = userManagerFactory;
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            try {
                _logger.Info("Authentication attempt for {0}", context.UserName);

                var userManager = _userManagerFactory();
                var user = await userManager.FindAsync(context.UserName, context.Password);

                if (user == null) {
                    _logger.Info(
                        "Authentication by username attempt for user '{0}' failed. Invalid username or password. Checking for email.",
                        context.UserName);

                    const string ERROR_TYPE = "invalid_grant";
                    const string ERROR_MSG = "The username/email or password is incorrect.";
                    user = userManager.FindByEmail(context.UserName);
                    if (user == null) {
                        context.SetError(ERROR_TYPE, ERROR_MSG);
                        _logger.Info("Authentication by email attempt for user '{0}' failed. Invalid email.",
                            context.UserName);
                        return;
                    }

                    user = userManager.CheckPassword(user, context.Password) ? user : null;
                    if (user == null) {
                        context.SetError(ERROR_TYPE, ERROR_MSG);
                        _logger.Info("Authentication password attempt for user '{0}' check failed. Invalid password.",
                            context.UserName);
                        return;
                    }
                }

                var oAuthIdentity = user.GenerateUserIdentity(userManager, OAuthDefaults.AuthenticationType);
                var cookiesIdentity =
                    user.GenerateUserIdentity(userManager, CookieAuthenticationDefaults.AuthenticationType);

                var properties = CreateProperties(user);

                user.LastLogin = DateTime.UtcNow;
                await userManager.UpdateAsync(user).ConfigureAwait(false);

                // TODO: in case of ...
                //if (!user.EmailConfirmed)
                //{
                //    context.SetError("invalid_grant", "Account pending approval.");
                //    return;
                //}

                var ticket = new AuthenticationTicket(oAuthIdentity, properties);
                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(cookiesIdentity);
            }
            catch (Exception ex) {

                telemetry.TrackException(ex);
            }
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
                                                       {"currency", user.Currency},
                                                       {"allowGzEmail", user.AllowGzEmail?.ToString() ?? ""},
                                                       {"allowGzSms", user.AllowGzSms?.ToString() ?? ""},
                                                       {"allow3rdPartySms", user.Allow3rdPartySms?.ToString() ?? ""},
                                                       {"acceptedGdprTc", user.AcceptedGdprTc?.ToString() ?? ""},
                                                       {"acceptedGdprPp", user.AcceptedGdprPp?.ToString() ?? ""},
                                                       {"acceptedGdpr3rdParties", user.AcceptedGdpr3rdParties?.ToString() ?? ""}
                                               };
            return new AuthenticationProperties(data);
        }
    }
}