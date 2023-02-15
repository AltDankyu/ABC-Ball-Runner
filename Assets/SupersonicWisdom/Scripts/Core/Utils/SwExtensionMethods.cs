using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SupersonicWisdomSDK
{
    internal static class HashSetExtensions
    {
        #region --- Private Methods ---

        internal static void Add<TValue>(this HashSet<TValue> self, TValue addition)
        {
            var ignoreThis = self.Add(addition);
        }

        internal static void ForEach<TValue>(this HashSet<TValue> self, Action<TValue> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }

        #endregion
    }

    internal static class SwExtensionMethods
    {
        #region --- Public Methods ---

        public static void DontDestroyOnLoad(this Object gameObject)
        {
            Object.DontDestroyOnLoad(gameObject);
        }

        public static void RenderLast(this Canvas canvas)
        {
            canvas.transform.SetAsLastSibling();
            canvas.sortingOrder = short.MaxValue;
        }

        #endregion


        #region --- Private Methods ---

        internal static Color ExtractColorFromHex(this string colorHex, Color defaultColor)
        {
            if (colorHex == null || colorHex.Equals(string.Empty)) return defaultColor;

            return ColorUtility.TryParseHtmlString(colorHex, out var color) ? color : defaultColor;
        }

        internal static TSource FirstOr<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
        {
            var result = defaultValue;

            try
            {
                result = source.First(predicate);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                // Ignore the exception and return the default value...
            }

            return result;
        }

        /// Returns the `defaultValue` in case: the key doesn't exists / it holds to a null value.
        internal static bool SwAddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (key == null) return false;

            if (self.ContainsKey(key))
            {
                self.Remove(key);
            }

            self.Add(key, value);

            return true;
        }

        /// <summary>
        ///     Merge one dictionary into another.
        ///     The last source keys will override the first source keys, in case of conflicts.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sources"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        internal static Dictionary<TKey, TValue> SwClone<TKey, TValue>(this Dictionary<TKey, TValue> self)
        {
            var copied = new Dictionary<TKey, TValue>();
            copied.SwMerge(self);

            return copied;
        }

        /// <summary>
        ///     Merge one dictionary into another.
        ///     The last source keys will override the first source keys, in case of conflicts.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sources"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        internal static Dictionary<TKey, TValue> SwCloneMerged<TKey, TValue>(this Dictionary<TKey, TValue> self, params Dictionary<TKey, TValue>[] sources)
        {
            return self.SwClone().SwMerge(sources);
        }

        internal static long SwGetLength(this Stream self)
        {
            long? totalBytes = null;

            try
            {
                totalBytes = self.Length;
            }
            catch (Exception e)
            {
                //SwEditorLogger.LogError(e);
            }

            if (totalBytes != null) return totalBytes.Value;

            try
            {
                var contentBytesRemaining = self.GetType().GetRuntimeFields().FirstOr(fieldInfo => fieldInfo.Name == "_contentBytesRemaining", null);
                totalBytes = long.Parse(contentBytesRemaining?.GetValue(self)?.ToString() ?? "0");
            }
            catch (Exception e)
            {
                //SwEditorLogger.LogError(e);
            }

            return totalBytes ?? 0;
        }

        internal static bool SwIsValidEmailAddress(this string self)
        {
            var emailAddressRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");

            return emailAddressRegex.IsMatch(self);
        }

        /// <summary>
        ///     Merge dictionaries extension
        ///     The last source keys overrides the first source keys
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sources"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        internal static Dictionary<TKey, TValue> SwMerge<TKey, TValue>(this Dictionary<TKey, TValue> self, params Dictionary<TKey, TValue>[] sources)
        {
            foreach (var source in sources)
            {
                if (source != null)
                {
                    foreach (var keyValuePair in source)
                    {
                        self[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
            }

            return self;
        }

        internal static string SwRemoveSpaces(this string self)
        {
            return self?.Replace(" ", "");
        }

        /// Returns the `defaultValue` in case: the key doesn't exists / it holds to a null value.
        internal static TValue SwSafelyGet<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, [CanBeNull] TValue defaultValue)
        {
            if (key == null) return defaultValue;
            if (!self.ContainsKey(key)) return defaultValue;
            self.TryGetValue(key, out var value);

            return value == null ? defaultValue : value;
        }

        internal static TValue SwSafelyGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> self, TKey key, [CanBeNull] TValue defaultValue)
        {
            if (!self.ContainsKey(key)) return defaultValue;
            self.TryGetValue(key, out var value);

            return value == null ? defaultValue : value;
        }

        internal static TSource SwSafelyGet<TSource>(this IEnumerable<TSource> source, int index, TSource defaultValue)
        {
            var arr = source as TSource[] ?? source.ToArray();

            if (arr.Length <= index) return defaultValue;

            return arr[index];
        }

        internal static long SwTimestampMilliseconds(this DateTime self)
        {
            return self.Ticks / TimeSpan.TicksPerMillisecond;
        }

        internal static long SwTimestampSeconds(this DateTime self)
        {
            return self.Ticks / TimeSpan.TicksPerSecond;
        }

        internal static Dictionary<string, object> SwToJsonDictionary(this string self)
        {
            return SwJsonParser.DeserializeToDictionary(self);
        }

        internal static string SwToJsonString(this Dictionary<string, object> self)
        {
            return SwJsonParser.Serialize(self);
        }

        /// <summary>
        ///     <para>Use this method instead of `dateTime.ToString()` to avoid exceptions that occur in Unity 2020 and above.</para>
        /// </summary>
        internal static string SwToString(this DateTime self)
        {
            // Formatting a date with CultureInfo.InvariantCulture as second parameter prevents exceptions in case the users' default culture is Arabic or Thai
            return self.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     <para>Use this method instead of `dateTime.ToString(...)` to avoid exceptions that occur in Unity 2020 and above.</para>
        /// </summary>
        internal static string SwToString(this DateTime self, string format)
        {
            // Formatting a date with CultureInfo.InvariantCulture as second parameter prevents exceptions in case the users' default culture is Arabic or Thai
            return self.ToString(format, CultureInfo.InvariantCulture);
        }

        #endregion
    }
}