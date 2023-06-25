namespace Garnet.Standard.ExternalService.Configurations;

/// <summary>
/// Basic configuration fields that is necessary for a basic client to work. Can be extended to add more fields
/// </summary>
public class ClientConfiguration
{
    /// <summary>
    /// Base uri for the client to send the requests to
    /// </summary>
    public string BaseUri { get; set; }

    /// <summary>
    /// Indicates request response logging
    /// </summary>
    public bool LogRequestResponseWithContents { get; set; } = true;
}