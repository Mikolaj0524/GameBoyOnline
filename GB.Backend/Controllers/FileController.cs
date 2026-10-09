using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using GB.Backend.Dtos;

namespace GB.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController(IMemoryCache cache, IWebHostEnvironment environment) : ControllerBase
{
	private readonly string filesPath = Path.Combine(environment.ContentRootPath, "Files");

	[HttpGet("getlist")]
	public IActionResult GetList(string type)
	{
		if (!IsAuthorized()) 
			return Unauthorized();

		if(type != "rom" && type != "bios")
			return BadRequest("Type must be 'rom' or 'bios'.");

		string dir = Path.Combine(filesPath, type);
		string[] files = Directory.GetFiles(dir).Where(f => !Path.GetFileName(f).StartsWith('.')).ToArray();
		var result = files.Select((file, id) => new FileDto
		{
			Id = id,
			Name = Path.GetFileNameWithoutExtension(file),
			Ext = Path.GetExtension(file),
			Type = type
		});

		return Ok(result);
	}

	[HttpGet("file/{type}/{id:int}")]
	public IActionResult GetFile(string type, int id)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (type != "rom" && type != "bios")
			return BadRequest("Type must be 'rom' or 'bios'.");

		string dir = Path.Combine(filesPath, type);
		string[] files = Directory.GetFiles(dir).Where(f => !Path.GetFileName(f).StartsWith('.')).ToArray();
		if (id < 0 || id >= files.Length)
			return NotFound();

		string path = files[id];
		return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
	}

	private bool IsAuthorized()
	{
		string auth = Request.Headers.Authorization.ToString();
		if (!auth.StartsWith("Bearer "))
			return false;

		string token = auth["Bearer ".Length..].Trim();
		return token.Length > 0 && cache.TryGetValue($"token:{token}", out _);
	}
}