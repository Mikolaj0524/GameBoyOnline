using GB.Interfaces;
using System.Runtime.InteropServices;

namespace GB.Memory
{
	public class Memory : IRWInterface
	{
		private readonly byte[] _memory;
		private readonly uint _offset;
		private GCHandle _handle;

		public byte[] Data => _memory;

		public Memory(int size, uint offset)
		{
			_memory = GC.AllocateArray<byte>(size, pinned: true);
			_offset = offset;
			_handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);
		}

		public byte Read8(ushort address) => _memory[address - _offset];
		public void Write8(ushort address, byte value) => _memory[address - _offset] = value;
		public byte ReadDirect(ushort address) => _memory[address];
		public void WriteDirect(ushort address, byte value) => _memory[address] = value;
		public nint GetPtr() => _handle.AddrOfPinnedObject();

		public void Dispose()
		{
			if (_handle.IsAllocated)
				_handle.Free();

			GC.SuppressFinalize(this);
		}
	}
}
