﻿#nullable enable

using System.Globalization;
using Newtonsoft.Json;

namespace GeometryGraph.Runtime.Serialization {
    internal static class JsonHelperExtensions {
        public static float? ReadAsFloat(this JsonReader reader) {
            // https://github.com/jilleJr/Newtonsoft.Json-for-Unity.Converters/issues/46

            string? str = reader.ReadAsString();

            if (string.IsNullOrEmpty(str))
                return null;

            return float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float valueParsed) ? valueParsed : null;
        }
    }
}