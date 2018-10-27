# YoutubeExplode.Converter

[![Build](https://img.shields.io/appveyor/ci/Tyrrrz/YoutubeExplode-Converter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)
[![Tests](https://img.shields.io/appveyor/tests/Tyrrrz/YoutubeExplode-Converter/master.svg)](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)
[![NuGet](https://img.shields.io/nuget/v/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)
[![NuGet](https://img.shields.io/nuget/dt/YoutubeExplode.Converter.svg)](https://nuget.org/packages/YoutubeExplode.Converter)

YoutubeExplode.Converter is a helper library for [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) that provides an interface to download videos directly, without having to multiplex or convert streams manually.

## Download

- [NuGet](https://nuget.org/packages/YoutubeExplode.Converter): `Install-Package YoutubeExplode.Converter`
- [Continuous integration](https://ci.appveyor.com/project/Tyrrrz/YoutubeExplode-Converter)

## Features

- Download and multiplex videos directly to a file
- Manually specify output format
- Progress reporting and cancellation
- Fully asynchronous API
- Targets .NET Framework 4.5+ and .NET Standard 2.0+

## Usage

The library uses [FFmpeg](https://ffmpeg.org) under the hood and thus requires the static link binaries in order to work (only `ffmpeg.exe`, the rest are not needed). The default `YoutubeConverter` constructor will look for it in the same directory but you can also specify the exact location.

##### Download video

This will download highest quality audio and video streams and mux them into one file. Transcoding is skipped if the output format is `mp4`.

```c#
var converter = new YoutubeConverter();
await converter.DownloadVideoAsync("-qmBhoeQgv8", "video.mp4");
```

## Libraries used

- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode)
- [CliWrap](https://github.com/Tyrrrz/CliWrap)
- [NUnit](https://github.com/nunit/nunit)