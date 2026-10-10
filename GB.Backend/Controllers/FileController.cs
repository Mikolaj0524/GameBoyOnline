
using GB.Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using System.Data;

namespace GB.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController(IConfiguration configuration, IMemoryCache cache, IWebHostEnvironment environment) : ControllerBase
{
	private readonly IConfiguration _configuration = configuration;
	private readonly IMemoryCache _cache = cache;
	private readonly string _filesPath = Path.Combine(environment.ContentRootPath, "Files");

	/// <summary>Returns the list of ROM or BIOS files.</summary>
	/// <returns>List of files.</returns>
	[HttpGet("getlist")]
	public async Task<IActionResult> GetList([FromQuery] string type)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (!IsValidType(type))
			return BadRequest("Type must be 'rom' or 'bios'.");

		if (DatabaseEnabled())
		{
			try
			{
				await using var connection = await OpenConnection();
				await using var command = new MySqlCommand("SELECT Id, Name, Ext, Type FROM Files WHERE Type = @type ORDER BY Id", connection);
				command.Parameters.AddWithValue("@type", type);

				await using var reader = await command.ExecuteReaderAsync();
				var result = new List<FileDto>();

				while (await reader.ReadAsync())
					result.Add(new FileDto { Id = Convert.ToInt32(reader["Id"]), Name = reader["Name"].ToString()!, Ext = reader["Ext"].ToString()!, Type = reader["Type"].ToString()! });

				return Ok(result);
			}
			catch (MySqlException)
			{
				return StatusCode(500, "Database error.");
			}
		}

		string dir = Path.Combine(_filesPath, type);

		if (!Directory.Exists(dir))
			return Ok(Array.Empty<FileDto>());

		string[] files = GetLocalFiles(dir);

