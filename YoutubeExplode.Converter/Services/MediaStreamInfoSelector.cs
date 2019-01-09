using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Converter.Internal;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter.Services
{
    /// <summary>
    /// Default media stream info selector.
    /// </summary>
    public partial class MediaStreamInfoSelector : IMediaStreamInfoSelector
    {
        /// <inheritdoc />
        public IReadOnlyList<MediaStreamInfo> Select(MediaStreamInfoSet mediaStreamInfoSet, string format,
            VideoQuality? preferredVideoQuality = null, int? preferredFramerate = null)
        {
            mediaStreamInfoSet.GuardNotNull(nameof(mediaStreamInfoSet));
            format.GuardNotNull(nameof(format));

            // If audio-only - get the highest quality audio
            if (IsAudioOnlyFormat(format))
            {
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                    .OrderByDescending(s => s.Bitrate) // order by bitrate
                    .First(); // take highest bitrate

                return new[] {audioStreamInfo};
            }

            // If needs video but requested video quality is 720p30 or below - use muxed stream
            if (preferredVideoQuality <= VideoQuality.High720 && preferredFramerate <= 30)
            {
                // Values for `preferredVideoQuality` and `preferredFramerate` are guaranteed to not be null in this scope

                var muxedStreamInfo = mediaStreamInfoSet.Muxed
                    .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                    .Where(s => s.VideoQuality == preferredVideoQuality) // only preferred video quality
                    .OrderBy(s => s.Size) // order by size
                    .First(); // take lowest size

                return new[] {muxedStreamInfo};
            }

            // Otherwise - use adaptive streams
            {
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                    .OrderByDescending(s => s.Bitrate) // order by bitrate
                    .First(); // take highest bitrate

                var videoStreamInfo = mediaStreamInfoSet.Video
                    .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                    .Where(s => preferredVideoQuality == null || s.VideoQuality == preferredVideoQuality) // filter by preferred video quality if set
                    .Where(s => preferredFramerate == null || s.Framerate == preferredFramerate) // filter by preferred framerate if set
                    .OrderByDescending(s => s.VideoQuality) // order by video quality (in case it wasn't filtered by it)
                    .ThenByDescending(s => s.Framerate) // order by framerate (in case it wasn't filtered by it) 
                    .ThenBy(s => s.Size) // order by size
                    .First(); // take smallest size, highest video quality and framerate

                return new MediaStreamInfo[] {audioStreamInfo, videoStreamInfo};
            }
        }
    }

    public partial class MediaStreamInfoSelector
    {
        private static readonly string[] AudioOnlyFormats = {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};

        private static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }
}