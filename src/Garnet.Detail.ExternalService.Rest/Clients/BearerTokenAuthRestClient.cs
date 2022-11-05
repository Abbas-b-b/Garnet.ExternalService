using System;
using System.Threading.Tasks;
using Garnet.Standard.ExternalService.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Garnet.Detail.ExternalService.Rest.Clients;

/// <summary>
/// A client for handling Bearer token authentication
/// </summary>
/// <typeparam name="TAuthRequestDto">Auth request object type</typeparam>
/// <typeparam name="TAuthResponseDto">Auth response object type</typeparam>
/// <typeparam name="TTokenRefreshRequestDto">Refresh token request object type</typeparam>
public abstract class BearerTokenAuthRestClient<TAuthRequestDto, TAuthResponseDto, TTokenRefreshRequestDto> : BasicRestClient
    where TAuthRequestDto : class
    where TAuthResponseDto : class
    where TTokenRefreshRequestDto : class
{
    /// <summary>
    /// Client configuration
    /// </summary>
    protected readonly TokenAuthClientConfiguration TokenAuthClientConfiguration;
    
    /// <summary>
    /// Memory cache for storing token
    /// </summary>
    protected readonly IMemoryCache MemoryCache;
    
    /// <summary>
    /// The key used for token cache
    /// </summary>
    protected readonly string TokenMemoryCacheKey = Guid.NewGuid().ToString();

    
    /// <summary>
    /// A client for handling Bearer token authentication
    /// </summary>
    /// <param name="tokenAuthClientConfiguration">To configure client with base and auth URIs</param>
    /// <param name="logger"></param>
    /// <param name="memoryCache">For caching token</param>
    protected BearerTokenAuthRestClient(TokenAuthClientConfiguration tokenAuthClientConfiguration,
        ILogger<BasicRestClient> logger,
        IMemoryCache memoryCache)
        : base(tokenAuthClientConfiguration, logger)
    {
        TokenAuthClientConfiguration = tokenAuthClientConfiguration;
        MemoryCache = memoryCache;
    }

    /// <inheritdoc />
    public override async Task<RestResponse> SendRequestAsync(RestRequest request)
    {
        if (request.Resource != TokenAuthClientConfiguration.AuthUri
            && request.Resource != TokenAuthClientConfiguration.RefreshTokenUri)
        {
            await GetAndSetAuthHeaders(request);
        }

        return await base.SendRequestAsync(request);
    }

    /// <summary>
    /// Get Auth parameters and set to the request
    /// </summary>
    /// <param name="request">RestSharp request to set Auth parameters</param>
    protected virtual async Task GetAndSetAuthHeaders(RestRequest request)
    {
        var token = await GetAuthToken();

        SetAuthHeaderParameter(request, token);
    }

    /// <summary>
    /// Set Auth headers to the RestSharp request
    /// </summary>
    /// <param name="request">RestSharp request to set Auth parameters</param>
    /// <param name="token">Bearer token to set on request</param>
    protected virtual void SetAuthHeaderParameter(RestRequest request, string token)
    {
        request.AddOrUpdateHeader("Authorization", $"Bearer {token}");
    }

    /// <summary>
    /// Get Auth token
    /// </summary>
    /// <returns>Auth token</returns>
    protected virtual async Task<string> GetAuthToken()
    {
        var existInCache = MemoryCache.TryGetValue<TAuthResponseDto>(TokenMemoryCacheKey, out var authResponseDto);

        if (!existInCache)
        {
            authResponseDto = await SendAuthRequest();
        }
        else if (RequiresTokenRefresh(authResponseDto))
        {
            authResponseDto = await SendTokenRefreshRequest(authResponseDto);
        }
        else
        {
            return ConvertResponseToAuthToken(authResponseDto);
        }

        MemoryCache.Set(TokenMemoryCacheKey, authResponseDto, GetExpirationOfToken(authResponseDto));

        return ConvertResponseToAuthToken(authResponseDto);
    }

    /// <summary>
    /// Send Auth request for getting Auth token
    /// </summary>
    /// <returns><typeparamref name="TAuthResponseDto"/> after executing the request</returns>
    protected virtual async Task<TAuthResponseDto> SendAuthRequest()
    {
        return await base.SendRequestAsync<TAuthRequestDto, TAuthResponseDto>(TokenAuthClientConfiguration.AuthUri,
            Method.Post,
            await GetAuthRequestDto());
    }

    /// <summary>
    /// Send token refresh token request to renew token
    /// </summary>
    /// <param name="responseDto">Token refresh response to cast the response to</param>
    /// <returns></returns>
    protected virtual async Task<TAuthResponseDto> SendTokenRefreshRequest(TAuthResponseDto responseDto)
    {
        return await base.SendRequestAsync<TTokenRefreshRequestDto, TAuthResponseDto>(
            TokenAuthClientConfiguration.RefreshTokenUri,
            Method.Post,
            await GetTokenRefreshRequestDto(responseDto));
    }

    /// <summary>
    /// Convert the auth response to a token string for using in future requests
    /// </summary>
    /// <param name="responseDto">The response received from Auth request</param>
    /// <returns>Bearer token</returns>
    protected abstract string ConvertResponseToAuthToken(TAuthResponseDto responseDto);
    
    /// <summary>
    /// Token expiration period
    /// </summary>
    /// <param name="responseDto">The response received from Auth request</param>
    /// <returns></returns>
    protected abstract TimeSpan GetExpirationOfToken(TAuthResponseDto responseDto);
    
    /// <summary>
    /// Create Auth request object for getting tokens
    /// </summary>
    /// <returns></returns>
    protected abstract Task<TAuthRequestDto> GetAuthRequestDto();
    
    /// <summary>
    /// Create request object for refreshing the token
    /// </summary>
    /// <param name="responseDto">Auth response object</param>
    /// <returns>Token Refresh request object</returns>
    protected abstract Task<TTokenRefreshRequestDto> GetTokenRefreshRequestDto(TAuthResponseDto responseDto);
    
    /// <summary>
    /// To determine whether token should be refreshed if its expired
    /// </summary>
    /// <param name="responseDto">Auth response</param>
    /// <returns></returns>
    protected abstract bool RequiresTokenRefresh(TAuthResponseDto responseDto);
}