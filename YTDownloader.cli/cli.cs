using YoutubeExplode;
using Tomlyn.Model;
using Tomlyn;
using System.Reflection;

namespace YTD.cli
{

    public class Cli
    {
        YTD ytd = new();
        readonly string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string res = "720p";
        string Playlistfile;

        async Task SavePlaylist(string name, string url)
        {
            await File.AppendAllLinesAsync(Playlistfile, new[] { $"\n[{ytd.ConfigTitle(name).Replace(' ', '_')}]\nurl = '{url}'" });
        }
        public async Task cli(string[] args)
        {   Playlistfile = Path.Join(exePath, "playlist.toml");
            if (!File.Exists(Path.Join(exePath, "playlist.toml")))
            {
                try
                {
                    File.Create(Path.Join(exePath, "playlist.toml")).Dispose();
                }
                catch { Console.WriteLine($"unable to make playlist file plese make a file named 'playlist.toml' at '{exePath}'"); }
            }
            else
            {
                Console.WriteLine(
                "toml file found"
            );
            }
            // Read the file contents
            string toml = File.ReadAllText(Playlistfile);
            // Parse the contents into a TomlTable object
            var model = Toml.ToModel(toml);

            string url = args[0];
            try
            {
                url = ((TomlTable)model[url]!)["url"].ToString();

            }
            catch { Console.WriteLine("unable to find url"); }
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
                        case "--save" or "-s":
                            if (url.Contains("playlist?list="))
                            {
                                var playlist = await youtube.Playlists.GetAsync(url);
                                var title = playlist.Title;
                                try
                                {
                                    if (((TomlTable)model[title]!).ToString() == null)
                                    {
                                        await SavePlaylist(title, url);
                                    }
                                }
                                catch (System.Collections.Generic.KeyNotFoundException)
                                {
                                    await SavePlaylist(title, url);
                                }
                            }
                            break;
                    }
                    count++;
                }
                if (!ytd.audio)
                {

                    foreach (var item in ytd.audio_formats)
                    {

                        if (args[1] == item)
                        {
                            ytd.audio = true;
                        }
                    }
                }
                if (url.Contains("watch?v="))
                    await ytd.Downloadvideo(url, args, youtube, res);
                else if (url.Contains("playlist?list="))
                {
                    var playlist = youtube.Playlists.GetVideosAsync(url);
                    var playlist_data = await youtube.Playlists.GetAsync(url);
                    Console.WriteLine(playlist_data.Title);
                    int index = 1;
                    await foreach (var video in playlist)
                    {
                        if (!File.Exists(@$"{ytd.ConfigTitle(video.Title)}.{args[1]}"))
                        {
                            Console.Write(index + " ");
                            await ytd.Downloadvideo(video.Url, args, youtube, res);
                        }
                        index++;
                    }
                }
                Console.WriteLine("Done");

            }
            else if (args.Length >= 0 || url == "-h" || url == "-help")
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