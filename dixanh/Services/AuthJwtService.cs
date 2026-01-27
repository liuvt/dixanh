using dixanh.Helpers;
using dixanh.Libraries.Entities;
using dixanh.Services.Interfaces;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace dixanh.Services;

public class AuthJwtService : IAuthJwtService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private const string Key = "_identityApp";

    public AuthJwtService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<string> LoginTokenAsync(AppLoginDTO dto)
    {
        var res = await _http.PostAsJsonAsync("api/auth/token", dto);
        res.EnsureSuccessStatusCode();

        var token = (await res.Content.ReadAsStringAsync()).Trim('"');
        await _js.SetFromLocalStorage(Key, token);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    public async Task LogoutTokenAsync()
    {
        await _js.RemoveFromLocalStorage(Key);
        _http.DefaultRequestHeaders.Authorization = null;
    }
}
