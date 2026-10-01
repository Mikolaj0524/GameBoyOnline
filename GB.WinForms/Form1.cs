using GB.Audio;
using GB.Memory;
using NAudio.Wave;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GB.WinForms
{
	public partial class Form1 : Form
	{
		private const int scale = 4;
		private readonly Bitmap _bitmap = new(160, 144, PixelFormat.Format24bppRgb);

		private Label? _label;
		private PictureBox? _picture;

		private WaveOut? _waveOut;
		private readonly GameBoy _gameBoy;

		[DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

		public Form1(GameBoy gameBoy)
		{
			InitializeComponent();
			_gameBoy = gameBoy;

			AdjustWindow();
			InitFrame();

			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
		}

		private void OnKeyDown(object? sender, KeyEventArgs e) => UseButton(e, true);
		private void OnKeyUp(object? sender, KeyEventArgs e) => UseButton(e, false);

		private void UseButton(KeyEventArgs e, bool state)
		{
			Controls.Button? button = null;
			switch (e.KeyCode)
			{
				case Keys.Oemcomma: button = GB.Controls.Button.A; break;
				case Keys.OemPeriod: button = GB.Controls.Button.B; break;
				case Keys.Enter: button = GB.Controls.Button.Start; break;
				case Keys.RShiftKey: button = GB.Controls.Button.Select; break;
				case Keys.Up: button = GB.Controls.Button.Up; break;
				case Keys.Down: button = GB.Controls.Button.Down; break;
				case Keys.Left: button = GB.Controls.Button.Left; break;
				case Keys.Right: button = GB.Controls.Button.Right; break;
			}

			if (button.HasValue)
				_gameBoy.Bus.IO.Joypad.Press(button.Value, state);
		}

		public void UpdateFrame(byte[] framebuffer)
		{
			if (IsDisposed)
				return;

			if (InvokeRequired)
			{
				BeginInvoke(UpdateFrame, framebuffer);
				return;
			}

			var data = _bitmap.LockBits(new(0, 0, 160, 144), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			Marshal.Copy(framebuffer, 0, data.Scan0, framebuffer.Length);
			_bitmap.UnlockBits(data);

			_picture?.Invalidate();
		}

		private void AdjustWindow()
		{
			ClientSize = new Size(160 * scale, 144 * scale);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Text = "GB Emulator";
			KeyPreview = true;
			BackColor = Color.FromArgb(32, 32, 32);

			int darkMode = 1;
			DwmSetWindowAttribute(Handle, 20, ref darkMode, sizeof(int));
		}

		private void InitFrame()
		{
			_picture = new()
			{
				Image = _bitmap,
				Dock = DockStyle.Fill,
				SizeMode = PictureBoxSizeMode.Zoom
			};
			Controls.Add(_picture);

			Cartridge? c = _gameBoy.Bus.Cartridge;
			Text += $" | {c?.Title}";
			_label = new()
			{
				BackColor = Color.Transparent,
				ForeColor = Color.White,
				AutoSize = true,
				Text = $"Version: {c?.Version} | Publisher: {c?.GetPublisher()} | Type: {c?.GetRomType()} | Title: {c?.Title?.Trim()}"
			};
			_picture.Controls.Add(_label);

			_gameBoy.Bus.IO.Ppu.FrameReady = fb => UpdateFrame(fb);
		}

	}
}