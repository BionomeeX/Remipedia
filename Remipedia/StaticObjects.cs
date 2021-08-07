using System;
using System.Net.Http;

namespace Remipedia
{
    public static class StaticObjects
    {
        public static HttpClient HttpClient { get; } = new();
        public static Random Random { get; } = new();

        static StaticObjects()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Remipedia");
        }
    }
}
