# YoutubeExplode.Converter

[![Build](https://github.com/Tyrrrz/YoutubeExplode.Converter/workflows/CI/badge.svg?branch=master)](https://github.com/Tyrrrz/YoutubeExplode.Converter/actions)
[![Coverage](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter/branch/master/graph/badge.svg)](https://codecov.io/gh/Tyrrrz/YoutubeExplode.Converter)
[![Version](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Downloads](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![Donate](https://img.shields.io/badge/donate-$$$-purple.svg)](https://tyrrrz.me/donate)

YoutubeExplode.Converter is an extension package for [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) that provides an interface to download and convert videos directly, with the help of FFmpeg.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `dotnet add package YoutubeExplode.Converter`

## Features

- Download adaptive videos directly to a file
- Choose specific streams to download
- Configure conversion settings
- Works with .NET Standard 2.0+ and .NET Framework 4.6.1+ (desktop only)

## Usage

The library relies [FFmpeg](https://ffmpeg.org) under the hood and thus requires the CLI in order to work. You can download them [here](https://github.com/vot/ffbinaries-prebuilt). By default, `YoutubeExplode.Converter` will look for it in the probe directory (where the `dll` files are) but you can also specify the exact location with one of the overloads.

**Note:** the resource usage and execution time mostly depends on whether transcoding is required. When possible, use streams that have the same container as the output format (e.g. mp4 audio/video streams for mp4 output format). Currently, YouTube only provides adaptive streams in mp4 or webm containers, with highest quality video streams (e.g. 4K) only available in webm.

### Download video in highest quality

This will automatically resolve and determine the most fitting set of streams for the specified format, download them, and process into a single file. Audio streams are prioritized by format then by bitrate, while video streams are prioritized by video quality and framerate, then by format.

If the output format is audio-only (e.g. `mp3` or `ogg`) then only the audio stream is downloaded. The strategy above aims for the highest quality output with the most efficient conversion options.

It's recommended to use mp4 as output format where possible, because most videos provide highest quality streams in mp4 and, when the former is not the case, transcoding to mp4 is the fastest compared to other formats.

```c#
using YoutubeExplode;
using YoutubeExplode.Converter;

var youtube = new YoutubeClient();
await youtube.Videos.DownloadAsync("-qmBhoeQgv8", "video.mp4");
```

### Configure conversion

You can use one of the overloads to configure different aspects of the conversion process:

```c#
var youtube = new YoutubeClient();

await youtube.Videos.DownloadAsync(
    new ConversionRequestBuilder("video.mp4")
        .SetFormat("webm") // override format
        .SetPreset(ConversionPreset.UltraFast) // change preset
        .SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
        .Build()
);
```

### Download specific streams

You can also override the default strategy for determining most fitting streams by passing them directly:

```c#
var youtube = new YoutubeClient();

// Get stream manifest
var streamManifest = await youtube.Videos.Streams.GetManifestAsync("-qmBhoeQgv8");

// Select audio stream
var audioStreamInfo = streamManifest.GetAudio().WithHighestBitrate();

// Select video stream
var videoStreamInfo = streamManifest.GetVideo().FirstOrDefault(s => s.VideoQualityLabel == "1080p60");

// Combine them into a collection
var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };

// Download and process them into one file
await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder("video.mp4").Build());
```