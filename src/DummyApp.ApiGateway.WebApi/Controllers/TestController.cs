using DummyApp.ApiGateway.WebApi.Services;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.ApiGateway.WebApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IClientCredentialsTokenCache _tokenCache;

        public TestController(IHttpClientFactory httpClientFactory, IClientCredentialsTokenCache tokenCache)
        {
            _httpClientFactory = httpClientFactory;
            _tokenCache = tokenCache;
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

            var accessToken = await _tokenCache.GetTokenAsync(scope);
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

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token may have been revoked — clear the cache and retry once.
                _tokenCache.Invalidate(scope);
                accessToken = await _tokenCache.GetTokenAsync(scope);
                if (accessToken is null)
                    return StatusCode(502, new { error = "Unable to re-acquire access token from identity." });

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                response = await httpClient.GetAsync("api/test/testX");
                content = await response.Content.ReadAsStringAsync();
            }

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
            var accessToken = await _tokenCache.GetTokenAsync(scope);
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
    }
}
