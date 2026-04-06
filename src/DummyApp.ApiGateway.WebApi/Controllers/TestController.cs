using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DummyApp.ApiGateway.WebApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TestController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("testA")]
        public async Task<IActionResult> GetStorageTestA()
        {
            var httpClient = _httpClientFactory.CreateClient("storage");
            if (httpClient.BaseAddress == null)
            {
                return Problem("Storage service URL is not configured.");
            }

            var response = await httpClient.GetAsync("api/test/testA");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            return Content(content, "application/json");
        }

        [HttpGet("testX/{type}")]
        public async Task<IActionResult> GetStorageTestX(string type)
        {
            var scope = type?.ToUpperInvariant() switch
            {
                "R" => "storage.read",
                "W" => "storage.write",
                _ => null
            };

            if (scope is null)
            {
                return BadRequest(new { error = "Type must be 'R' or 'W'." });
            }

            var accessToken = await AcquireClientCredentialsTokenAsync(scope);
            if (accessToken is null)
            {
                return StatusCode(502, new { error = "Unable to acquire access token from identity." });
            }

            var httpClient = _httpClientFactory.CreateClient("storage");
            if (httpClient.BaseAddress == null)
            {
                return Problem("Storage service URL is not configured.");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("api/test/testX");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = "Storage service returned an error.", details = content });
            }

            return Content(content, "application/json");
        }

        [Authorize]
        [HttpGet("testX2/{type}")]
        public async Task<IActionResult> GetStorageTestX2(string type)
        {
            var scope = type?.ToUpperInvariant() switch
            {
                "R" => "storage.read",
                "W" => "storage.write",
                _ => null
            };

            if (scope is null)
            {
                return BadRequest(new { error = "Type must be 'R' or 'W'." });
            }

            // The user's identity was already validated by [Authorize] (JWT from Identity).
            // To call StorageService we still use client credentials (M2M token).
            var accessToken = await AcquireClientCredentialsTokenAsync(scope);
            if (accessToken is null)
            {
                return StatusCode(502, new { error = "Unable to acquire access token from identity." });
            }

            var httpClient = _httpClientFactory.CreateClient("storage");
            if (httpClient.BaseAddress == null)
            {
                return Problem("Storage service URL is not configured.");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("api/test/testX");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = "Storage service returned an error.", details = content });
            }

            // Enrich response with authenticated user info from the JWT.
            var userSub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst("name")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            return Ok(new
            {
                authenticatedAs = new { sub = userSub, name = userName },
                storageResponse = content
            });
        }

        private async Task<string?> AcquireClientCredentialsTokenAsync(string scope)
        {
            var identity = _configuration.GetSection("Identity");
            var metadataAddress = identity["MetadataAddress"]?.TrimEnd('/');
            var tokenEndpoint = !string.IsNullOrEmpty(metadataAddress) && metadataAddress.EndsWith("/.well-known/openid-configuration")
                ? metadataAddress[..^"/.well-known/openid-configuration".Length] + "/connect/token"
                : identity["Authority"]?.TrimEnd('/') + "/connect/token";

            if (string.IsNullOrEmpty(tokenEndpoint))
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = identity["ClientId"] ?? string.Empty,
                    ["client_secret"] = identity["ClientSecret"] ?? string.Empty,
                    ["scope"] = scope
                })
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.TryGetProperty("access_token", out var token)
                ? token.GetString()
                : null;
        }
    }
}
