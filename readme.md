# YTDownloader
YTD (YouTube Downloader) is a CLI tool for downloading youtube videos.

## usage
`path/to/YoutubeD <"youtube url in quotes"> <file extension> [optional args]`

### optional arguments

--audio...................downloads only the audio  
-res, --resolution........sets video resolution (default "720p")  
-h,--help.................displays this help message 
> **Note**:
> When using -res you can change fps by adding fps at the end of video resolution (eg. 1080p60) it will throw an error if the video does not support the target fps
