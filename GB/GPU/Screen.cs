using GB.Interfaces;

namespace GB.GPU
{
	public class Screen : IRWInterface
	{
		public byte Lcdc, Bgp, Scx, Scy, Obp0, Obp1, Wy, Wx, Stat, Ly, Lyc;

		public byte Read8(ushort address) => address switch
		{
			0xFF40 => Lcdc,
			0xFF41 => Stat,
			0xFF42 => Scy,
			0xFF43 => Scx,
			0xFF44 => Ly,
			0xFF45 => Lyc,
			0xFF47 => Bgp,
			0xFF48 => Obp0,
			0xFF49 => Obp1,
			0xFF4A => Wy,
			0xFF4B => Wx,
			_ => throw new NotImplementedException($"[Screen] Unable to find destination, address: 0x{address:X4}")
		};

		public void Write8(ushort address, byte value)
		{
			switch (address)
			{
				case 0xFF40: Lcdc = value; return;
				case 0xFF41: Stat = value; return;
				case 0xFF42: Scy = value; return;
				case 0xFF43: Scx = value; return;
				case 0xFF44: return;
				case 0xFF45: Lyc = value; return;
				case 0xFF47: Bgp = value; return;
				case 0xFF48: Obp0 = value; return;
				case 0xFF49: Obp1 = value; return;
				case 0xFF4A: Wy = value; return;
				case 0xFF4B: Wx = value; return;
				default: throw new NotImplementedException($"[Screen] Unable to find destination, address: 0x{address:X4}");
			}
		}
	}
}
