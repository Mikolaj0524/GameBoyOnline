using System.Runtime.InteropServices;

namespace GB.WinForms
{
	internal static class Program
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AllocConsole();

		[STAThread]
		static void Main()
		{
			// Open a console window.
			AllocConsole();
			ApplicationConfiguration.Initialize();


			// Create Game Boy emulator.
			GameBoy gameBoy = new();

			// Load BIOS and ROM.
			byte[] bios = File.ReadAllBytes("bios.gb");
			gameBoy.SetBios(bios);

			byte[] rom = File.ReadAllBytes("camera.gb");
			gameBoy.SetCartridge(rom);

			var window = new Form1(gameBoy);

			// Run emulator
			if (!gameBoy.Run())
			{
				Application.Exit();
				return;
			}

			Application.Run(window);
		}
	}
}