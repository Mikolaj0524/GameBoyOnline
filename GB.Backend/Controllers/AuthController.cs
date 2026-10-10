
using GB.Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace GB.Backend.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthController(IConfiguration configuration, IMemoryCache cache) : ControllerBase
	{
		private readonly IConfiguration _configuration = configuration;
		private readonly IMemoryCache _cache = cache;

		/// <summary>Generates a temporary authentication challenge.</summary>
		/// <returns>The generated challenge.</returns>
		[HttpPost("challenge")]
		public IActionResult GetChallenge()
		{
			string challenge = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
			_cache.Set($"challenge:{challenge}", true, TimeSpan.FromMinutes(2));
			return Ok(new { challenge });
		}

		/// <summary>Verifies the response and generates an access token.</summary>
		/// <returns>A token with the assigned access role.</returns>
		[HttpPost("verify")]
		public IActionResult Verify([FromBody] VerifyRequestDto request)
		{
			if (string.IsNullOrWhiteSpace(request.Challenge) || string.IsNullOrWhiteSpace(request.Response))
				return BadRequest();

			string challengeKey = $"challenge:{request.Challenge}";
			if (!_cache.TryGetValue(challengeKey, out _))
				return Unauthorized();

			_cache.Remove(challengeKey);

			var keys = _configuration.GetSection("AccessKeys").Get<List<AccessKeyConfig>>();

			if (keys == null || keys.Count == 0)
				return StatusCode(500, "No access keys configured.");

			try
			{
				byte[] challenge = Convert.FromBase64String(request.Challenge);
				byte[] received = Convert.FromBase64String(request.Response);

				foreach (var key in keys)
				{
					if (string.IsNullOrWhiteSpace(key.Code) || (key.Role != "Normal" && key.Role != "Special"))
						continue;

					using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key.Code));
					byte[] expected = hmac.ComputeHash(challenge);

					if (received.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(expected, received))
						continue;

					string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
					_cache.Set($"token:{token}", key.Role, TimeSpan.FromMinutes(15));

					return Ok(new { token, role = key.Role });
				}

				return Unauthorized();
			}
			catch (FormatException)
			{
				return BadRequest();
			}
		}

		/// <summary>Validates the authentication token.</summary>
		/// <returns>The token role if valid.</returns>
		[HttpGet("validate")]
		public IActionResult Validate()
		{
			string? role = GetTokenRole();
			return role == null ? Unauthorized() : Ok(new { role });
		}

		/// <summary>Returns the role assigned to the Bearer token.</summary>
		private string? GetTokenRole()
		{
			string auth = Request.Headers.Authorization.ToString();

			if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				return null;

			string token = auth["Bearer ".Length..].Trim();

			if (token.Length == 0 || !_cache.TryGetValue($"token:{token}", out string? role))
				return null;

			return role;
		}
	}

	public class AccessKeyConfig
	{
		public string Code { get; set; } = "";
		public string Role { get; set; } = "Normal";
	}
}
