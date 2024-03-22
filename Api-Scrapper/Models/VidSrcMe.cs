using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Api_Scrapper.Models;

namespace Api_Scrapper.Models;
public static class VidSrcMe
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<(int, string)> VidSrcMeAsync(string source, string url)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://rcp.vidsrc.me/rcp/{source}");

        request.Headers.Add("Referer", url);

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        var content = await response.Content.ReadAsStringAsync();
        var htmlParser = new HtmlParser();
        var document = await htmlParser.ParseDocumentAsync(content);
        var encodedDiv = document.GetElementById("hidden")?.GetAttribute("data-h");
        if (string.IsNullOrEmpty(encodedDiv)) return (1506, null);

        var seed = document.Body.GetAttribute("data-i");
        var encodedBuffer = encodedDiv.HexToBytes();
        var decoded = new StringBuilder();
        for (var i = 0; i < encodedBuffer.Length; i++)
            decoded.Append((char)(encodedBuffer[i] ^ seed[i % seed.Length]));
        var decodedUrl = decoded.ToString().StartsWith("//") ? $"https:{decoded}" : decoded.ToString();


        _httpClient.DefaultRequestHeaders.Referrer = new Uri($"https://rcp.vidsrc.me/rcp/{source}");

        var redirectResponse = await _httpClient.GetAsync(decodedUrl, HttpCompletionOption.ResponseHeadersRead);


        var location = redirectResponse.Headers.Location?.ToString();



        if (location == null) return (1506, seed);
        if (location.Contains("playhydrax.com")) return (1500, seed);
        if (location.Contains("vidsrc.stream"))
        {
            var (hlsUrl, subtitles) = await Superembed.HandleSuperembed(location, source, int.Parse(seed));
            return (hlsUrl != "" ? 0 : 1500, seed);
        }
        if (location.Contains("2embed.cc")) return (1500, seed);
        if (location.Contains("multiembed.mov"))
        {
            var (hlsUrl, subtitles) = await Superembed.HandleSuperembed(location, source, int.Parse(seed));
            return (hlsUrl != "" ? 0 : 1500, seed);
        }

        throw new Exception("Unhandled location");
    }

    public static async Task<List<object>> Get(string dbid, string s = null, string e = null, string l = "eng")
    {
        var provider = dbid.Contains("tt") ? "imdb" : "tmdb";
        var media = (s != null && e != null) ? "tv" : "movie";
        var language = l;
        var url = $"https://vidsrc.me/embed/{dbid}" + ((s != null && e != null) ? $"/{s}-{e}" : "");

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var htmlParser = new HtmlParser();
        var document = await htmlParser.ParseDocumentAsync(content);
        var serverDivs = document.GetElementsByClassName("server");
        var sources = serverDivs.Select(attr => new KeyValuePair<string, string>(attr.TextContent, attr.GetAttribute("data-hash"))).ToDictionary(pair => pair.Key, pair => pair.Value);

        sources.Remove("VidSrc Hydrax");
        sources.Remove("2Embed");

        var source = sources.Values.ToList();
        if (!source.Any()) return new List<object> { 1404, null };

        var tasks = source.Select(s => VidSrcMeAsync(s, url));
        var results = await Task.WhenAll(tasks);
        Console.WriteLine(results); 

        var subSeed = results[0].Item2 ?? "1500";
        var subtitles = subSeed != "500" ? await Subtitle.Subfetch(subSeed, language) : "500";

        return new List<object>
        {
            new
            {
                name = "VidSrcPRO",
                data = new
                {
                    file = results[0].Item1,
                    sub = subtitles
                }
            },
            new
            {
                name = "SuperEmbed",
                data = new
                {
                    file = results.Length == 2 ? results[1].Item1.ToString() : "1500",
                    sub = results.Length == 2 ? results[1].Item2 : "1500"
                }
            }
        };
    }
}

public static class Extensions
{
    public static byte[] HexToBytes(this string hex)
    {
        if (hex.Length % 2 != 0)
            throw new ArgumentException("Invalid length");
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }
}
