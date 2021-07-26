# YoutubeExplode.Converter

[![Build](https://github.com/Tyrrrz/YoutubeExplode.Converter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode.Converter/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

⚠️ **Project status: maintenance mode** (bug fixes only).

YoutubeExplode.Converter is an extension package for [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) that provides an interface to download and mux videos directly using FFmpeg.

## Download

📦 [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `dotnet add package YoutubeExplode.Converter`

## Usage

> Note that this library relies on [FFmpeg](https://ffmpeg.org) binaries, which you can download [here](https://github.com/vot/ffbinaries-prebuilt).
By default, YoutubeExplode.Converter will look for FFmpeg in the probe directory (where the application's `dll` files are located), but you can also specify the exact location as well.

### Downloading a video in highest quality

YoutubeExplode.Converter can be used through one of the extension methods provided on `VideoClient`.
For example, to download a video in the best available quality, simply call `DownloadAsync(...)` with the video ID and the destination file path:

```c#
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();
await youtube.Videos.DownloadAsync("https://youtube.com/watch?v=u_yIGGhubZs", "video.mp4");
```

Under the hood, this resolves available media streams and selects the best candidates based on bitrate, quality, and framerate.
If the specified output format is a known audio-only container (e.g. `mp3` or `ogg`) then only the audio stream is downloaded.

> Resource usage and execution time depends mostly on whether transcoding between streams is required.
When possible, use streams that have the same container as the output format (e.g. `mp4` audio/video streams for `mp4` output format).
Currently, YouTube only provides adaptive streams in `mp4` or `webm` containers, with highest quality video streams (e.g. 4K) only available in `webm`.

### Custom conversion options

You can configure various aspects pertaining to the conversion process by using one of the overloads of `DownloadAsync(...)`:

```c#
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();

await youtube.Videos.DownloadAsync(
    "https://youtube.com/watch?v=u_yIGGhubZs",
    "video.mp4",
    o => o
        .SetFormat("webm") // override format
        .SetPreset(ConversionPreset.UltraFast) // change preset
        .SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
);
```

### Specifying streams manually

If you need precise control over which streams are used for conversion, you can specify them directly as well:

```c#
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();

// Get stream manifest
var streamManifest = await youtube.Videos.Streams.GetManifestAsync("u_yIGGhubZs");

// Select streams (1080p60 / highest bitrate audio)
var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
var videoStreamInfo = streamManifest.GetVideoStreams().First(s => s.VideoQuality.Label == "1080p60");
var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

// Download and process them into one file
await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder("video.mp4").Build());
```
