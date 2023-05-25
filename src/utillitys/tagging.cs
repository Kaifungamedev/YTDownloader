using System;
using TagLib;

namespace YTD;

public class tagger
{
    public void setCoverArt(string filePath, string imagePath)
    {
        // Load the file
        TagLib.File file = TagLib.File.Create(filePath);

        // Get the picture
        TagLib.Picture picture = new TagLib.Picture(imagePath);

        // Create a new attachment frame, set its type to FrontCover and add the picture to it
        TagLib.Id3v2.AttachmentFrame albumCoverPictFrame = new TagLib.Id3v2.AttachmentFrame(picture);
        albumCoverPictFrame.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
        albumCoverPictFrame.Type = TagLib.PictureType.FrontCover;

        // Add the attachment frame to the file's metadata
        file.Tag.Pictures = new TagLib.IPicture[] { albumCoverPictFrame };

        // Save the changes
        file.Save();


    }
}