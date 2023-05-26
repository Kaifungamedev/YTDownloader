using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Common;
namespace YTD;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public class Program
{
    static string resolution = "720p30";
    static bool audio;
    static string configTitle(string name)
    {
        return name.Replace(",", "").Replace("*", "").Replace("?", "").Replace("|", "").Replace("/", "").Replace(":", "").Replace(">", "").Replace("<", "").Replace('"', ' ').Replace("'", "");
    }
    public static async Task downloadvideo(string url, string[] args, YoutubeClient youtube)
    {
        var video = await youtube.Videos.GetAsync(url);
        VideoId videoId = VideoId.Parse(url);
        var extention = args[1];
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
            .First(s => s.VideoQuality.Label == resolution)
            ;
        var download_res = audio ? "audio" : videoStreamInfo.VideoQuality.Label;
        Console.Write(
             $"Downloading {video.Title}: {download_res} / {extention} "
         );
        using (var progress = new ConsoleProgress())
        {
            if (audio)
            {
                var stream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
                // Download the stream to a file
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, fileName, progress);
            }
            else
            {

                // Download and mux streams into a single file
                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(fileName).SetPreset(ConversionPreset.VerySlow).Build(), progress);
            }
        }

        Console.WriteLine("getting thumbnail");
        tagger t = new();
        using (var client = new HttpClient())
        {
            byte[] thumbnalebyts = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
            Directory.CreateDirectory("img");
            string thumbnailpath;
            using (var httpclient = new HttpClient())
            {
                var thumbnail = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                thumbnailpath = $"img/{configTitle(video.Title).Replace(" ", "_")}.jpg";
                System.IO.File.WriteAllBytes(thumbnailpath, thumbnail);
            }
            t.setCoverArt(fileName, thumbnailpath);

        }

        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");

    }
    public static async Task Main(string[] args)
    {
        if (args.Length >= 2)
        {
            Console.Title = "YTD";
            var youtube = new YoutubeClient();
            int count = 0;
            if (args[1] == "ogg" || args[1] == "oga" || args[1] == "ogv")
            {
                Console.WriteLine("ogg file format not suported");
                System.Environment.Exit(1);
            }
            foreach (var i in args)
            {
                switch (i)
                {
                    case "--audio":
                        audio = true;
                        break;
                    case "-r" or "--resolution":
                        resolution = args[count + 1];
                        if (!resolution.Contains("p"))
                        {
                            resolution += "p";
                        }
                        break;
                }
                count++;
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