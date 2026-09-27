using GB.Interfaces;

namespace GB.Memory
{
	public class Memory : IRWInterface
	{
		private readonly byte[] _memory;
		private readonly uint _offset;

		public Memory(int size, uint offset)
		{
			_memory = new byte[size];
			_offset = offset;
		}

		public byte Read8(ushort address) => _memory[address - _offset];
		public void Write8(ushort address, byte value) => _memory[address - _offset] = value;
		public byte ReadDirect(ushort address) => _memory[address];
		public void WriteDirect(ushort address, byte value) => _memory[address] = value;
	}
}
