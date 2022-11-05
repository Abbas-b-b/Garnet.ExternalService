namespace Garnet.Standard.ExternalService.Configurations;

/// <summary>
/// Basic configuration fields that is necessary for a token based client to work. Can be extended to add more fields
/// </summary>
public class TokenAuthClientConfiguration : ClientConfiguration
{
    /// <summary>
    /// Full path for authenticating and getting tokens
    /// </summary>
    public string AuthUri { get; set; }
    
    /// <summary>
    /// Full path for refreshing the token
    /// </summary>
    public string RefreshTokenUri { get; set; }
}