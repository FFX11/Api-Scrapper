using System;
using System.Text;

public static class HunterDecoder
{
    public static int HunterDef(string d, int e, int f)
    {
        string charset = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/";
        string sourceBase = charset.Substring(0, e);
        string targetBase = charset.Substring(0, f);

        char[] reversedInput = d.ToCharArray();
        Array.Reverse(reversedInput);
        int result = 0;

        for (int power = 0; power < reversedInput.Length; power++)
        {
            char digit = reversedInput[power];
            if (sourceBase.Contains(digit))
            {
                result += sourceBase.IndexOf(digit) * (int)Math.Pow(e, power);
            }
        }

        StringBuilder convertedResult = new StringBuilder();
        while (result > 0)
        {
            convertedResult.Insert(0, targetBase[result % f]);
            result = (result - (result % f)) / f;
        }

        return Convert.ToInt32(convertedResult.ToString()) != 0 ? Convert.ToInt32(convertedResult.ToString()) : 0;
    }

    public static string Hunter(string h, string u, string n, int t, int e, string r)
    {
        int i = 0;
        StringBuilder resultStr = new StringBuilder();
        while (i < h.Length)
        {
            int j = 0;
            StringBuilder s = new StringBuilder();
            while (h[i] != n[e])
            {
                s.Append(h[i]);
                i++;
            }

            while (j < n.Length)
            {
                s.Replace(n[j].ToString(), j.ToString());
                j++;
            }

            resultStr.Append((char)(HunterDef(s.ToString(), e, 10) - t));
            i++;
        }

        return resultStr.ToString();
    }
}
