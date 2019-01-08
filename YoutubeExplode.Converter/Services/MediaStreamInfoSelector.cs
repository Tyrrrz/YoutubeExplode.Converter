using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Converter.Services
{
    /// <summary>
    /// Default media stream info selector.
    /// </summary>
    public partial class MediaStreamInfoSelector : IMediaStreamInfoSelector
    {
        /// <inheritdoc />
        public IReadOnlyList<MediaStreamInfo> Select(MediaStreamInfoSet mediaStreamInfoSet,
            VideoQuality videoQuality, string format)
        {
            // If audio-only - get the highest quality audio
            if (IsAudioOnlyFormat(format))
            {
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .Where(s => s.Container == Container.Mp4)
                    .WithHighestBitrate();

                return new[] {audioStreamInfo};
            }

            // If needs video but requested video quality is below 720p - use muxed stream
            if (videoQuality < VideoQuality.High720)
            {
                var muxedStreamInfo = mediaStreamInfoSet.Muxed
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality == videoQuality);

                return new[] {muxedStreamInfo};
            }

            // Otherwise - use adaptive streams
            {
                var audioStreamInfo = mediaStreamInfoSet.Audio
                    .Where(s => s.Container == Container.Mp4)
                    .WithHighestBitrate();

                var videoStreamInfo = mediaStreamInfoSet.Video
                    .Where(s => s.Container == Container.Mp4)
                    .First(s => s.VideoQuality == videoQuality);

                return new MediaStreamInfo[] {audioStreamInfo, videoStreamInfo};
            }
        }
    }

    public partial class MediaStreamInfoSelector
    {
        private static readonly string[] AudioOnlyFormats = { "mp3", "m4a", "wav", "wma", "ogg", "aac", "opus" };

        private static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Default instance of <see cref="MediaStreamInfoSelector"/>.
        /// </summary>
        public static MediaStreamInfoSelector Instance { get; } = new MediaStreamInfoSelector();
    }
}