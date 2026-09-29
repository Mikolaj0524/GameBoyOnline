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
			AllocConsole();
			ApplicationConfiguration.Initialize();

			GameBoy gameBoy = new();
			byte[] bios = File.ReadAllBytes("bios.gb");

			gameBoy.SetBios(bios);

			byte[] rom = File.ReadAllBytes("tetris.gb");
			gameBoy.SetCartridge(rom);

			var window = new Form1();

			if (!gameBoy.Run())
			{
				Application.Exit();
				return;
			}

			Application.Run(window);
		}
	}
}