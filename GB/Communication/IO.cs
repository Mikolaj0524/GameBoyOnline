using GB.Audio;
using GB.Controls;
using GB.GPU;
using GB.Interfaces;
using GB.Memory;

namespace GB.Communication
{
	public class IO : IRWInterface
	{
		public Screen Screen = new();
		public Timers.Timer Timer;
		public Joypad Joypad;
		public APU Apu;
		public SerialPort SerialPort;
		public WaveRAM WaveRam = new();
		public PPU Ppu;
		public BUS Bus;
		public DMA Dma;
		public IORegisters Registers = new();

		public IO(BUS bus) {
			Bus = bus;

			Joypad = new Joypad(bus.InterruptController);
			Timer = new Timers.Timer(bus.InterruptController);
			SerialPort = new SerialPort(bus.InterruptController);

			Dma = new DMA(bus);
			Ppu = new PPU(this);
			Apu = new APU(WaveRam);
		}


		/// <summary>Reads from an I/O register.</summary>
		public byte Read8(ushort address)
		{
			if (address == 0xFF0F)
				return Bus.InterruptController.IF;

			if (address == 0xFF00)
				return Joypad.Read8(address);

			if (address >= 0xFF01 && address <= 0xFF02)
				return SerialPort.Read8(address);

			if (address >= 0xFF04 && address <= 0xFF07)
				return Timer.Read8(address);

			if (address >= 0xFF10 && address <= 0xFF26)
				return Apu.Read8(address);

			if (address >= 0xFF30 && address <= 0xFF3F)
				return WaveRam.Read8(address);

			if (address >= 0xFF40 && address <= 0xFF4B)
				return Screen.Read8(address);

			if (address >= 0xFF4D && address <= 0xFF70)
				return Registers.Read8(address);

			Console.WriteLine($"[IO] Unable to find destination, address: 0x{address:X4}");
			return 0xFF;
		}


		/// <summary>Writes to an I/O register.</summary>
		public void Write8(ushort address, byte value)
		{
			if (address == 0xFF0F)
			{
				Bus.InterruptController.IF = value;
				return;
			}

			if (address == 0xFF46)
			{
				Dma.Transfer(value);
				return;
			}

			if (address == 0xFF00)
			{
				Joypad.Write8(address, value);
				return;
			}

			if (address >= 0xFF01 && address <= 0xFF02)
			{
				SerialPort.Write8(address, value);
				return;
			}

			if (address >= 0xFF04 && address <= 0xFF07)
			{
				Timer.Write8(address, value);
				return;
			}

			if (address >= 0xFF10 && address <= 0xFF26)
			{
				Apu.Write8(address, value);
				return;
			}

			if (address >= 0xFF30 && address <= 0xFF3F)
			{
				WaveRam.Write8(address, value);
				return;
			}

			if (address >= 0xFF40 && address <= 0xFF4B)
			{
				Screen.Write8(address, value);
				return;
			}

			if (address >= 0xFF4D && address <= 0xFF70)
			{
				Registers.Write8(address, value);
				return;
			}

			Console.WriteLine($"[IO] Unable to find destination, address: 0x{address:X4}");
		}

		/// <summary>Runs steps in other components.</summary>
		public void Step(int cycles)
		{
			Timer.Step(cycles);
			Ppu.Step(cycles);
			SerialPort.Step(cycles);
			Apu.Step(cycles);
		}
	}
}