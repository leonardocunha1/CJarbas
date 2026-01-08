using System.Globalization;

namespace CashFlow.Api.Middleware;

public class CultureMiddleware
{
    private readonly RequestDelegate _next;
    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        var culture = context.Request.Headers.AcceptLanguage.FirstOrDefault();
        // Define a cultura padrão como "en"
        var cultureInfo = new CultureInfo("en");
        // Executado quando o header Accept-Language estiver presente e não for vazio ou nulo
        if (!string.IsNullOrWhiteSpace(culture))
        {
            // Tenta definir a cultura com base no header
            cultureInfo = new CultureInfo(culture);
        }
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        await _next(context);
    }
}
