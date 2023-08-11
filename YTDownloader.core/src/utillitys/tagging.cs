using System;
using TagLib;

namespace YTD;

public class Tagger
{
    public void SetCoverArt(string filePath, string imagePath)
    {
        // Load the file
        TagLib.File file = TagLib.File.Create(filePath);
        TagLib.Picture pic = new TagLib.Picture
        {
            Type = TagLib.PictureType.FrontCover,
            Description = "Cover",
            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
            Data = TagLib.ByteVector.FromPath(imagePath)
        };
        file.Tag.Pictures = new TagLib.IPicture[] { pic };
        file.Save();

    }
}