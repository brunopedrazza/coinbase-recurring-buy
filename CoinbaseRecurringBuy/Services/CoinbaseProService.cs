using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Microsoft.Extensions.Logging;
using CoinbaseRecurringBuy.Models.Configuration;
using CoinbaseRecurringBuy.Models.Coinbase;

namespace CoinbaseRecurringBuy.Services;

public class CoinbaseProService(
    HttpClient httpClient,
    IOptions<RecurringBuyConfig> config,
    ILogger<CoinbaseProService> logger)
{
    private readonly RecurringBuyConfig _config = config.Value;
    private const string ApiUrl = "api.coinbase.com/api/v3/brokerage";

    public async Task PlaceMarketOrderAsync(string symbol, decimal usdcAmount)
    {
        const string requestPath = "/orders";
        
        var order = new CoinbaseOrder
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = $"{symbol}-USDC",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc
                {
                    QuoteSize = usdcAmount.ToString("0.00", CultureInfo.InvariantCulture)
                }
            }
        };

        var jsonContent = JsonSerializer.Serialize(order);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
        var jwt = GenerateToken(_config.CoinbaseApiName, _config.CoinbaseApiPrivateKey, $"POST {ApiUrl}{requestPath}");
        
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await httpClient.PostAsync($"https://{ApiUrl}{requestPath}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to place order: {responseContent}");
        }

        var orderResponse = JsonSerializer.Deserialize<CoinbaseOrderResponse>(responseContent);
        if (orderResponse?.Success == false)
        {
            var error = orderResponse.ErrorResponse;
            var errorMessage = $"Order failed: {error?.Error} - {error?.Message}";
            if (!string.IsNullOrEmpty(error?.ErrorDetails))
            {
                errorMessage += $" Details: {error.ErrorDetails}";
            }
            if (!string.IsNullOrEmpty(error?.PreviewFailureReason))
            {
                errorMessage += $" Preview Failure: {error.PreviewFailureReason}";
            }
            throw new InvalidOperationException(errorMessage);
        }

        logger.LogInformation("Order response: {Content}", responseContent);
    }

    private static string GenerateToken(string name, string privateKeyPem, string uri)
    {
        // Load EC private key using BouncyCastle
        var ecPrivateKey = LoadEcPrivateKeyFromPem(privateKeyPem);

        // Create security key from the manually created ECDsa
        var ecdsa = GetECDsaFromPrivateKey(ecPrivateKey);
        var securityKey = new ECDsaSecurityKey(ecdsa);

        // Signing credentials
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

        var now = DateTimeOffset.UtcNow;

        // Header and payload
        var header = new JwtHeader(credentials)
        {
            ["kid"] = name,
            ["nonce"] = GenerateNonce() // Generate dynamic nonce
        };

        var payload = new JwtPayload
        {
            { "iss", "coinbase-cloud" },
            { "sub", name },
            { "nbf", now.ToUnixTimeSeconds() },
            { "exp", now.AddMinutes(2).ToUnixTimeSeconds() },
            { "uri", uri }
        };

        var token = new JwtSecurityToken(header, payload);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    // Method to generate a dynamic nonce
    private static string GenerateNonce(int length = 64)
    {
        var nonceBytes = new byte[length / 2]; // Allocate enough space for the desired length (in hex characters)
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonceBytes);
        }
        return BitConverter.ToString(nonceBytes).Replace("-", "").ToLower(); // Convert byte array to hex string
    }

    // Method to load EC private key from PEM using BouncyCastle
    private static ECPrivateKeyParameters LoadEcPrivateKeyFromPem(string privateKeyPem)
    {
        using var stringReader = new StringReader(privateKeyPem);
        var pemReader = new PemReader(stringReader);
        if (pemReader.ReadObject() is not AsymmetricCipherKeyPair keyPair)
            throw new InvalidOperationException("Failed to load EC private key from PEM");

        return (ECPrivateKeyParameters)keyPair.Private;
    }

    // Method to convert ECPrivateKeyParameters to ECDsa
    private static ECDsa GetECDsaFromPrivateKey(ECPrivateKeyParameters privateKey)
    {
        var q = privateKey.Parameters.G.Multiply(privateKey.D).Normalize();
        var qx = q.AffineXCoord.GetEncoded();
        var qy = q.AffineYCoord.GetEncoded();

        var ecdsaParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256, // Adjust if you're using a different curve
            Q =
            {
                X = qx,
                Y = qy
            },
            D = privateKey.D.ToByteArrayUnsigned()
        };

        return ECDsa.Create(ecdsaParams);
    }
} 