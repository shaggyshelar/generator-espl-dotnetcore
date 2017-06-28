using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using AppConfig;
using Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.Domain;

namespace WebApi.Providers
{
    public interface ITokenProvider
    {
        object GenerateToken(string userName, string password);
    }

    public class TokenProvider : ITokenProvider
    {
        private readonly HttpContext _httpContext;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger _logger;

        public TokenProvider(
            IHttpContextAccessor httpContextAccessor, 
            IApplicationUserService applicationUserService,
            ILoggerFactory loggerFactory)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _applicationUserService = applicationUserService;
            _logger = loggerFactory.CreateLogger<TokenProvider>();
        }

        public object GenerateToken(string userName, string password)
        {
            // Make sure the incoming parameters are valid
            if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
                return null;

            var claimsIdentity = GetClaimsIdentity(userName, password);
            // If no identity was found, the login is invalid
            if (claimsIdentity == null)
                return null;

            // Make sure we have valid token provider options
            ThrowIfInvalidOptions(OptionsStore.TokenProviderOptions);

            // If we're still here: We found a user, create the token object
            var now = DateTime.UtcNow;
            var jtiTask = OptionsStore.TokenProviderOptions.NonceGenerator();
            jtiTask.Wait();

            // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var jwtClaims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, jtiTask.Result),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            };

            // Combine the user's claims (roles and stuff) with the JWT claims
            var allClaims = claimsIdentity.Claims.Union(jwtClaims);

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: OptionsStore.TokenProviderOptions.Issuer,
                audience: OptionsStore.TokenProviderOptions.Audience,
                claims: allClaims,
                notBefore: now,
                expires: now.Add(OptionsStore.TokenProviderOptions.Expiration),
                signingCredentials: OptionsStore.TokenProviderOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var roles = claimsIdentity.Claims
                .Where(x => x.Type.Contains("claims/role"))
                .Select(x => x.Value);

            var token = new
            {
                access_token = encodedJwt,
                expires_in = (int)OptionsStore.TokenProviderOptions.Expiration.TotalSeconds,
                userName = claimsIdentity.Name,
                token_type = "JWT",
                roles = roles
            };

            return token;
        }

        private ApplicationUser GetCurrentApplicationUser(string userName)
        {
            return _applicationUserService.FindByNameAsync(userName);
        }

        private ClaimsIdentity GetClaimsIdentity(string userName, string password)
        {
            var user = GetCurrentApplicationUser(userName);
            if (user != null)
            {
                var signInTask = _applicationUserService.PasswordSignInAsync(userName, password, false, true);
                signInTask.Wait();

                if (signInTask.Result.Succeeded)
                {
                    _logger.LogInformation($"User {user.Id} logged in");

                    var rolesTask = _applicationUserService.GetRolesAsync(user);
                    rolesTask.Wait();

                    var claims = new List<Claim>();
                    foreach (var role in rolesTask.Result)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, OptionsStore.TokenProviderOptions.Issuer));
                    }

                    return new ClaimsIdentity(new System.Security.Principal.GenericIdentity(userName, "Token"), claims);
                }
            }

            _logger.LogInformation($"Invalid login with user {userName}.");

            return null;
        }

        private void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            //if (options.IdentityResolver == null)
            //{
            //    throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            //}

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }
        }

        public static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
