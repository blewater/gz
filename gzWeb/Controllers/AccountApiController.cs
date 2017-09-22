using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using gzDAL.Repos.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using gzDAL.Conf;
using gzDAL.Models;
using gzWeb.Areas.Mvc.Models;
using gzWeb.Models;
using gzWeb.Utilities;
using System.Reflection;
using gzWeb.Providers;

namespace gzWeb.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountApiController : BaseApiController
    {
        private const string LocalLoginProvider = "Local";
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserPortfolioRepo _custPortfolioRepo;
        private readonly IUserRepo _userRepo;
        private readonly ICacheUserData _cacheUserData;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public AccountApiController(
            ApplicationUserManager userManager, 
            ApplicationDbContext dbContext, 
            IUserPortfolioRepo custPortfolioRepo,
            IUserRepo userRepo, 
            ICacheUserData cacheUserData)
                : base(userManager)
        {
            _dbContext = dbContext;
            _userRepo = userRepo;
            _custPortfolioRepo = custPortfolioRepo;
            _cacheUserData = cacheUserData;
        }

        #region AccessTokenFormat Constructor
        //public AccountApiController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        //{
        //    UserManager = userManager;
        //    AccessTokenFormat = accessTokenFormat;
        //}

        //public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }
        #endregion

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Logger.Info("Logout requested for [User#{0}]", User.Identity.GetUserId<int>());
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        #region TODO: UserInfo/ManageInfo

        //// GET api/Account/UserInfo
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("UserInfo")]
        //public UserInfoViewModel GetUserInfo()
        //{
        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    return new UserInfoViewModel
        //    {
        //        Email = User.Identity.GetUserName(),
        //        HasRegistered = externalLogin == null,
        //        LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
        //    };
        //}

        //// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        //[Route("ManageInfo")]
        //public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        //{
        //    IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

        //    foreach (IdentityUserLogin linkedAccount in user.Logins)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = linkedAccount.LoginProvider,
        //            ProviderKey = linkedAccount.ProviderKey
        //        });
        //    }

        //    if (user.PasswordHash != null)
        //    {
        //        logins.Add(new UserLoginInfoViewModel
        //        {
        //            LoginProvider = LocalLoginProvider,
        //            ProviderKey = user.UserName,
        //        });
        //    }

        //    return new ManageInfoViewModel
        //    {
        //        LocalLoginProvider = LocalLoginProvider,
        //        Email = user.UserName,
        //        Logins = logins,
        //        ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
        //    };
        //}

        #endregion

        // POST api/Account/ChangePassword
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserId<int>();
            IdentityResult result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                Logger.Error("ChangePassword for [User#{0}]. Failed with error: {1}", userId,
                             String.Join(Environment.NewLine, result.Errors));

                return GetErrorResult(result);
            }

            Logger.Info("ChangePassword for [User#{0}]. Succeeded.", userId);

            return Ok();
        }
        
        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserId<int>();
            IdentityResult result = await UserManager.AddPasswordAsync(userId, model.NewPassword);

            if (!result.Succeeded)
            {
                Logger.Error("SetPassword for [User#{0}]. Failed with error: {1}", userId,
                            String.Join(Environment.NewLine, result.Errors));

                return GetErrorResult(result);
            }

            Logger.Info("SetPassword for [User#{0}]. Succeeded.", userId);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null /*|| !await UserManager.IsEmailConfirmedAsync(user.Id)*/)
            {
                Logger.Error("User with email '{0}' not found.", model.Email);
                return BadRequest();
            }

            var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            Logger.Info("ForgotPassword for [User#{0}] with email:'{1}'. Succeeded.", user.Id, model.Email);

            return Ok(token);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Logger.Error("User with email '{0}' not found.", model.Email);
                return BadRequest();
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (!result.Succeeded)
            {
                Logger.Error("ResetPassword for [User#{0}] failed with error: {1}", user.Id,
                             String.Join(Environment.NewLine, result.Errors));
                return BadRequest();
            }

            Logger.Info("ResetPassword for [User#{0}]. Succeeded.", user.Id);

            return Ok();
        }

        #region TODO: ExternalLogin

        //// POST api/Account/AddExternalLogin
        //[Route("AddExternalLogin")]
        //public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //        && ticket.Properties.ExpiresUtc.HasValue
        //        && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
        //        new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// POST api/Account/RemoveLogin
        //[Route("RemoveLogin")]
        //public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityResult result;

        //    if (model.LoginProvider == LocalLoginProvider)
        //    {
        //        result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
        //    }
        //    else
        //    {
        //        result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
        //            new UserLoginInfo(model.LoginProvider, model.ProviderKey));
        //    }

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogin
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[AllowAnonymous]
        //[Route("ExternalLogin", Name = "ExternalLogin")]
        //public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        //{
        //    if (error != null)
        //    {
        //        return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
        //    }

        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    if (externalLogin == null)
        //    {
        //        return InternalServerError();
        //    }

        //    if (externalLogin.LoginProvider != provider)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        return new ChallengeResult(provider, this);
        //    }

        //    ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

        //    bool hasRegistered = user != null;

        //    if (hasRegistered)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //        ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager, OAuthDefaults.AuthenticationType);
        //        ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager, CookieAuthenticationDefaults.AuthenticationType);

        //        AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
        //        Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        //    }
        //    else
        //    {
        //        IEnumerable<Claim> claims = externalLogin.GetClaims();
        //        var identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
        //        Authentication.SignIn(identity);
        //    }

        //    return Ok();
        //}

        //// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        //[AllowAnonymous]
        //[Route("ExternalLogins")]
        //public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //{
        //    IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
        //    List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

        //    string state;

        //    if (generateState)
        //    {
        //        const int strengthInBits = 256;
        //        state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //    }
        //    else
        //    {
        //        state = null;
        //    }

        //    foreach (AuthenticationDescription description in descriptions)
        //    {
        //        ExternalLoginViewModel login = new ExternalLoginViewModel
        //        {
        //            Name = description.Caption,
        //            Url = Url.Route("ExternalLogin", new
        //            {
        //                provider = description.AuthenticationType,
        //                response_type = "token",
        //                client_id = Startup.PublicClientId,
        //                redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //                state = state
        //            }),
        //            State = state
        //        };
        //        logins.Add(login);
        //    }

        //    return logins;
        //}

        //// POST api/Account/RegisterExternal
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("RegisterExternal")]
        //public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var info = await Authentication.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return InternalServerError();
        //    }

        //    var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

        //    IdentityResult result = await UserManager.CreateAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }
        //    return Ok();
        //}
        #endregion

        // POST api/Account/Register
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            Logger.Info("Server Registration Step 1:Register requested for [User - {0}/{1}].", model.Username, model.Email);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
                       {
                               UserName = model.Username,
                               Email = model.Email,
                               FirstName = model.FirstName,
                               LastName = model.LastName,
                               Birthday = model.Birthday,
                               Currency = model.Currency,

                               EmailConfirmed = true,
                       };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                Logger.Error("Server Registration Step 1:gzRegistration for User with username: '{0}' and email: '{1}' failed with error: {2}",
                             model.Username,
                             model.Email,
                             String.Join(Environment.NewLine, result.Errors));
                var deleteResult = await UserManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    Logger.Warn(
                            "Server Registration Step 1:Failed to delete User of unsuccessful gzRegistration with username: '{0}' and email: '{1}', with error: {2}",
                            model.Username,
                            model.Email,
                            String.Join(Environment.NewLine, result.Errors));
                }
                else
                {
                    Logger.Info(
                            "Server Registration Step 1:Delete of unsuccessful user gzRegistration with username: '{0}' and email: '{1}', succeeded.",
                            model.Username, model.Email);
                }
                return GetErrorResult(result);
            }

            #region In case of email confirmation ...
            //var activationCode = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //var callbackUrl = Url.Link("MvcRoute",
            //                           new
            //                           {
            //                                   Controller = "Auth",
            //                                   Action = "Activate",
            //                                   userId = user.Id,
            //                                   code = activationCode
            //                           });
            //callbackUrl += "&key=";
            //return Ok(callbackUrl);
            #endregion

            Logger.Info("Server Registration Step 1:gzRegistration for [User#{0} - {1}]. Succeeded.", user.Id, model.Username);

            return Ok(result);
        }

        [HttpPost]
        [Route("RevokeRegistration")]
        public async Task<IHttpActionResult> RevokeRegistration()
        {
            var userId = User.Identity.GetUserId<int>();
            
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return Ok("User not found!");
            }

            var result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                Logger.Error("RevokeRegistration for [User#{0}] failed with error: {1}", userId,
                             String.Join(Environment.NewLine, result.Errors));
                return GetErrorResult(result);
            }

            Logger.Info("RevokeRegistration for [User#{0}]. Succeeded.", userId);


            return Ok(result);
        }

        [HttpPost]
        [Route("FinalizeRegistration")]
        public async Task<IHttpActionResult> FinalizeRegistration(int gmUserId)
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found. [User#{0}]", userId);
                return Ok("User not found!");
            }

            try
            {
                await _custPortfolioRepo.SetDbDefaultPorfolioAddGmUserId(user.Id, gmUserId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Server Registration Step 6: FinalizeRegistration for [User#{0}]. Failed.", userId);
                throw;
            }

            Logger.Info("Server Registration Step 6: GzFinalizeRegistration for [User#{0}]. Succeeded.", userId);

            return Ok();
        }

        #region In case of email confirmation ...
        [AllowAnonymous]
        [HttpPost]
        [Route("Activate")]
        public async Task<IHttpActionResult> Activate(ActivationBindingModel model)
        {
            Logger.Info("Activate requested for [User#{0}] with code: {1}.", model.UserId, model.Code);

            if (!ModelState.IsValid)
            {
                Logger.Error("ModelState is invalid.");
                return BadRequest(ModelState);
            }

            if (model.UserId == default(int) || string.IsNullOrEmpty(model.Code))
            {
                Logger.Error("UserId is 0 and/or Code is null or empty");
                return BadRequest();
            }

            var result = await UserManager.ConfirmEmailAsync(model.UserId, model.Code);
            if (!result.Succeeded)
                Logger.Error("UserManager.ConfirmEmailAsync failed. {0}",
                             String.Join(Environment.NewLine, result.Errors));

            return Ok(result.Succeeded ? "Ok" : "Error");
        }
        #endregion

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        #region ExternalLoginData
        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator Random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");

                var strengthInBytes = strengthInBits / bitsPerByte;

                var data = new byte[strengthInBytes];
                Random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }
        #endregion

        #endregion
        
        #region GetDeploymentInfo
        private static bool IsInDebugMode()
        {
#if DEBUG
                return true;
#else
                return false;
#endif
        }

        private string GetVersion()
        {
            var assembly = Assembly.GetAssembly(typeof(ApplicationOAuthProvider));
            var assemblyName = assembly.GetName().Name;
            var gitVersionInformationType = assembly.GetType(assemblyName + ".GitVersionInformation");
            var versionField = gitVersionInformationType.GetField("InformationalVersion");
            return versionField.GetValue(null).ToString();
        }

        private string GetReCaptchaSiteKey()
        {
            var host = Request.RequestUri.Host;
            if (host == "localhost")
                host = "greenzorro.azurewebsites.net";
            var appKey = "ReCaptchaSiteKey@" + host;
            var reCaptchaSiteKey = System.Configuration.ConfigurationManager.AppSettings[appKey];
            return reCaptchaSiteKey;
        }

        //private bool IsLive()
        //{
        //    return Request.RequestUri.Host == "www.greenzorro.com";
        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("GetDeploymentInfo")]
        // 10 minutes caching
        [System.Web.Mvc.OutputCache(Duration = 6000, VaryByParam = "none", NoStore = false)]
        public IHttpActionResult GetDeploymentInfo()
        {
            return OkMsg(new
            {
                Debug = IsInDebugMode(),
                Version = GetVersion(),
                ReCaptchaSiteKey = GetReCaptchaSiteKey()
                //Live = IsLive()
            });
        }
        #endregion

        // GET api/Account/CacheUserData
        [HttpPost]
        [Route("CacheUserData")]
        public async Task<IHttpActionResult> CacheUserData()
        {
            var userId = User.Identity.GetUserId<int>();
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}", userId);
                return Ok("User not found!");
            }

            await _cacheUserData.Query(user.Id);

            return Ok();
        }
    }
}
