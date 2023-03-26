using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Playlists;
using System.IO;
using YoutubeExplode.Videos;

int num = 1;
string extention = "ogg";
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
Playlist getplaylistinfo;
bool checkfile(string title)
{
    if (File.Exists($"{Directory.GetCurrentDirectory()}/{title}.{extention}"))
        return false;

    else
        return true;
}
var youtube = new YoutubeClient();

Console.WriteLine("youtube url");
string response = Console.ReadLine();
if (response is not null)
{
    System.IO.Directory.CreateDirectory(@$"img");
    if (response.Contains("watch?v=")){
        var video = await youtube.Videos.GetAsync(response.ToString());
        Console.WriteLine($"downloading: {video.Title}");
        string title = configTitle(video.Title);
        await youtube.Videos.DownloadAsync(video.Url, $"{title}.{extention}");
        Console.WriteLine("geting Thumbnail");
        using (var client = new HttpClient())
        {
            var url = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
            System.IO.File.WriteAllBytes($"img/{title}.jpg", url);
        }
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
    else
    {
        getplaylistinfo = await youtube.Playlists.GetAsync(response);
        Console.WriteLine(getplaylistinfo.Title);
        await foreach (var video in youtube.Playlists.GetVideosAsync(
            response
        ))
        {
            try
            {
                if (checkfile(configTitle(video.Title)))
                {
                    Console.WriteLine($"downloading: {video.Title} {num}");
                    string title = configTitle(video.Title);
                    await youtube.Videos.DownloadAsync(video.Url, $"{title}.{extention}");
                    Console.WriteLine("geting Thumbnail");
                    using (var client = new HttpClient())
                    {
                        var url = await client.GetByteArrayAsync($"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg");
                        System.IO.File.WriteAllBytes($"img/{title}.jpg", url);
                    }
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
            catch (YoutubeExplode.Exceptions.VideoUnavailableException e)
            {
                Console.WriteLine("unable to find video skiping {0}", e.Message);
            }

        }
    }
    Console.WriteLine("done");
}