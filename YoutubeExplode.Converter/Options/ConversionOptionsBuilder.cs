using System.IO;
using YoutubeExplode.Converter.Internal.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter.Options
{
    /// <summary>
    /// Builder for <see cref="ConversionOptions"/>.
    /// </summary>
    public class ConversionOptionsBuilder
    {
        private readonly string _outputFilePath;
        private string? _format;
        private EncoderPreset _preset = EncoderPreset.Medium;
        private Framerate? _targetFramerate;
        private Bitrate? _targetBitrate;

        /// <summary>
        /// Initializes an instance of <see cref="ConversionOptionsBuilder"/>.
        /// </summary>
        public ConversionOptionsBuilder(string outputFilePath) =>
            _outputFilePath = outputFilePath;

        private string GetDefaultFormat() =>
            Path.GetExtension(_outputFilePath).TrimStart('.').NullIfWhiteSpace() ?? "mp4";

        /// <summary>
        /// Set output format.
        /// </summary>
        public ConversionOptionsBuilder SetFormat(string format)
        {
            _format = format;
            return this;
        }

        /// <summary>
        /// Set encoder preset.
        /// </summary>
        public ConversionOptionsBuilder SetPreset(EncoderPreset preset)
        {
            _preset = preset;
            return this;
        }

        /// <summary>
        /// Set target framerate.
        /// </summary>
        public ConversionOptionsBuilder SetTargetFramerate(Framerate framerate)
        {
            _targetFramerate = framerate;
            return this;
        }

        /// <summary>
        /// Set target bitrate.
        /// </summary>
        public ConversionOptionsBuilder SetTargetBitrate(Bitrate bitrate)
        {
            _targetBitrate = bitrate;
            return this;
        }

        /// <summary>
        /// Create an instance of <see cref="ConversionOptions"/> with the configured parameters.
        /// </summary>
        public ConversionOptions Build() => new ConversionOptions(
            _outputFilePath,
            _format ?? GetDefaultFormat(),
            _preset,
            _targetFramerate,
            _targetBitrate
        );
    }
}