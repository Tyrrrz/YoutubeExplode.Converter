// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0 || NET461
namespace System.IO
{
    using Threading;
    using Threading.Tasks;

    internal static class Extensions
    {
        public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
            await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
    }
}
#endif