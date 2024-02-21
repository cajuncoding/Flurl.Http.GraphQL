﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlurlGraphQL.SystemTextJsonExtensions
{
    public static class SystemTextJsonExtensions
    {
        public static string ToJsonStringIndented(this JsonNode json, JsonSerializerOptions options = null)
        {
            if (json == null) return string.Empty;

            var serializerOptions = options ?? new JsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            return json.ToJsonString(serializerOptions);
        }
    }

    /// <summary>
    /// A Set of Json Extensions that were approved for V7, but didn't make it into V6 -- and are documented to be there...
    /// Sourced from GitHub Issue & Post: https://github.com/dotnet/runtime/issues/55827#issuecomment-889443545
    /// </summary>
    public static class JsonNodeExtensionsForV6FromMS
    {
        public static IEnumerable<T> GetValues<T>(this JsonArray jArray)
        {
            return jArray.Select(v => v.GetValue<T>());
        }

        public static string GetAccessorFromPath(this JsonNode node)
        {
            string path = node.GetPath();

            int propertyIndex = path.LastIndexOf('.');
            int arrayIndex = path.LastIndexOf('[');

            if (propertyIndex > arrayIndex)
            {
                return path.Substring(propertyIndex + 1);
            }

            if (arrayIndex > 0)
            {
                return path.Substring(arrayIndex);
            }

            return "$";
        }

        public static JsonValueKind GetValueKind(this JsonNode node, JsonSerializerOptions options = null)
        {
            JsonValueKind valueKind;

            if (node is null)
            {
                valueKind = JsonValueKind.Null;
            }
            else if (node is JsonObject)
            {
                valueKind = JsonValueKind.Object;
            }
            else if (node is JsonArray)
            {
                valueKind = JsonValueKind.Array;
            }
            else
            {
                JsonValue jValue = (JsonValue)node;

                if (jValue.TryGetValue(out JsonElement element))
                {
                    // Typically this will occur in read mode after a Parse(), so just use the JsonElement.
                    valueKind = element.ValueKind;
                }
                else
                {
                    object obj = jValue.GetValue<object>();


                    if (obj is string)
                    {
                        valueKind = JsonValueKind.String;
                    }
                    else if (IsKnownNumberType(obj.GetType()))
                    {
                        valueKind = JsonValueKind.Number;
                    }
                    else
                    {
                        // Slow, but accurate.
                        string json = jValue.ToJsonString();
                        valueKind = JsonSerializer.Deserialize<JsonElement>(json, options).ValueKind;
                    }
                }
            }

            return valueKind;

        }

        //BBernard: Relocated to private static vs local static for netstandard2.0 compatibility
        private static bool IsKnownNumberType(Type type)
        {
            return type == typeof(sbyte) ||
                   type == typeof(byte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        public static JsonNode DeepClone(this JsonNode node, JsonSerializerOptions options = null)
        {
            if (node is null)
            {
                return null;
            }

            string json = node.ToJsonString(options);

            JsonNodeOptions nodeOptions = default;
            if (options != null)
            {
                nodeOptions = new JsonNodeOptions() { PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive };
            }


            return JsonNode.Parse(json, nodeOptions);
        }

        public static bool DeepEquals(this JsonNode node, JsonNode other, JsonSerializerOptions options = null)
        {
            string json = node.ToJsonString(options);
            string jsonOther = other.ToJsonString(options);

            return json == jsonOther;
        }
    }
}
