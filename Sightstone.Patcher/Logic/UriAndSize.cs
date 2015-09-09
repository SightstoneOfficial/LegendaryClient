using System;

namespace Sightstone.Patcher.Logic
{
    internal class UriAndSize
    {
        public Uri uri;
        public long size;

        public UriAndSize(Uri uri, long size)
        {
            this.uri = uri;
            this.size = size;
        }
    }
}