using System;
using System.Runtime.Caching;
using System.Linq;

namespace Ness.Utils
{
    /// <summary>
    /// insert example:
    /// Cacher.Add(token, user, DateTimeOffset.UtcNow.AddHours(1));
    /// get example:
    /// var result = memCacher.Get(token);
    /// </summary>
    public class CacheHelper
    {
        public static object Get(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }

        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, value, absExpiration);
        }

        public static void Delete(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }

        /// <summary>
        /// returns values as string from object array
        /// string cacheKey = CacheHelper.GetKey(userLookupsKey, t.ToArray());
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetKey(string key, object[] values)
        {
            string vals = string.Empty;

            if (values != null)
            {
                vals = string.Join(".", values);
            }

            return string.Format("{0}.{1}", key, vals);
        }


        /// <summary>
        /// returns values as string from object
        ///  string cacheKey = CacheHelper.GetKey("Object", model);
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetKey(string key, object obj)
        {

            string vals = string.Empty;

            if (obj != null)
            {
                var arrVals = obj.GetType()
                 .GetProperties()
                 .Select(p =>
                 {
                     object value = p.GetValue(obj, null);
                     return value == null ? null : value.ToString();
                 })
                 .ToArray();

                vals = string.Join(".", arrVals);
            }

            return string.Format("{0}.{1}", key, vals);
        }

    }
}
