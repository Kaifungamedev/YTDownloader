using YoutubeExplode;
using YTD;
namespace YTD.cli;

class program
{
    
    public static async Task Main(string[] args)
    {   YTD ytd = new();
        if (args.Length >= 2)
        {
            Console.Title = "YTD";
            var youtube = new YoutubeClient();
            int count = 0;
            foreach (var i in args)
            {
                switch (i)
                {
                    case "--audio":
                        ytd.audio = true;
                        break;
                    case "-res" or "--resolution":
                        ytd.resolution = args[count + 1];
                        if (!ytd.resolution.EndsWith("p"))
                        {
                            ytd.resolution += "p";
                        }
                        break;
                }
                count++;
            }
            if (args[0].Contains("watch?v="))
                await ytd.downloadvideo(args[0], args, youtube);
            else if (args[0].Contains("playlist?list="))
            {
                var playlist = youtube.Playlists.GetVideosAsync(args[0]);
                await foreach (var video in playlist)
                {
                    if (!File.Exists($"{ytd.configTitle(video.Title)}.{args[1]}"))
                    {
                        await ytd.downloadvideo(video.Url, args, youtube);
                    }
                }
            }
        }
    }
}