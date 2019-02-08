# YoutubeExplode.Converter

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/YoutubeExplode-Converter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/YoutubeExplode-Converter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)
[![NuGet](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![NuGet](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)

YoutubeExplode.Converter is a helper library for [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) that provides an interface to download videos directly, without having to multiplex or convert streams yourself.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `Install-Package YoutubeExplode.Converter`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)

## Features

- Download and multiplex videos directly to a file
- Manually specify output format
- Select specific streams to download
- Progress reporting and cancellation
- Fully asynchronous API
- Targets .NET Framework 4.5+ and .NET Standard 2.0+

## Usage

The library uses [FFmpeg](https://ffmpeg.org) under the hood and thus requires the static link binaries in order to work (only `ffmpeg.exe`, the rest are not needed). The default `YoutubeConverter` constructor will look for it in the same directory but you can also specify the exact location.

If you don't want to add FFmpeg to your git repository, check out how it's downloaded in the test project.

**Note:** the resource usage and execution time mostly depends on whether transcoding is required. When possible, use streams that have the same container as the output format (e.g. mp4 audio/video streams for mp4 output format). Currently, YouTube only provides adaptive streams in mp4 or webm containers, with highest quality video streams (e.g. 4K) only available in webm.

### Download video

This will automatically determine the most fitting audio and video streams, download them, and process them into a single file. Audio streams are prioritized by format then by bitrate. Video streams are prioritized by video quality and framerate, then by format. If the output format is audio-only (e.g. mp3) then the video stream is skipped entirely.

The approach above will try to select streams to achieve the fastest execution speed while not sacrificing video quality.

It's recommended to use mp4 as output format where possible, because most videos provide highest quality streams in mp4 and, when the former is not the case, transcoding to mp4 is the fastest compared to other formats.

```c#
var converter = new YoutubeConverter();
await converter.DownloadVideoAsync("-qmBhoeQgv8", "video.mp4"); // output format inferred from file extension
```

### Download and mux specific streams

If you want a more fine-grained control over which streams are processed, you can select them yourself and pass them as a parameter to a different method. This lets you specify exact streams you want to use (e.g. for specific quality) while still benefiting from FFmpeg abstraction, progress reporting and cancellation support.

```c#
var client = new YoutubeClient();
var converter = new YoutubeConverter(client); // re-using the same client instance for efficiency, not required

// Get media stream info set
var mediaStreamInfoSet = await client.GetVideoMediaStreamInfosAsync("-qmBhoeQgv8");

// Select audio stream
var audioStreamInfo = mediaStreamInfoSet.Audio.WithHighestBitrate();

// Select video stream
var videoStreamInfo = mediaStreamInfoSet.Video.FirstOrDefault(s => s.VideoQualityLabel == "1080p60");

// Combine them into a collection
var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };

// Download and process them into one file
await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, "video.mp4", "mp4");
```

## Libraries used

- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [NUnit](https://github.com/nunit/nunit)
