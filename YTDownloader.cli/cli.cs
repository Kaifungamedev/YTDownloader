using YoutubeExplode;
namespace YTD.cli
{

    public class Cli
    {

        public async Task cli(string[] args)
        {
            YTD ytd = new();
            string res = "720p";
            string[] audio_formats = { "mp3", "oga", "wav", "flac", "acc", "alac", "wma", "pcm" };
            if (args.Length >= 2)
            {
                Console.Title = "YTDownloader";
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
                if (!ytd.audio)
                {

                    foreach (var item in audio_formats)
                    {

                        if (args[1] == item)
                        {
                            ytd.audio = true;
                        }
                    }
                }

                if (args[0].Contains("watch?v="))
                    await ytd.downloadvideo(args[0], args, youtube, res);
                else if (args[0].Contains("playlist?list="))
                {
                    var playlist = youtube.Playlists.GetVideosAsync(args[0]);
                    var playlist_data = await youtube.Playlists.GetAsync(args[0]);
                    Console.WriteLine(playlist_data.Title);
                    int index = 1;
                    await foreach (var video in playlist)
                    {
                        if (!File.Exists(@$"{ytd.configTitle(video.Title)}.{args[1]}"))
                        {
                            Console.Write(index + " ");
                            await ytd.downloadvideo(video.Url, args, youtube, res);
                        }
                        index++;
                    }
                }
                Console.WriteLine("Done");
            }
            else if (args.Length >= 0 || (args[0] == "-h" || args[0] == "-help"))
            {

                Console.WriteLine("path/to/YoutubeD <'youtube url in quotes'> <file extension> [optional args]");
                Console.WriteLine();
                Console.WriteLine("optional args");
                Console.WriteLine();
                Console.WriteLine("--audio...................downloads only the audio");
                Console.WriteLine("-res, --resolution........sets video resolution (default '720p')");
                Console.WriteLine("-h,--help.................displays this message");

            }

        }
    }
}