using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using GB.Backend.Dtos;

namespace GB.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController(IMemoryCache cache, IWebHostEnvironment environment) : ControllerBase
{
	private readonly string _filesPath = Path.Combine(environment.ContentRootPath, "Files");


	/// <summary> Returns the list of ROM or BIOS files. </summary>
	/// <returns> List of files </returns>
	[HttpGet("getlist")]
	public IActionResult GetList(string type)
	{
		if (!IsAuthorized()) 
			return Unauthorized();

		if(type != "rom" && type != "bios")
			return BadRequest("Type must be 'rom' or 'bios'.");

		string dir = Path.Combine(_filesPath, type);
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

	/// <summary> Returns the contents of a ROM or BIOS file. </summary>
	/// <returns> The requested file as a binary download. </returns>
	[HttpGet("file/{type}/{id:int}")]
	public IActionResult GetFile(string type, int id)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (type != "rom" && type != "bios")
			return BadRequest("Type must be 'rom' or 'bios'.");

		string dir = Path.Combine(_filesPath, type);
		string[] files = Directory.GetFiles(dir).Where(f => !Path.GetFileName(f).StartsWith('.')).ToArray();
		if (id < 0 || id >= files.Length)
			return NotFound();

		string path = files[id];
		return PhysicalFile(path, "application/octet-stream", Path.GetFileName(path));
	}


	/// <summary> Checks bearer token. </summary>
	/// <returns> True if bearer token is valid. </returns>
	private bool IsAuthorized()
	{
		string auth = Request.Headers.Authorization.ToString();
		if (!auth.StartsWith("Bearer "))
			return false;

		string token = auth["Bearer ".Length..].Trim();
		return token.Length > 0 && cache.TryGetValue($"token:{token}", out _);
	}
}