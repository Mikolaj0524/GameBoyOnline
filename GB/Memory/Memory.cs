using GB.Interfaces;
using System.Runtime.InteropServices;

namespace GB.Memory
{
	public class Memory : IRWInterface
	{
		private readonly byte[] _memory;
		private readonly uint _offset;
		private GCHandle _handle;

		/// <summary>Gets memory data.</summary>
		public byte[] Data => _memory;

		public Memory(int size, uint offset)
		{
			_memory = GC.AllocateArray<byte>(size, pinned: true);
			_offset = offset;
			_handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);
		}


		/// <summary>Reads byte directly from memory, including offset.</summary>
		public byte Read8(ushort address) => _memory[address - _offset];


		/// <summary>Reads byte directly from memory, bypassing offset.</summary>
		public byte ReadDirect(ushort address) => _memory[address];


		/// <summary>Writes byte into memory, including offset.</summary>
		public void Write8(ushort address, byte value) => _memory[address - _offset] = value;


		/// <summary>Writes byte directly into memory, bypassing offset.</summary>
		public void WriteDirect(ushort address, byte value) => _memory[address] = value;


		/// <summary>Returns pointer to memory array.</summary>
		/// <returns>Pointer to memory.</returns>
		public nint GetPtr() => _handle.AddrOfPinnedObject();


		/// <summary>Releases the memory.</summary>
		public void Dispose()
		{
			if (_handle.IsAllocated)
				_handle.Free();

			GC.SuppressFinalize(this);
		}
	}
}
