using CashFlow.Domain.Security.Tokens;

namespace CashFlow.Api.Token;

public class HttpContextTokenValue : ITokenProvider
{
    private readonly IHttpContextAccessor _contextAccessor;
    public HttpContextTokenValue(IHttpContextAccessor httpContextAccessor)
    {
        _contextAccessor = httpContextAccessor;
    }
    public string TokenOnRequest()
    {
        var authorization = _contextAccessor.HttpContext!.Request.Headers.Authorization.ToString();
        // "Bearer abc123"

        // authorization = authorization.Replace("Bearer ", string.Empty);

        // o .. no código abaixo pegamos a substring a partir do índice do tamanho da string. Nesse caso, pegamos a substring a partir do índice 7 (tamanho da string "Bearer ")
        return authorization["Bearer ".Length..].Trim();
    }
}
