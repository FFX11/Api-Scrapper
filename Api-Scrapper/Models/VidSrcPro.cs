using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api_Scrapper.Models;
public static class VidSrcPro
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<(int, string)> HandleVidsrcPro(HttpResponseMessage req, string source, string seed)
    {
        string reqContent = await req.Content.ReadAsStringAsync();
        string hlsUrl = Regex.Match(reqContent, @"file:""([^""]*)""").Groups[1].Value;
        hlsUrl = Regex.Replace(hlsUrl, @"\/\/\S+?=", "", RegexOptions.None).Substring(2);

        const int maxTries = 5;
        for (int i = 0; i < maxTries; i++)
        {
            hlsUrl = Regex.Replace(hlsUrl, @"\/@#@\/[^=\/]+==", "", RegexOptions.None);
            if (Regex.IsMatch(hlsUrl, @"\/@#@\/[^=\/]+=="))
                continue;
        }

        hlsUrl = hlsUrl.Replace('_', '/').Replace('-', '+');
        byte[] decodedBytes = Convert.FromBase64String(hlsUrl);
        hlsUrl = System.Text.Encoding.UTF8.GetString(decodedBytes);

        string setPass = Regex.Match(reqContent, @"var pass_path = ""(.*?)"";").Groups[1].Value;
        if (setPass.StartsWith("//"))
            setPass = $"https:{setPass}";

        await _httpClient.GetAsync(setPass, HttpCompletionOption.ResponseHeadersRead);

        return (int.Parse(seed), hlsUrl);
    }
}