		return Ok(files.Select((file, id) => new FileDto { Id = id, Name = Path.GetFileNameWithoutExtension(file), Ext = Path.GetExtension(file), Type = type }));
	}

	/// <summary>Returns the contents of a ROM or BIOS file.</summary>
	/// <returns>The requested file as a binary download.</returns>
	[HttpGet("file/{type}/{id:int}")]
	public async Task<IActionResult> GetFile(string type, int id)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (!IsValidType(type))
			return BadRequest("Type must be 'rom' or 'bios'.");

		if (id < 0)
			return NotFound();

		if (DatabaseEnabled())
		{
			try
			{
				await using var connection = await OpenConnection();
				await using var command = new MySqlCommand("SELECT Name, Ext, Data FROM Files WHERE Id = @id AND Type = @type LIMIT 1", connection);
				command.Parameters.AddWithValue("@id", id);
				command.Parameters.AddWithValue("@type", type);

				await using var reader = await command.ExecuteReaderAsync();

				if (!await reader.ReadAsync())
					return NotFound();

				string name = reader["Name"].ToString()!;
				string ext = reader["Ext"].ToString()!;
				byte[] data = (byte[])reader["Data"];

				return File(data, "application/octet-stream", name + ext);
			}
			catch (MySqlException)
			{
				return StatusCode(500, "Database error.");
			}
		}

		string dir = Path.Combine(_filesPath, type);

		if (!Directory.Exists(dir))
			return NotFound();

		string[] files = GetLocalFiles(dir);

		if (id >= files.Length)
			return NotFound();

		return PhysicalFile(files[id], "application/octet-stream", Path.GetFileName(files[id]));
	}

	/// <summary>Adds a ROM or BIOS file.</summary>
	/// <returns>The ID and name of the added file.</returns>
	[HttpPost("file/{type}")]
	[RequestSizeLimit(67108864)]
	public async Task<IActionResult> AddFile(string type, IFormFile? file)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (!IsSpecial())
			return StatusCode(403, "Special key required.");

		if (!IsValidType(type))
			return BadRequest("Type must be 'rom' or 'bios'.");

		if (file == null || file.Length == 0)
			return BadRequest("A file is required.");

		if (file.Length > 64 * 1024 * 1024)
			return BadRequest("Maximum file size is 64 MB.");

		string extension = Path.GetExtension(Path.GetFileName(file.FileName));

		if (string.IsNullOrWhiteSpace(extension) || extension.Length > 16)
			return BadRequest("Invalid file extension.");

		string name = Path.GetFileNameWithoutExtension(file.FileName);

		if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
			return BadRequest("Invalid file name.");

		if (DatabaseEnabled())
		{
			try
			{
				await using var stream = new MemoryStream();
				await file.CopyToAsync(stream);
				byte[] data = stream.ToArray();

				await using var connection = await OpenConnection();
				await using var command = new MySqlCommand("INSERT INTO Files (Name, Ext, Type, Data) VALUES (@name, @ext, @type, @data); SELECT LAST_INSERT_ID();", connection);
				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@ext", extension);
				command.Parameters.AddWithValue("@type", type);
				command.Parameters.Add("@data", MySqlDbType.LongBlob).Value = data;

				int id = Convert.ToInt32(await command.ExecuteScalarAsync());

				return Created($"/File/file/{type}/{id}", new { id, name, ext = extension, type });
			}
			catch (MySqlException)
			{
				return StatusCode(500, "Database error.");
			}
		}

		string dir = Path.Combine(_filesPath, type);
		Directory.CreateDirectory(dir);

		string path = Path.Combine(dir, name + extension);

		if (System.IO.File.Exists(path))
			return Conflict("A file with this name already exists.");

		try
		{
			await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			await file.CopyToAsync(output);
		}
		catch (IOException)
		{
			return Conflict("A file with this name already exists.");
		}

		int localId = Array.IndexOf(GetLocalFiles(dir), path);

		return Created($"/File/file/{type}/{localId}", new { id = localId, name, ext = extension, type });
	}

	/// <summary>Deletes a ROM or BIOS file.</summary>
	/// <returns>No content if the file was deleted.</returns>
	[HttpDelete("file/{type}/{id:int}")]
	public async Task<IActionResult> DeleteFile(string type, int id)
	{
		if (!IsAuthorized())
			return Unauthorized();

		if (!IsSpecial())
			return StatusCode(403, "Special key required.");

		if (!IsValidType(type))
			return BadRequest("Type must be 'rom' or 'bios'.");

		if (id < 0)
			return NotFound();

		if (DatabaseEnabled())
		{
			try
			{
				await using var connection = await OpenConnection();
				await using var command = new MySqlCommand("DELETE FROM Files WHERE Id = @id AND Type = @type", connection);
				command.Parameters.AddWithValue("@id", id);
				command.Parameters.AddWithValue("@type", type);

				if (await command.ExecuteNonQueryAsync() == 0)
					return NotFound();

				return NoContent();
			}
			catch (MySqlException)
			{
				return StatusCode(500, "Database error.");
			}
		}

		string dir = Path.Combine(_filesPath, type);

		if (!Directory.Exists(dir))
			return NotFound();

		string[] files = GetLocalFiles(dir);

		if (id >= files.Length)
			return NotFound();

		System.IO.File.Delete(files[id]);

		return NoContent();
	}


	/// <summary>Checks whether the request has a valid token.</summary>
	private bool IsAuthorized() => GetRole() is "Normal" or "Special";


	/// <summary>Checks whether the request has a special token.</summary>
	private bool IsSpecial() => GetRole() == "Special";


	/// <summary>Gets the role assigned to the Bearer token.</summary>
	private string? GetRole()
	{
		string auth = Request.Headers.Authorization.ToString();

		if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			return null;

		string token = auth["Bearer ".Length..].Trim();

		if (token.Length == 0 || !_cache.TryGetValue($"token:{token}", out string? role))
			return null;

		return role;
	}


	/// <summary>Checks whether the requested file type is supported.</summary>
	private static bool IsValidType(string type) => type is "rom" or "bios";


	/// <summary>Checks whether database storage is enabled.</summary>
	private bool DatabaseEnabled() => _configuration.GetValue<bool>("Database:Enabled");


	/// <summary>Opens a connection to the configured database.</summary>
	private async Task<MySqlConnection> OpenConnection()
	{
		string? connectionString = _configuration.GetConnectionString("Database");

		if (string.IsNullOrWhiteSpace(connectionString))
			throw new InvalidOperationException("Database connection string is missing.");

		var connection = new MySqlConnection(connectionString);

		try
		{
			await connection.OpenAsync();
			return connection;
		}
		catch
		{
			await connection.DisposeAsync();
			throw;
		}
	}


	/// <summary>Returns local files in a stable, sorted order.</summary>
	private static string[] GetLocalFiles(string dir) => Directory.GetFiles(dir).Where(f => !Path.GetFileName(f).StartsWith('.')).OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToArray();
}
