using Api_Scrapper.Models;

public static class Program
{
    public static async Task Main(string[] args)
    {
        string id = "693134";
        await VidSrcMe.Get(id);
    }
}