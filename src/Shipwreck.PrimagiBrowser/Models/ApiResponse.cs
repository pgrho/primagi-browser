namespace Shipwreck.PrimagiBrowser.Models;

public sealed class ApiResponse<T>
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public bool IsSuccessful()
        => Code == "00";
}
