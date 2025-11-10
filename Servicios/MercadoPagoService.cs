using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PredatorsGym.Servicios;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly HttpClient _http;
    private readonly string _accessToken;

    public MercadoPagoService(IConfiguration cfg)
    {
        _http = new HttpClient();
        _accessToken = cfg["MercadoPago:AccessToken"] ?? string.Empty;
    }

    private void EnsureToken()
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new InvalidOperationException("MercadoPago AccessToken no configurado. Configure 'MercadoPago:AccessToken' en appsettings o secrets.");
    }

    public async Task<MercadoPagoPreferenceResult> CreatePreferenceAsync(string plan, string duration, decimal unitPrice, string userId, string? userEmail, string baseUrl)
    {
        EnsureToken();
        var url = "https://api.mercadopago.com/checkout/preferences";

        var body = new
        {
            items = new[]
            {
                new { title = $"Membresía {plan} ({duration})", quantity = 1, unit_price = unitPrice, currency_id = "PEN" }
            },
            payer = userEmail is null ? null : new { email = userEmail },
            external_reference = $"membresia:{plan}:{duration}:{Guid.NewGuid():N}",
            back_urls = new
            {
                success = Combine(baseUrl, "/Membresia/PaymentReturn?status=success"),
                failure = Combine(baseUrl, "/Membresia/PaymentReturn?status=failure"),
                pending = Combine(baseUrl, "/Membresia/PaymentReturn?status=pending")
            },
            notification_url = Combine(baseUrl, "/Membresia/Webhook"),
            auto_return = "approved",
            metadata = new { userId, plan, duration }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var res = await _http.SendAsync(req);
        var json = await res.Content.ReadAsStringAsync();
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new MercadoPagoPreferenceResult
        {
            Id = root.GetProperty("id").GetString() ?? string.Empty,
            InitPoint = root.TryGetProperty("init_point", out var init) ? init.GetString() ?? string.Empty : string.Empty,
            ExternalReference = root.TryGetProperty("external_reference", out var ext) ? ext.GetString() ?? string.Empty : string.Empty
        };
    }

    public async Task<MercadoPagoPreferenceInfo?> GetPreferenceAsync(string preferenceId)
    {
        EnsureToken();
        var url = $"https://api.mercadopago.com/checkout/preferences/{preferenceId}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var info = JsonSerializer.Deserialize<MercadoPagoPreferenceInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return info;
    }

    public async Task<MercadoPagoPaymentInfo?> GetPaymentAsync(string paymentId)
    {
        EnsureToken();
        var url = $"https://api.mercadopago.com/v1/payments/{paymentId}";
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        var info = JsonSerializer.Deserialize<MercadoPagoPaymentInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return info;
    }

    private static string Combine(string baseUrl, string path)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return path;
        if (baseUrl.EndsWith('/')) baseUrl = baseUrl.TrimEnd('/');
        return baseUrl + path;
    }
}
