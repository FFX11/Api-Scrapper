using RandomUserAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api_Scrapper.Models
{
    public class VidPlay
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<Dictionary<string, string>> HandleVidplay(string url)
        {
            string furl = url;
            string[] urlParts = url.Split('?');
            var subtitles = await Subtitle.Vscsubs(urlParts[1]);
            string userAgent = RandomUa.RandomUserAgent;

            Random rand = new Random();
            string ip = $"{rand.Next(1, 256)}.{rand.Next(0, 256)}.{rand.Next(0, 256)}.{rand.Next(0, 256)}";

            var keys = await GetKeys();

            var decodedId = DecodeData(keys[0], urlParts[0].Split("/e/")[^1]);
            var encodedResult = DecodeData(keys[1], Encoding.UTF8.GetString(decodedId));
            string encodedBase64 = Convert.ToBase64String(encodedResult).Replace('/', '_');



            var req = await _httpClient.GetAsync("https://vidplay.online/futoken?Referer=" + url);
            string fuKey = Regex.Match(await req.Content.ReadAsStringAsync(), @"var\s+k\s*=\s*'([^']+)'").Groups[1].Value;
            string data = $"{fuKey},{string.Join(",", Encoding.UTF8.GetBytes(fuKey).Zip(Encoding.UTF8.GetBytes(encodedBase64), (a, b) => a + b))}";


            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString["v"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var requestUri = $"https://vidplay.online/mediainfo/{data}?{queryString}&autostart=true";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Origin", ip);
            //request.Headers.Add("Referer", "of5lMs1n");
            request.Headers.Add("Host", "vidplay.online");
            request.Headers.Add("User-Agent", rand.ToString());

            var response = await _httpClient.SendAsync(request);
            var reqData = await response.Content.ReadAsStringAsync();





            if (reqData.Contains("\"result\":{"))
            {
                string src = Regex.Match(reqData, "\"file\":\"([^\"]+)\"").Groups[1].Value.Replace("#.mp4", "");
                string thumbnails = Regex.Match(reqData, "\"file\":\"([^\"]+)\"").Groups[1].Value.Replace("#.mp4", "");

                return new Dictionary<string, string>
                    {
                        { "url", src },
                        { "source", "VidsrcTo" },
                        { "proxy", "True" },
                        { "lang", "en" },
                        { "type", "hls" },
                        { "thumbnails", thumbnails }
                    };
            }
            return new Dictionary<string, string> { { "error", "1401" } };
        }


        private static byte[] DecodeData(string key, string data)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] s = new byte[256];
            byte[] t = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
                t[i] = keyBytes[i % keyBytes.Length];
            }
            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + t[i]) % 256;
                byte temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }
            int x = 0;
            int y = 0;
            byte[] decoded = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                x = (x + 1) % 256;
                y = (y + s[x]) % 256;
                byte temp = s[x];
                s[x] = s[y];
                s[y] = temp;
                int tIndex = (s[x] + s[y]) % 256;
                decoded[i] = (byte)(data[i] ^ s[tIndex]);
            }
            return decoded;
        }

        private static async Task<List<string>> GetKeys()
        {
            var response = await _httpClient.GetAsync("https://raw.githubusercontent.com/Ciarands/vidsrc-keys/main/keys.json");
            var json = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
        }
    }
}
