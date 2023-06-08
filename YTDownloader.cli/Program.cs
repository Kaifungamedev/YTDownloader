using YoutubeExplode;
using YTD;
namespace YTD.cli;

class program
{

    public static async Task Main(string[] args)
    {
        YTD ytd = new();
        string res = "720p";
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
                        res = args[count + 1];
                        if (!res.EndsWith("p") && !res.Contains("p"))
                        {
                            res += "p";
                        }
                        break;
                }
                count++;
            }
            if (args[0].Contains("watch?v="))
                await ytd.downloadvideo(args[0], args, youtube, res);
            else if (args[0].Contains("playlist?list="))
            {
                var playlist = youtube.Playlists.GetVideosAsync(args[0]);
                await foreach (var video in playlist)
                {

                        await ytd.downloadvideo(video.Url, args, youtube, res);
                }
            }
        }
        Console.WriteLine("Done");
    }
}