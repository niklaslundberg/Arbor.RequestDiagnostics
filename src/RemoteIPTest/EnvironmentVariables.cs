using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

namespace RemoteIPTest
{
    public static class EnvironmentVariables
    {
        public static ImmutableDictionary<string, string> All =>
            Environment.GetEnvironmentVariables()
                .OfType<DictionaryEntry>()
                .ToImmutableDictionary(pair => (string) pair.Key, pair => (string) pair.Value);
    }
}
