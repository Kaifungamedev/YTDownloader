using YoutubeExplode.Videos.Streams;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using Xabe.FFmpeg;
using Luna.ConsoleProgressBar;
namespace YTD;


public class YTD
{
    public string resolution;
    public readonly string[] audio_formats = { "mp3", "oga", "wav", "flac", "acc", "alac", "wma", "pcm" };
    public bool audio;
    /// <summary>
    /// The function `configTitle` takes a string `name` as input and replaces any illegal characters with
    /// underscores, then returns the modified string.
    /// </summary>
    /// <param name="name">The name parameter is a string that represents the title of a
    /// configuration.</param>
    /// <returns>
    /// The method is returning a string that has replaced any illegal characters in the input name with
    /// underscores.
    /// </returns>
    public string ConfigTitle(string name)
    {
        string[] illegalChars = { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
        string title = name;
        foreach (string illegalChar in illegalChars)
        {
            title = title.Replace(illegalChar, "_");
        }
        return title;
    }
    public async Task Downloadvideo(string url, string[] args, YoutubeClient youtube, string res = "720p")
    {
        var video = await youtube.Videos.GetAsync(url);
        var extension = args[1];
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);
        
        var title = ConfigTitle(video.Title);
        // Name of output file
        var fileName = $"{title}.{extension}";
        /* The code is iterating through each item in the `audio_formats` array. It checks if the `extension`
        variable is equal to the current item in the iteration. If it is, it sets the `audio` variable to
        `true`. This code is used to determine if the file extension corresponds to an audio format. */
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
        catch (InvalidOperationException ex)
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
                    File.Delete($"{title}.{audioStreamInfo.Container}.tmp");
                }
                catch (Xabe.FFmpeg.Exceptions.ConversionException)
                {
                    Console.WriteLine("Unable to convert");
                    File.Move($"{title}.{audioStreamInfo.Container}.tmp",
                        $"{title}.{audioStreamInfo.Container}");
                }
            }
            else
            {

                // Download and mux streams into a single file
                var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

                await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder(fileName).SetPreset(YoutubeExplode.Converter.ConversionPreset.VerySlow).Build(), progress);
            }
            Tagger t = new();
            using var client = new HttpClient();
            byte[] thumbnalebytes = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
            string thumbnailpath = "icon.jpg";
            File.WriteAllBytes(thumbnailpath, thumbnalebytes);
            t.SetCoverArt(fileName, thumbnailpath);
            bool icon_deleted = false;
            int tries = 0;
            while (!icon_deleted && tries <= 5)
            {
                try
                {
                    File.Delete("icon.jpg");
                    icon_deleted = true;

                }
                catch (UnauthorizedAccessException)
                {
                    tries++;
                }
            }
        }
        Console.WriteLine();
    }

}