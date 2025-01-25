namespace SinafProduction.Models;

public class ErrorModel
{
	public string? RequestId { get; init; }
	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	
	public string message { get; set; }
}