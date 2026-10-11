<div align="center">
	<br />
	<h1>⚡ GB.Backend — REST API & Storage Server</h1>
	<p align="center">
		<b>The ASP.NET Core backend server for the GameBoyOnline ecosystem.</b>
		<br />
		<i>Provides RESTful ROM hosting, file synchronization, authentication, and database connectivity.</i>
	</p>
	<p align="center">
		<a href="#overview"><b>Overview</b></a> •
		<a href="#configuration"><b>Configuration</b></a> •
		<a href="#project-structure"><b>File Structure</b></a> •
		<a href="#getting-started"><b>Quick Start</b></a>
	</p>
	<p align="center">
		<img src="https://img.shields.io/badge/C%23_.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="C# .NET 10" />
		<img src="https://img.shields.io/badge/ASP.NET_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
		<img src="https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySQL" />
		<img src="https://img.shields.io/badge/MySqlConnector-4479A1?style=for-the-badge&logo=mysql&logoColor=white" alt="MySqlConnector" />
		<a href="https://gplv3.fsf.org/">
		<img src="https://img.shields.io/badge/License-GNU_GPL--3.0-blue?style=for-the-badge&logoColor=white" alt="License" />
		</a>
	</p>
	<br>
</div>

---
<br>
<h2 align="center" id="overview">📖 Overview</h2>

<p align="center">
	<b>GB.Backend</b> is the server-side API component of GameBoyOnline built with <b>ASP.NET Core (.NET 10)</b> and <b>MySqlConnector</b>. It manages ROM file storage, user authentication via access keys, and allows the emulator clients to seamlessly switch between local disk storage and a MySQL database backend.
</p>
<br>

---
<br>
<h2 align="center" id="configuration">⚙️ Configuration (`appsettings.json`)</h2>
<p align="center">
	The server behavior, database toggles, connection strings, and security access keys are configured directly in <code>appsettings.json</code>:
</p>

```json
{
"Logging": {
	"LogLevel": {
	"Default": "Information",
	"Microsoft.AspNetCore": "Warning"
	}
},
"AllowedHosts": "*",
"Database": {
	"Enabled": false
},
"ConnectionStrings": {
	"Database": "Server=127.0.0.1;Port=3306;Database=GB;User ID=root;Password=;"
},
"AccessKeys": [
	{
	"Code": "NORMAL_KEY",
	"Role": "Normal"
	},
	{
	"Code": "SPECIAL_KEY",
	"Role": "Special"
	}
]
}
```
<br>

---
<br>
<h2 align="center" id="project-structure">📃 File Structure</h2>

```text
GB.Backend/
├── Controllers/
│   ├── AuthController.cs       # Authentication & access key verification endpoints
│   └── FileController.cs       # ROM file upload, download, and management endpoints
├── Dtos/
│   ├── FileDto.cs              # DTOs for file metadata
│   └── VerifyRequestDto.cs     # DTOs for auth verification
├── Files/                      # Local file storage for ROM binaries
├── Properties/
│   └── launchSettings.json     # Kestrel and IIS server profile configuration
├── appsettings.Development.json# Development environment overrides
├── appsettings.json            # Main application configuration (DB toggle, connection & keys)
├── database.sql                # MySQL database schema initialization script
├── GB.Backend.csproj           # MSBuild project file targeting .NET 10
├── GB.Backend.csproj.user      # Local user-specific MSBuild configuration settings
├── GB.Backend.http             # HTTP file for testing API requests directly in IDE
└── Program.cs                  # Application entry point, DI container & middleware configuration

```
<br>

---
<br>

<h2 align="center" id="getting-started">⭐ Quick start</h2>

> [!NOTE]
> Make sure you have the **[.NET 10.0 SDK](https://dotnet.microsoft.com/)** and a running **MySQL Server** instance installed if database mode is enabled.

```bash
cd GB.Backend
dotnet build
dotnet run
```

The backend server will start locally (typically on ports configured in `Properties/launchSettings.json`).
<br>

---