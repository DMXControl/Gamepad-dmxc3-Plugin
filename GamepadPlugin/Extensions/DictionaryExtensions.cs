using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.Extensions
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
