using System.Runtime.InteropServices;
using System.Data;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Common;
namespace YTD;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public class YTD
{
    public string? resolution;
    public bool audio;
    public string configTitle(string name)
    {
        string[] illegalChars = { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
        string title = name;
        foreach (string illegalChar in illegalChars)
        {
            title = title.Replace(illegalChar, "");
        }
        return title;
    }
    public async Task downloadvideo(string url, string[] args, YoutubeClient youtube, string res = "720p")
    {
        if (resolution != null)
            resolution = res;
        var video = await youtube.Videos.GetAsync(url);
        VideoId videoId = VideoId.Parse(url);
        var extention = args[1];
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var fileName = $"{configTitle(video.Title)}.{args[1]}";
        /* This code is getting the audio stream information from the stream manifest for a specific video URL.
        It filters the available audio streams to only include those with the highest bitrate, and then
        selects the resulting audio stream information and stores it in the variable `audioStreamInfo`. */
        var audioStreamInfo = streamManifest
            .GetAudioStreams()
            .GetWithHighestBitrate();
        /* This code is getting the video stream information from the stream manifest for a specific video URL.
        It filters the available video streams to only include those with a container of WebM, and then
        selects the first stream with a video quality label that matches the specified resolution (res). The
        resulting video stream information is stored in the variable `videoStreamInfo`. */
        var videoStreamInfo = streamManifest
            .GetVideoStreams()
            .Where(s => s.Container == Container.WebM)
            .First(s => s.VideoQuality.Label == res);
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
            byte[] thumbnalebytes = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/mqdefault.jpg");
            string thumbnailpath = "icon.jpg";
            System.IO.File.WriteAllBytes(thumbnailpath, thumbnalebytes);

            t.setCoverArt(fileName, thumbnailpath);
            System.IO.File.Delete("icon.jpg");

        }

        Console.WriteLine("Done");
        Console.WriteLine($"Video saved to '{fileName}'");

    }
}