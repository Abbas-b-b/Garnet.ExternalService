# Garnet External Service

Makes external service integration easier by providing some boilerplate codes.

## Garnet.Standard.ExternalService [![Nuget](https://img.shields.io/nuget/dt/Garnet.Standard.ExternalService?style=for-the-badge)](https://www.nuget.org/packages/Garnet.Standard.ExternalService/)

    dotnet add package Garnet.Standard.ExternalService

The idea behind standard packages is not to mess with the domain or application layer with implementation details. this package holds configuration DTOs and other shared info

## Garnet.Detail.ExternalService.Rest [![Nuget](https://img.shields.io/nuget/dt/Garnet.Detail.ExternalService.Rest?style=for-the-badge)](https://www.nuget.org/packages/Garnet.Detail.ExternalService.Rest/)

    dotnet add package Garnet.Standard.ExternalService

This package is based on [RestSharp](https://restsharp.dev/) and provides these clients:

- **BasicRestClient**: For simple REST service by handling logging and castings
- **BearerTokenAuthRestClient**: handle Bearer token authentication by caching the token and refreshing token when it is necessary

Clients can be used by inheriting it and overriding any method that needs to work in a different approach

```C#
public class MyTestClient : BearerTokenAuthRestClient<LoginRequest, LoginResponse, RefreshTokenRequest>
{
    private readonly MyTestClientConfiguration _myTestClientConfiguration;

    public MyTestClient(MyTestClientConfiguration myTestClientConfiguration,
        ILogger<BasicRestClient> logger,
        IMemoryCache memoryCache)
        : base(myTestClientConfiguration, logger, memoryCache)
    {
        Client.UseSystemTextJson(new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = new MyTestJsonPropertyNaming(),
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        _myTestClientConfiguration = myTestClientConfiguration;
    }

    protected override string ConvertResponseToAuthToken(LoginResponse responseDto)
    {
        return responseDto.AccessToken;
    }

    protected override TimeSpan GetExpirationOfToken(LoginResponse responseDto)
    {
        return _myTestClientConfiguration.TokenExpirationTime;
    }

    protected override Task<LoginRequest> GetAuthRequestDto()
    {
        var loginRequest = new LoginRequest(_myTestClientConfiguration.Username, _myTestClientConfiguration.Password);

        return Task.FromResult(loginRequest);
    }

    protected override Task<RefreshTokenRequest> GetTokenRefreshRequestDto(LoginResponse responseDto)
    {
        return Task.FromResult(new RefreshTokenRequest(responseDto.RefreshToken));
    }

    protected override bool RequiresTokenRefresh(LoginResponse responseDto)
    {
        return false;
    }
}
```
