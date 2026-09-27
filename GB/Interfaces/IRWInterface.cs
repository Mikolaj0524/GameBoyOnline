namespace GB.Interfaces
{
	public interface IRWInterface
	{
		byte Read8(ushort address);
		void Write8(ushort address, byte value);
	}
}