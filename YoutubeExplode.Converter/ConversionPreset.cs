using System;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Conversion preset.
    /// </summary>
    public readonly partial struct ConversionPreset
    {
        /// <summary>
        /// Preset name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ConversionPreset"/>.
        /// </summary>
        public ConversionPreset(string name) => Name = name;

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    public partial struct ConversionPreset
    {
        /// <summary>
        /// Implicit conversion to preset.
        /// </summary>
        public static implicit operator ConversionPreset(string name) => new ConversionPreset(name);

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        public static implicit operator string(ConversionPreset preset) => preset.Name;
    }

    public partial struct ConversionPreset : IEquatable<ConversionPreset>
    {
        /// <inheritdoc />
        public bool Equals(ConversionPreset other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ConversionPreset other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(ConversionPreset left, ConversionPreset right) => left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(ConversionPreset left, ConversionPreset right) => !left.Equals(right);
    }

    public partial struct ConversionPreset
    {
        /// <summary>
        /// Much faster conversion speed and bigger output file size.
        /// </summary>
        public static ConversionPreset VeryFast { get; } = new ConversionPreset("veryfast");

        /// <summary>
        /// Slightly faster conversion speed and bigger output file size.
        /// </summary>
        public static ConversionPreset Fast { get; } = new ConversionPreset("fast");

        /// <summary>
        /// Default preset.
        /// Balanced conversion speed and output file size.
        /// </summary>
        public static ConversionPreset Medium { get; } = new ConversionPreset("medium");

        /// <summary>
        /// Slightly slower conversion speed and smaller output file size.
        /// </summary>
        public static ConversionPreset Slow { get; } = new ConversionPreset("slow");

        /// <summary>
        /// Much slower conversion speed and smaller output file size.
        /// </summary>
        public static ConversionPreset VerySlow { get; } = new ConversionPreset("veryslow");
    }
}