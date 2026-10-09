namespace GB.Backend.Dtos
{
	public class VerifyRequestDto
	{
		public string Challenge { get; set; } = string.Empty;
		public string Response { get; set; } = string.Empty;
	}
}
