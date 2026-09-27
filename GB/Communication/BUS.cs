using GB.Interfaces;

namespace GB.Communication
{
	public class BUS : IRWInterface
	{
		public byte Read8(ushort address) {
			return ReadDirect8(address);
		}

		public void Write8(ushort address, byte value) {
			WriteDirect8(address, value);
		}

		public byte ReadDirect8(ushort address) {
			return 0xFF;
		}

		public void WriteDirect8(ushort address, byte value){
			
		}
	}
}
