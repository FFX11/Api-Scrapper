using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Api_Scrapper.Models
{
    public class Subtitle
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<string> Subfetch(string code, string language)
        {
            string subBaseUrl = "https://api-vidsrc-rouge.vercel.app/subs?url=";
            Console.WriteLine(code);
            string url;
            if (code.Contains("_"))
            {
                string[] parts = code.Split("_");
                string imdbId = parts[0];
                string seasonEpisode = parts[1];
                string[] seParts = seasonEpisode.Split('x');
                string season = seParts[0];
                string episode = seParts[1];
                url = $"https://rest.opensubtitles.org/search/episode-{episode}/imdbid-{imdbId}/season-{season}/sublanguageid-{language}";
            }
            else
            {
                url = $"https://rest.opensubtitles.org/search/imdbid-{code}/sublanguageid-{language}";
            }

            var headers = new Dictionary<string, string>
        {
            { "authority", "rest.opensubtitles.org" },
            { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36" },
            { "x-user-agent", "trailers.to-UA" }
        };

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var subtitles = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);
                var bestSubtitle = subtitles.OrderByDescending(x => x.ContainsKey("score") ? int.Parse(x["score"]) : 0).FirstOrDefault();
                if (bestSubtitle == null) return null;
                return $"[{JsonConvert.SerializeObject(new { lang = language, file = $"{subBaseUrl}{bestSubtitle["SubDownloadLink"]}" })}]";
            }

            return "1310";
        }

        public static async Task<string> Vscsubs(string url)
        {
            var subtitlesUrlMatch = System.Text.RegularExpressions.Regex.Match(url, @"info=([^&]+)");
            if (!subtitlesUrlMatch.Success)
                return "{}";

            string subtitlesUrlFormatted = HttpUtility.UrlDecode(subtitlesUrlMatch.Groups[1].Value);
            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(subtitlesUrlFormatted);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var subtitles = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);
                        return JsonConvert.SerializeObject(subtitles.Select(subtitle => new { lang = subtitle["label"], file = subtitle["file"] }));
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return "1310";
        }
    }
}
