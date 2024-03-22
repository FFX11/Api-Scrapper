using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api_Scrapper.Models
{
    public class Filemoon
    {
        private static readonly HttpClient client = new HttpClient();

        public async static Task<Dictionary<string, string>> HandleFilemoon(string url)
        {

            var req = await client.GetAsync(url);
            var matches = Regex.Match(await req.Content.ReadAsStringAsync(), @"return p}\((.+)\)");

            var processedMatches = new List<dynamic>();

            if (!matches.Success)
            {
                for (int i = 0; i < 10; i++)
                {
                    req = await client.GetAsync(url);
                    matches = Regex.Match(await req.Content.ReadAsStringAsync(), @"return p}\((.+)\)");
                    if (matches.Success)
                        break;
                }
            }

            if (!matches.Success)
                return new Dictionary<string, string> { { "file", null }, { "sub", "1402" } };

            var splitMatches = matches.Groups[1].Value.Split(",");
            var correctedSplitMatches = new List<string> { string.Join(",", splitMatches[0..^3]) };
            correctedSplitMatches.AddRange(splitMatches[^3..]);

            foreach (var val in correctedSplitMatches)
            {
                var processedVal = val.Trim().Replace(".split('|'))", "");
                if (int.TryParse(processedVal, out int intVal) || (processedVal.StartsWith('-') && int.TryParse(processedVal.Substring(1), out _)))
                {
                    processedMatches.Add(intVal);
                }
                else if (processedVal.StartsWith("'") && processedVal.EndsWith("'"))
                {
                    processedMatches.Add(processedVal[1..^1]);
                }
            }

            var lastProcessedMatch = processedMatches[^1];
            if (lastProcessedMatch is string)
                processedMatches[^1] = lastProcessedMatch.Split("|");

            var unpacked = Unpack(processedMatches.ToArray());
            var hlsUrl = Regex.Match(unpacked, @"file:""([^""]*)""").Groups[1].Value;

            return new Dictionary<string, string> { { "file", hlsUrl }, { "sub", "1404" } };
        }

        private static string Unpack(params object[] args)
        {
            var p = (string)args[0];
            var a = (int)args[1];
            var c = (int)args[2];
            var k = (string[])args[3];
            var e = args.Length >= 5 ? (int?)args[4] : null;
            var d = args.Length >= 6 ? (int?)args[5] : null;

            for (int i = c - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(k[i]))
                    p = Regex.Replace(p, "\\b" + IntToBase(i, a) + "\\b", k[i]);
            }

            return p;
        }

        private static string IntToBase(int x, int baseValue)
        {
            const string charset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/";

            int sign;
            if (x < 0)
                sign = -1;
            else if (x == 0)
                return "0";
            else
                sign = 1;

            x *= sign;
            var digits = new List<char>();

            while (x != 0)
            {
                digits.Add(charset[x % baseValue]);
                x = x / baseValue;
            }

            if (sign < 0)
                digits.Add('-');

            digits.Reverse();
            return new string(digits.ToArray());
        }
    }
}
