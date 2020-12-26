### v2.0.2 (26-Dec-2020)

- Fixed `NullReferenceException` that occurred when using latest version of CliWrap.

### v2.0.1 (26-Dec-2020)

- Exceptions thrown when executing FFmpeg now also contain the process stderr buffer.
- Bumped all dependency versions to latest.

### v2.0 (15-Oct-2020)

- Reworked the library. Please refer to the readme for updated usage.

### v1.5.1 (22-Apr-2020)

- Fixed an issue where progress was not correctly reported when there was transcoding involved.

### v1.5 (13-Apr-2020)

- Added ability to specify a preset that influences conversion speed and output file size.

### v1.4.4 (12-Apr-2020)

- Added support for YoutubeExplode v5.

### v1.4.3 (27-Jul-2019)

- Fixed an issue where `NullReferenceException` was thrown in `DownloadVideoAsync` on videos that don't have adaptive streams.

### v1.4.2 (12-May-2019)

- Updated YoutubeExplode dependency to version 4.7, which resolves critical issues related to recent YouTube changes.

### v1.4.1 (03-Mar-2019)

- Fixed an issue where `DownloadAndProcessMediaStreamsAsync` would sometimes report progress above 1.

### v1.4 (17-Jan-2019)

- Reworked automatic stream selection in `DownloadVideoAsync` so that it prioritizes video streams by highest quality and only then by format. This ensures that it always downloads the highest quality video stream even if the input format doesn't match the output format.

### v1.3 (16-Jan-2019)

- Removed quality preferences and instead added a public method `DownloadAndProcessMediaStreamsAsync` that takes a list of `MediaStreamInfo`s as a parameter. That way the consumers can find the most fitting combination of audio and video streams (or just a single stream in case of audio-only or video-only output) and just pass them as a parameter. The higher level method `DownloadVideoAsync` will always attempt to find the highest possible quality streams.

### v1.2 (09-Jan-2019)

- Added preferred framerate as a parameter to one of the overloads of `DownloadVideoAsync`, alongside preferred video quality.
- Reworked the way preferences are passed, they are now optional parameters.
- An exception of type `MediaStreamInfoNotFoundException` will now be thrown when there were no streams in the set that match input preferences.
- Made progress reporting even more linear.

### v1.1 (08-Jan-2019)

- Added an overload of `DownloadVideoAsync` that takes `MediaStreamInfoSet` and `VideoQuality` as parameters. It can be used if you are manually resolving the `MediaStreamInfoSet` with YoutubeExplode and want to download specific video quality.
- Made progress reporting slightly more linear by taking into account whether transcoding is required.

### v1.0.3 (20-Dec-2018)

- Fixed an issue where temporary files were not deleted after the download was canceled. The underlying issue was fixed in CliWrap v2.2.

### v1.0.2 (17-Dec-2018)

- Fixed some incompatibility issues with YoutubeExplode v4.6.x.

### v1.0.1 (27-Oct-2018)

- Fixed an issue where downloading would fail if the output file path contained spaces.