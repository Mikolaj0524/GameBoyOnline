using GB.Interfaces;

namespace GB.Communication
{
	public class SerialPort : IRWInterface
	{
		/// <summary>Serial transfer registers.</summary>
		public byte Stc, Stb;


		/// <summary>Reads a serial register.</summary>
		/// <returns>Register value.</returns>
		public byte Read8(ushort address)
		{
			if (address == 0xFF01)
				return Stb;

			if (address == 0xFF02)
				return Stc;

			return 0xFF;
		}


		/// <summary>Writes to a serial register.</summary>
		public void Write8(ushort address, byte value)
		{
			if (address == 0xFF01)
			{
				Stb = value;
				return;
			}

			if (address == 0xFF02)
			{
				Stc = value;
				if ((value & 0b1000_0000) != 0)
				{
					Console.Write((char)Stb);
					Stc &= 0b0111_1111;
				}
				return;
			}
		}
	}
}
