using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRedisCache.Serialization;

namespace TestRedisCache.Caching
{
    public class RedisMultiKeyCache<TValue> : RedisMultiKeyBaseCache<TValue>
    {
        protected string cacheName { get; private set; }

        protected List<string> associatedKeys { get; private set; }

        public RedisMultiKeyCache(string Name, List<string> keys, TimeSpan xiCacheExpiryDurationOrZero, SingleDataRetriever contentRetriver, DataRetriever allContentRetriever)
          : base(xiCacheExpiryDurationOrZero, contentRetriver, allContentRetriever)
        {
            this.cacheName = Name;
            this.associatedKeys = keys;
        }

        public RedisMultiKeyCache(string Name, List<string> keys, TimeSpan xiCacheExpiryDurationOrZero)
          : base(xiCacheExpiryDurationOrZero)
        {
            this.cacheName = Name;
            this.associatedKeys = keys;
        }

        protected override void StoreInCache(RedisCacheItem cacheItem)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            var hashEntries = new HashEntry[associatedKeys.Count];
            string primaryKeyFormat = string.Empty;
            int i = 0;
            foreach (var key in cacheItem.Keys)
            {
                if (associatedKeys[0].ToLower().Equals(key.Key.ToLower()))
                {
                    primaryKeyFormat = GestringFormat(key.Key, key.Value.ToString());
                    hashEntries[i++] = new HashEntry(primaryKeyFormat, ConvertToRedisValue(cacheItem));
                }
                else
                {
                    hashEntries[i++] = new HashEntry(GestringFormat(key.Key, key.Value.ToString()), ConvertToRedisValue(primaryKeyFormat));
                }
            }
            //hashEntries[0] = new HashEntry(Convert.ToString(cacheItem.Key), ConvertToRedisValue(cacheItem));
            lDatabase.HashSet(this.cacheName, hashEntries);

        }
        protected string GestringFormat(string keyClass, object key)
        {
            return String.Format("{0} : {1}", keyClass, key.ToString());
        }

        protected override RedisCacheItem RetrieveFromCache(string KeyClass, object Key)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            var Item = lDatabase.HashGet(this.cacheName, GestringFormat(KeyClass, Key.ToString()));

            if (Item.HasValue && this.associatedKeys[0].ToLower() == KeyClass.ToLower())
            {
                var responseItem = Item.IsNull ? null : ConvertFromRedisValue(Item);
                return responseItem != null ? responseItem : null;
            }
            else if (Item.HasValue)
            {
                var lookupItem = Item.IsNull ? null : ConvertFromRedisValueToString(Item);
                if (!String.IsNullOrEmpty(lookupItem))
                {
                    var result = lDatabase.HashGet(this.cacheName, lookupItem);
                    return result.HasValue ? ConvertFromRedisValue(result) : null;
                }

            }

            return default(RedisCacheItem);

        }

        protected override void RemoveFromCache(string KeyClass, object Key)
        {
            //TODO: change as per lookup
            var lDatabase = base.RedisConnection.GetDatabase();
            lDatabase.HashDelete(this.cacheName, Key.ToString());
        }

        public override bool ContainsKey(string Key)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            return lDatabase.HashExists(this.cacheName, Key.ToString());
        }

        public override void Clear()
        {

            var endpoints = ConnectionMultiplexer.Connect("localhost,allowAdmin=true").GetEndPoints(true);
            foreach (var endpoint in endpoints)
            {
                var server = ConnectionMultiplexer.Connect("localhost,allowAdmin=true").GetServer(endpoint);
                server.FlushDatabase();
            }

        }

        protected RedisValue ConvertToRedisValue(RedisCacheItem xiResults)
        {
            using (var lMemoryStream = new MemoryStream())
            {
                using (var lCompressionStream = new DeflateStream(lMemoryStream, CompressionLevel.Fastest))
                {
                    SerializationUtils.SerializeToStream(lCompressionStream, xiResults);
                }

                return lMemoryStream.ToArray();
            }
        }
        protected RedisValue ConvertToRedisValue(string xiResults)
        {
            using (var lMemoryStream = new MemoryStream())
            {
                using (var lCompressionStream = new DeflateStream(lMemoryStream, CompressionLevel.Fastest))
                {
                    SerializationUtils.SerializeToStream(lCompressionStream, xiResults);
                }

                return lMemoryStream.ToArray();
            }
        }
        protected RedisCacheItem ConvertFromRedisValue(RedisValue xiCachedValue)
        {
            try
            {
                using (var lMemoryStream = new MemoryStream(xiCachedValue))
                {
                    using (var lCompressionStream = new DeflateStream(lMemoryStream, CompressionMode.Decompress))
                    {
                        return SerializationUtils.DeserializeFromStream<RedisCacheItem>(lCompressionStream);
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        protected string ConvertFromRedisValueToString(RedisValue xiCachedValue)
        {
            try
            {
                using (var lMemoryStream = new MemoryStream(xiCachedValue))
                {
                    using (var lCompressionStream = new DeflateStream(lMemoryStream, CompressionMode.Decompress))
                    {
                        return SerializationUtils.DeserializeFromStream<string>(lCompressionStream);
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
    public class Keys
    {
        public string keyClass { get; set; }
        public object keyValue { get; set; }
    }
}
