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

		[HttpPost("challenge")]
		public IActionResult GetChallenge()
		{
			byte[] challengeBytes = RandomNumberGenerator.GetBytes(32);
			string challenge = Convert.ToBase64String(challengeBytes);

			_cache.Set($"challenge:{challenge}", true, TimeSpan.FromMinutes(2));

			return Ok(new
			{
				challenge
			});
		}

		[HttpPost("verify")]
		public IActionResult Verify([FromBody] VerifyRequestDto request)
		{
			if (string.IsNullOrWhiteSpace(request.Challenge) || string.IsNullOrWhiteSpace(request.Response))
				return BadRequest();

			string challengeKey = $"challenge:{request.Challenge}";

			if (!_cache.TryGetValue(challengeKey, out _))
				return Unauthorized();

			_cache.Remove(challengeKey);

			string[]? codes = _configuration.GetSection("AccessCodes").Get<string[]>();

			if (codes == null || codes.Length == 0)
				return StatusCode(500);

			try
			{
				byte[] challenge = Convert.FromBase64String(request.Challenge);
				byte[] received = Convert.FromBase64String(request.Response);

				foreach (string code in codes)
				{
					byte[] key = Encoding.UTF8.GetBytes(code);

					using var hmac = new HMACSHA256(key);

					byte[] expected = hmac.ComputeHash(challenge);

					if (!CryptographicOperations.FixedTimeEquals(expected, received))
						continue;

					byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
					string token = Convert.ToBase64String(tokenBytes);

					_cache.Set($"token:{token}", true, TimeSpan.FromMinutes(15));

					return Ok(new
					{
						token
					});
				}

				return Unauthorized();
			}
			catch (FormatException)
			{
				return BadRequest();
			}
		}

		[HttpGet("validate")]
		public IActionResult Validate()
		{
			string? authorization = Request.Headers.Authorization.ToString();

			if (string.IsNullOrWhiteSpace(authorization))
				return Unauthorized();

			if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				return Unauthorized();

			string token = authorization["Bearer ".Length..].Trim();

			if (string.IsNullOrWhiteSpace(token))
				return Unauthorized();

			if (!_cache.TryGetValue($"token:{token}", out _))
				return Unauthorized();

			return Ok();
		}
	}
}