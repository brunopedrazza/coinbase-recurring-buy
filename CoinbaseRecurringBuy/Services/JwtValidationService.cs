using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using CoinbaseRecurringBuy.Models.Configuration;

namespace CoinbaseRecurringBuy.Services;

public class JwtValidationService
{
    private readonly ILogger<JwtValidationService> _logger;
    private readonly AzureAdB2cConfig _azureAdB2cConfig;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    public JwtValidationService(ILogger<JwtValidationService> logger, IOptions<AzureAdB2cConfig> b2cConfig)
    {
        _logger = logger;
        _azureAdB2cConfig = b2cConfig.Value;

        // Set up the OpenID Connect configuration manager for Azure AD B2C
        string metadataAddress = $"https://{_azureAdB2cConfig.Tenant}.b2clogin.com/{_azureAdB2cConfig.Tenant}.onmicrosoft.com/{_azureAdB2cConfig.PolicyName}/v2.0/.well-known/openid-configuration";
        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever());
    }

    private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token is null or empty");
                return null;
            }

            var openIdConfig = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{_azureAdB2cConfig.Tenant}.b2clogin.com/{_azureAdB2cConfig.TenantId}/v2.0/",
                ValidateAudience = true,
                ValidAudience = _azureAdB2cConfig.Audience,
                ValidateLifetime = true,
                IssuerSigningKeys = openIdConfig.SigningKeys
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, validationParameters, out var validatedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return null;
        }
    }

    private bool IsUserAuthorized(ClaimsPrincipal? principal)
    {
        if (principal == null)
        {
            return false;
        }

        var userEmail = principal.Claims.FirstOrDefault(c => c.Type == "emails")?.Value;
        
        if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(_azureAdB2cConfig.AuthorizedUsers))
        {
            return false;
        }

        var authorizedUsers = _azureAdB2cConfig.AuthorizedUsers.Split(',').Select(email => email.Trim().ToLower());
        return authorizedUsers.Contains(userEmail.ToLower());
    }

    public async Task<(bool IsAuthenticated, bool IsAuthorized, ClaimsPrincipal? Principal)> ValidateAndAuthorizeAsync(string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            _logger.LogWarning("Invalid authorization header format");
            return (false, false, null);
        }

        string token = authHeader["Bearer ".Length..];
        
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Token is empty");
            return (false, false, null);
        }
        
        var principal = await ValidateTokenAsync(token);
        
        if (principal == null)
        {
            _logger.LogWarning("Token validation failed");
            return (false, false, null);
        }

        bool isAuthorized = IsUserAuthorized(principal);
        
        if (!isAuthorized)
        {
            var userEmail = principal.Claims.FirstOrDefault(c => c.Type == "emails")?.Value;
            _logger.LogWarning("User {Email} is not authorized", userEmail);
        }
        
        return (true, isAuthorized, principal);
    }
} 