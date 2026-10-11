<div align="center">
	<br>
	<h1>🖥️ GB.WinForms — Native Desktop Client</h1>
	<p align="center">
		<b>The native Windows desktop application wrapper for the GameBoyOnline emulator core.</b>
		<br />
		<i>Built with Windows Forms and C# (.NET 10) for direct, high-performance desktop execution on Windows.</i>
	</p>
	<p align="center">
		<a href="#overview"><b>Overview</b></a> •
		<a href="#project-structure"><b>File Structure</b></a> •
		<a href="#getting-started"><b>Quick Start</b></a>
	</p>
	<p align="center">
		<img src="https://img.shields.io/badge/C%23_.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="C# .NET 10" />
		<img src="https://img.shields.io/badge/Windows_Forms-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="Windows Forms" />
		<a href="https://gplv3.fsf.org/">
		<img src="https://img.shields.io/badge/License-GNU_GPL--3.0-blue?style=for-the-badge&logoColor=white" alt="License" />
		</a>
	</p>
	<br>
</div>

---
<br />
<h2 align="center" id="overview">📖 Overview</h2>
<p align="center">
	<b>GB.WinForms</b> is the native desktop client application for GameBoyOnline. It wraps the pure C# emulator core (<b><code>GB/</code></b>) inside a traditional Windows Forms graphical user interface, allowing users to play retro games and inspect hardware natively on Windows without needing a web browser.
</p>
<br />

---
<br />
<h2 align="center" id="project-structure">📂 Project Structure</h2>

```text
GB.WinForms/
├── Form1.cs                # Main application window logic, UI events & emulator loop hooks
├── Form1.Designer.cs       # Windows Forms designer-generated layout and component code
├── Form1.resx              # Embedded application resources (icons, localization, strings)
├── GB.WinForms.csproj      # MSBuild project file targeting .NET 10
├── GB.WinForms.csproj.user # Local user-specific MSBuild configuration settings
└── Program.cs              # Application entry point and startup configuration
```
<br />

---
<br />
<h2 align="center" id="getting-started">⭐ Quick start</h2>

> [!NOTE]
> Ensure you have the **[.NET 10.0 SDK](https://dotnet.microsoft.com/)** installed. Windows Forms execution requires a Windows environment.

```bash
cd GB.WinForms
dotnet build
dotnet run
```
<br />

---