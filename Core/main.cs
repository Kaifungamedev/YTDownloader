using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using System;
using System.IO;

namespace YTD
{
    public class YTD
    {
        int num = 1;
        string extention = "";
        Playlist getplaylistinfo;
        bool noimg = true;
        string response = "";

        public async Ytd(string[] args)
        {
            Console.WriteLine(0);
            response = "https://www.youtube.com/playlist?list=PLda3VoSoc_TSBBOBYwcmlamF1UrjVtccZ";//args[0];
            extention = args[1];
            await Download();

        }

        public void Download()
        {
            Console.WriteLine(0);
            var youtube = new YoutubeClient();


            System.IO.Directory.CreateDirectory(@$"img");
            if (response.Contains("watch?v="))
            {
                try
                {
                    download_video(response, youtube);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + $"\n failed");
                }
            }
            else
            {
                download_playlist(response, youtube);
            }

        }
        async void download_video(string response, YoutubeClient youtube)
        {
            /* Getting the video information from the url. */
            var video = await youtube.Videos.GetAsync(response.ToString());
            Console.WriteLine($"downloading: {video.Title}");
            /* Removing all the special characters from the title. */
            string title = configTitle(video.Title);
            /* Downloading the video from the url. */
            await youtube.Videos.DownloadAsync(video.Url, $"{title}.{extention}");
            Console.WriteLine("geting Thumbnail");
            /* Downloading the thumbnail of the video. */
            using (var client = new HttpClient())
            {
                var url = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                System.IO.File.WriteAllBytes($"img/{title}.jpg", url);
            }
            /* Creating a file with the title of the video and the extention. */
            var targetFile = TagLib.File.Create($"{title}.{extention}");
            /* Creating a new instance of the class `TagLib.Id3v2.AttachedPictureFrame` and
            assigning it to the variable `pic`. */
            var pic = new TagLib.Id3v2.AttachedPictureFrame();
            /* Setting the encoding of the picture to latin1. */
            pic.TextEncoding = TagLib.StringType.Latin1;
            /* Setting the mime type of the image to jpeg. */
            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
            /* Setting the type of the picture to front cover. */
            pic.Type = TagLib.PictureType.FrontCover;
            /* Setting the data of the picture to the image that was downloaded. */
            pic.Data = TagLib.ByteVector.FromPath($"img/{title}.jpg");
            // save picture to file
            targetFile.Tag.Pictures = new TagLib.IPicture[1] { pic };
            targetFile.Save();
            System.IO.File.Delete($"{title}");
        }
        async void download_playlist(string response, YoutubeClient youtube)
        {
            getplaylistinfo = await youtube.Playlists.GetAsync(response);
            Console.WriteLine(getplaylistinfo.Title);
            var client = new HttpClient();
            await foreach (var video in youtube.Playlists.GetVideosAsync(
                response
            ))
            {
                try
                {
                    if (checkfile(configTitle(video.Title)))
                    {
                        Console.WriteLine($"downloading: {video.Title} {num}");
                        // Download video
                        string title = configTitle(video.Title);
                        await youtube.Videos.DownloadAsync(video.Url, $"{title}.{extention}");
                        // Download cover image
                        Console.WriteLine("geting Thumbnail");
                        var url = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                        System.IO.File.WriteAllBytes($"img/{title}.jpg", url);
                        var targetFile = TagLib.File.Create($"{title}.{extention}");
                        // define picture
                        TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                        pic.TextEncoding = TagLib.StringType.Latin1;
                        pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        pic.Type = TagLib.PictureType.FrontCover;
                        pic.Data = TagLib.ByteVector.FromPath($"img/{title}.jpg");

                        // save picture to file
                        targetFile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                        targetFile.Save();

                        System.IO.File.Delete($"{title}");
                    }
                    num++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + $"\n skipping");
                }

            }
            if (noimg == true)
            {
                Console.WriteLine("cleaning up");
                Directory.Delete("img", true);
                Console.WriteLine("done");
            }
            else
            {
                Console.WriteLine("done");
            }
        }
        string configTitle(string name)
        {
            string var = name.Replace(".", "");
            string var2 = var.Replace("'", "");
            string var3 = var2.Replace('"'.ToString(), "");
            string var4 = var3.Replace("<", "");
            string var5 = var4.Replace(">", "");
            string var6 = var5.Replace(":", "");
            string var7 = var6.Replace("/", "");
            string var8 = var7.Replace("|", "");
            string var9 = var8.Replace("?", "");
            string var10 = var9.Replace("*", "");
            string var11 = var10.Replace(",", "");
            return var11;
        }
        bool checkfile(string title)
        {
            return File.Exists($"{Directory.GetCurrentDirectory()}/{title}.{extention}");
        }
    }
}