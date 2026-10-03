namespace GB.Interfaces
{
	/// <summary>Interface for R/W bytes.</summary>
	public interface IRWInterface
	{
		/// <summary>Reads byte.</summary>
		/// <returns>Read byte.</returns>
		byte Read8(ushort address);


		/// <summary>Writes byte.</summary>
		void Write8(ushort address, byte value);
	}
}