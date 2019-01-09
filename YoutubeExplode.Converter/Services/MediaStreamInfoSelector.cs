using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Converter.Exceptions;
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

            var result = new List<MediaStreamInfo>();

            // Get audio stream
            var audioStreamInfo = mediaStreamInfoSet.Audio
                .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                .OrderByDescending(s => s.Bitrate) // order by bitrate
                .FirstOrDefault(); // take highest bitrate

            // If not found - throw
            if (audioStreamInfo == null)
                throw new MediaStreamInfoNotFoundException("Couldn't find any audio-only streams.");

            // Add to result
            result.Add(audioStreamInfo);

            // If needs video - get video stream
            if (!IsAudioOnlyFormat(format))
            {
                var videoStreamInfo = mediaStreamInfoSet.Video
                    .Where(s => s.Container == Container.Mp4) // only mp4 to make encoding easier
                    .Where(s => preferredVideoQuality == null || s.VideoQuality == preferredVideoQuality) // filter by preferred video quality if set
                    .Where(s => preferredFramerate == null || s.Framerate == preferredFramerate) // filter by preferred framerate if set
                    .OrderByDescending(s => s.VideoQuality) // order by video quality (in case it wasn't filtered by it)
                    .ThenByDescending(s => s.Framerate) // order by framerate (in case it wasn't filtered by it) 
                    .ThenBy(s => s.Size) // order by size
                    .FirstOrDefault(); // take smallest size, highest video quality and framerate

                // If not found - throw
                if (videoStreamInfo == null)
                    throw new MediaStreamInfoNotFoundException("Couldn't find any video-only streams matching preferences. " +
                                                               $"Preferred video quality: {preferredVideoQuality}. " +
                                                               $"Preferred framerate: {preferredFramerate}.");

                // Add to result
                result.Add(videoStreamInfo);
            }

            return result;
        }
    }

    public partial class MediaStreamInfoSelector
    {
        private static readonly string[] AudioOnlyFormats = {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};

        private static bool IsAudioOnlyFormat(string format) =>
            AudioOnlyFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }
}