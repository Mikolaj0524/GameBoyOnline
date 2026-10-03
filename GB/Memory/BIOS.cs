namespace GB.Memory
{
	public class BIOS(byte[] bios)
	{
		private readonly byte[] _memory = bios;


		/// <summary>BIOS enabled state.</summary>
		public bool Enabled = true;


		/// <summary>Read bytes from the BIOS.</summary>
		/// <returns>Reads byte from memory.</returns>
		public byte Read8(ushort address)
		{
			return _memory[address];
		}
	}
}
