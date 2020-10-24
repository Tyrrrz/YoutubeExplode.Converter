# YoutubeExplode.Converter

[![Build](https://github.com/Tyrrrz/YoutubeExplode.Converter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode.Converter/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

**Project status: maintenance mode** (bug fixes only).

YoutubeExplode.Converter is an extension package for [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) that provides an interface to download and mux videos directly using FFmpeg.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `dotnet add package YoutubeExplode.Converter`

## Features

- Download adaptive videos directly to a file
- Choose specific streams to use
- Configure conversion settings
- Works with .NET Standard 2.0+ and .NET Framework 4.6.1+ (desktop only)

## Usage

This library relies on [FFmpeg](https://ffmpeg.org), which you can download [here](https://github.com/vot/ffbinaries-prebuilt). By default, `YoutubeExplode.Converter` will look for the CLI it in the probe directory (where the `dll` files are located), but you can also specify the exact location as well.

Resource usage and execution time depends mostly on whether transcoding between streams is required. When possible, use streams that have the same container as the output format (e.g. `mp4` audio/video streams for `mp4` output format). Currently, YouTube only provides adaptive streams in `mp4` or `webm` containers, with highest quality video streams (e.g. 4K) only available in `webm`.

### Download video in highest quality

The following will automatically resolve and determine the most fitting set of streams for the specified format, download them, and process into a single file: 

```c#
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();
await youtube.Videos.DownloadAsync("https://youtu.be/rQb7aIBuTvA", "video.mp4");
```

Audio streams are prioritized by format then by bitrate, while video streams are prioritized by video quality and framerate, then by format. Additionally, if the output format is a known audio-only format (e.g. `mp3` or `ogg`) then only the audio stream is downloaded.

### Configure conversion

You can use one of the overloads to configure different aspects of the conversion process:

```c#
var youtube = new YoutubeClient();

await youtube.Videos.DownloadAsync(
    "https://youtu.be/rQb7aIBuTvA", "video.mp4",
    o => o
        .SetFormat("webm") // override format
        .SetPreset(ConversionPreset.UltraFast) // change preset
        .SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
);
```

### Download specific streams

You can also skip the default strategy for determining most fitting streams and pass them directly:

```c#
var youtube = new YoutubeClient();

// Get stream manifest
var streamManifest = await youtube.Videos.Streams.GetManifestAsync("https://youtu.be/rQb7aIBuTvA");

// Select streams (1080p60 / highest bitrate audio)
var audioStreamInfo = streamManifest.GetAudio().WithHighestBitrate();
var videoStreamInfo = streamManifest.GetVideo().FirstOrDefault(s => s.VideoQualityLabel == "1080p60");
var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

// Download and process them into one file
await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder("video.mp4").Build());
```