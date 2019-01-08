### v1.1 (08-Jan-2019)

- Added an overload of `DownloadVideoAsync` that takes `MediaStreamInfoSet` and `VideoQuality` as parameters. It can be used if you are manually resolving the `MediaStreamInfoSet` with YoutubeExplode and want to download specific video quality.
- Made progress reporting slightly more linear by taking into account whether transcoding is required.

### v1.0.3 (20-Dec-2018)

- Fixed an issue where temporary files were not deleted after the download was canceled. The underlying issue was fixed in CliWrap v2.2.

### v1.0.2 (17-Dec-2018)

- Fixed some incompatibility issues with YoutubeExplode v4.6.x.

### v1.0.1 (27-Oct-2018)

- Fixed an issue where downloading would fail if the output file path contained spaces.