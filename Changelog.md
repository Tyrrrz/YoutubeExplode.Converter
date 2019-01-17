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