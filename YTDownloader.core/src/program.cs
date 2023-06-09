using YoutubeExplode.Videos.Streams;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using Xabe.FFmpeg;
using Luna.ConsoleProgressBar;
namespace YTD;

// This demo prompts for video ID and downloads one media stream.
// It's intended to be very simple and straight to the point.
// For a more involved example - check out the WPF demo.
public class YTD
{
    public string resolution;
    string[] audio_formats = { "mp3", "oga", "wav", "flac", "acc", "alac", "wma", "pcm" };
    public bool audio;
    public string configTitle(string name)
    {
        string[] illegalChars = { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
        string title = name;
        foreach (string illegalChar in illegalChars)
        {
            title = title.Replace(illegalChar, "_");
        }
        return title;
    }
    public async Task downloadvideo(string url, string[] args, YoutubeClient youtube, string res = "72060p")
    {
        var video = await youtube.Videos.GetAsync(url);
        var extension = args[1];
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);
        var title = configTitle(video.Title);
        var fileName = $"{title}.{extension}";
        foreach (var item in audio_formats)
        {
            if (extension == item)
            {
                audio = true;
            }
        }
        /* This code is getting the audio stream information from the stream manifest for a specific video URL.
        It filters the available audio streams to only include those with the highest bitrate, and then
        selects the resulting audio stream information and stores it in the variable `audioStreamInfo`. */
        var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        /* This code is getting the video stream information from the stream manifest for a specific video URL.
        It filters the available video streams to only include those with a container of WebM, and then
        selects the first stream with a video quality label that matches the specified resolution (res). The
        resulting video stream information is stored in the variable `videoStreamInfo`. */
        IVideoStreamInfo videoStreamInfo = null;
        try
        {
            videoStreamInfo = streamManifest
                .GetVideoStreams()
                .First(s => s.VideoQuality.Label == res);

        }
        catch (System.InvalidOperationException ex)
        {
            if (!audio)
            {
                Console.WriteLine($"ERROR (likely to do unsupported fps) {ex.Message}");
                return;
            }
        }
        catch (Exception ex)
        {

            if (!audio)
            {
                Console.WriteLine($" unable to find video stream skiping {ex}");
                return;
            }
        }
        var download_res = audio ? "audio" : videoStreamInfo.VideoQuality.Label;
        Console.Write(
             $"Downloading: {title} {download_res} / {extension} ");
        using (var progress = new ConsoleProgressBar
        {
            DisplayBars = false,
            AnimationSequence = UniversalProgressAnimations.PulsingLine,
            ForegroundColor = ConsoleColor.Red
        })
        {
            if (audio)
            {
                var stream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
                // Download the stream to a file
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, $"{title}.{audioStreamInfo.Container}.tmp", progress);
                string outputPath = $"{title}.{extension}";
                var mediaInfo = await FFmpeg.GetMediaInfo($@"{title}.{audioStreamInfo.Container}.tmp");
                try
                {
                    var conversion = await FFmpeg.Conversions.New().AddStream(mediaInfo.Streams).SetOutput(outputPath).Start();
                    System.IO.File.Delete($"{title}.{audioStreamInfo.Container}.tmp");
                }
                catch (Xabe.FFmpeg.Exceptions.ConversionException)
                {
                    Console.WriteLine("Unable to convert");
                    System.IO.File.Move($"{title}.{audioStreamInfo.Container}.tmp",
                        $"{title}.{audioStreamInfo.Container}");
                }
            }
            else
            {

                // Download and mux streams into a single file
                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(fileName).SetPreset(YoutubeExplode.Converter.ConversionPreset.VerySlow).Build(), progress);
            }
            tagger t = new();
            using (var client = new HttpClient())
            {
                byte[] thumbnalebytes = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                string thumbnailpath = "icon.jpg";
                System.IO.File.WriteAllBytes(thumbnailpath, thumbnalebytes);
                t.setCoverArt(fileName, thumbnailpath);
                bool icon_deleted = false;
                int tries = 0;
                while (!icon_deleted && tries <= 5)
                {
                    try
                    {
                        System.IO.File.Delete("icon.jpg");
                        icon_deleted = true;

                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        tries++;
                    }
                }


            }
        }
        Console.WriteLine();
    }

}