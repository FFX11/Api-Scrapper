using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api_Scrapper.Models
{
    public static class Superembed
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<(string, List<Dictionary<string, string>>)> HandleSuperembed(string location, string source, int seed)
        {
            var headers = new Dictionary<string, string>
                {
                    { "User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36" }
                };

            HttpResponseMessage req = await _httpClient.GetAsync(location, HttpCompletionOption.ResponseHeadersRead);
            string reqContent = await req.Content.ReadAsStringAsync();

            Match hunterArgs = Regex.Match(reqContent, @"eval\(function\(h,u,n,t,e,r\).*?}\((.*?)\)\)");
            if (!hunterArgs.Success)
                return ($"1308 {location}", new List<Dictionary<string, string>> { });

            string[] processedHunterArgs = ProcessHunterArgs(hunterArgs.Groups[1].Value);
            string unpacked = HunterDecoder.Hunter(processedHunterArgs);

            List<Dictionary<string, string>> subtitles = new List<Dictionary<string, string>>();
            List<string> hlsUrls = new List<string>();

            foreach (Match hlsUrlMatch in Regex.Matches(unpacked, @"file:""([^""]*)"""))
            {
                hlsUrls.Add(hlsUrlMatch.Groups[1].Value);
            }

            Match subtitleMatch = Regex.Match(unpacked, @"subtitle:""([^""]*)""");
            if (subtitleMatch.Success)
            {
                foreach (Match subtitleDataMatch in Regex.Matches(subtitleMatch.Groups[1].Value, @"\[(.*?)\](.*)$"))
                {
                    subtitles.Add(new Dictionary<string, string>
                {
                    { "lang", subtitleDataMatch.Groups[1].Value },
                    { "file", subtitleDataMatch.Groups[2].Value }
                });
                }
            }

            return (hlsUrls.Count > 0 ? hlsUrls[0] : "", subtitles);
        }

        private static string[] ProcessHunterArgs(string hunterArgs)
        {
            string[] args = hunterArgs.Split(',');
            args[0] = args[0].Trim('\"');
            args[1] = args[1].Trim();
            args[2] = args[2].Trim('\"');
            args[3] = args[3].Trim();
            args[4] = args[4].Trim();
            args[5] = args[5].Trim();

            return args;
        }
    }

    public static class HunterDecoder
    {
        public static string Hunter(params string[] hunterArgs)
        {
            // Implement the Hunter decoding logic here
            return ""; // Placeholder return
        }
    }
}
