using System.Net.Http.Headers;

public class HttpHelper
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    public HttpHelper(string baseUrl = "")
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }
    public async Task<string> GetAsync(string url)
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    public async Task<Stream> GetStreamAsync(string url)
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }
    public async Task<byte[]> GetBufferAsync(string url)
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<string> PostAsync(string url, string data, string mediaType = "application/json")
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var content = new StringContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    public async Task<string> PutAsync(string url, string data, string mediaType = "application/json")
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var content = new StringContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
    public async Task<string> DeleteAsync(string url)
    {
        if (!(url.Contains("http://") || url.Contains("https://")))
            url = _baseUrl + url;
        var response = await _httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}