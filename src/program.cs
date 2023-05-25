using System.IO;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Common;
using CliWrap;
using CliWrap.Buffered;
namespace YTD;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public class Program
{
    static bool audio;
    static string configTitle(string name)
    {
        return name.Replace(",", "").Replace("*", "").Replace("?", "").Replace("|", "").Replace("/", "").Replace(":", "").Replace(">", "").Replace("<", "").Replace('"', ' ').Replace("'", "");
    }
    public static async Task downloadvideo(string url, string[] args, YoutubeClient youtube)
    {
        Console.WriteLine(audio);
        var video = await youtube.Videos.GetAsync(url);
        VideoId videoId = VideoId.Parse(url);
        var extention = args[1] == "ogg" && audio ? "ogg audio" : "ogg";
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var fileName = $"{configTitle(video.Title)}.{args[1]}";
        // Select best audio stream (highest bitrate)
        var audioStreamInfo = streamManifest
            .GetAudioStreams()
            .GetWithHighestBitrate();

        // Select best video stream (1080p60 in this example)
        var videoStreamInfo = streamManifest
            .GetVideoStreams()
            .Where(s => s.Container == Container.WebM)
            //.GetWithHighestVideoQuality()
            .First(s => s.VideoQuality.Label == "144p")
            ;

        // Download and mux streams into a single file
        var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
        Console.Write(
            $"Downloading {video.Title}: {videoStreamInfo.VideoQuality} / {extention} "
        );

        using (var progress = new ConsoleProgress())
        {
            await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(fileName.Replace(".ogg", ".ogv")).Build(), progress);
        }
        if (audio)
        {
            var vfile = fileName.Replace(".ogg", ".ogv");
            Console.WriteLine(vfile);
            var result = await Cli.Wrap("ffmpeg").WithArguments(new[] { "-i", $"{vfile}", "-vn", "-acodec", "libvorbis", $"{fileName}", "-y" }).WithWorkingDirectory(".").ExecuteBufferedAsync();
            if (result.ExitCode != 0)
            {
                Console.WriteLine("error: ", result.StandardError);
                System.Environment.Exit(result.ExitCode);
            }
        }
        Console.WriteLine("getting thumbnail");
        tagger t = new();
        using (var client = new HttpClient())
        {
            byte[] thumbnalebyts = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
            Directory.CreateDirectory("img");
            using (var httpclient = new HttpClient())
            {
                var thumbnail = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                System.IO.File.WriteAllBytes($"img/{video.Title}.jpg", thumbnail);
            }
            t.setCoverArt(fileName, $"img/{video.Title}.jpg");

        }

        File.Delete(fileName.Replace(".ogg", ".ogv"));
        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");

    }
    public static async Task Main(string[] args)
    {
        if (args.Length >= 2)
        {
            Console.Title = "YTD";
            var youtube = new YoutubeClient();
            foreach (var i in args)
            {
                if (i == "--audio")
                {
                    audio = true;
                }
            }
            if (args[0].Contains("watch"))
                await downloadvideo(args[0], args, youtube);
            else if (args[0].Contains("list"))
            {
                var playlist = youtube.Playlists.GetVideosAsync(args[0]);
                await foreach (var video in playlist)
                {
                    if (!File.Exists($"{configTitle(video.Title)}.{args[1]}"))
                    {
                        await downloadvideo(video.Url, args, youtube);
                    }
                }
            }
        }
    }
}