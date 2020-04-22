using System;
using System.Collections.Generic;

namespace YoutubeExplode.Converter.Tests.Internal
{
    internal class ProgressCollector<T> : IProgress<T>
    {
        private readonly List<T> _list = new List<T>();

        public void Report(T value) => _list.Add(value);

        public IReadOnlyList<T> GetAll() => _list.ToArray();
    }
}