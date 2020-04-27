using StackExchange.Redis;
using TestRedisCache.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TestRedisCache.Serialization;

namespace TestRedisCache.Caching
{
    public class RedisMemoryCache<TKey, TValue> : RedisBaseCache<TKey, TValue>
    {
        protected string cacheName { get; private set; }

        public RedisMemoryCache(string Name, TimeSpan xiCacheExpiryDurationOrZero, DataRetriever xiContentRetriever)
          : base(xiCacheExpiryDurationOrZero, xiContentRetriever)
        {
            this.cacheName = Name;
        }

        public RedisMemoryCache(string Name, TimeSpan xiCacheExpiryDurationOrZero)
          : base(xiCacheExpiryDurationOrZero)
        {
            this.cacheName = Name;
        }

        protected override void StoreInCache(RedisCacheItem cacheItem)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            var hashEntries = new HashEntry[1];

            if (!lDatabase.HashExists(this.cacheName, Convert.ToString(cacheItem.Key)))
            {
                hashEntries[0] = new HashEntry(Convert.ToString(cacheItem.Key), ConvertToRedisValue(cacheItem));
                lDatabase.HashSet(this.cacheName, hashEntries);
            }
        }

        protected override RedisCacheItem RetrieveFromCache(TKey Key)
        {
           
            var lDatabase = base.RedisConnection.GetDatabase();
            var Item = lDatabase.HashGet(this.cacheName, Key.ToString());
         
            var responseItem = Item.IsNull ? null : ConvertFromRedisValue(Item);
            return responseItem != null ? responseItem : null;
        }

        protected override void RemoveFromCache(TKey Key)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            lDatabase.HashDelete(this.cacheName, Key.ToString());
        }

        public override bool ContainsKey(TKey Key)
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


        //public virtual ICollection<TValue> RetrieveAll()
        //{
        //    lock (mDictionary)
        //    {
        //        return mDictionary.Values.Select(cacheItem => cacheItem.Value).ToList();
        //    }
        //}


        //public override int Count
        //{
        //    get { return mDictionary.Count; }
        //}

        //public override int ExpiredCount
        //{
        //    get { return GetExpiredCount(mDictionary); }
        //}

        //public override int SizeInBytes
        //{
        //    get { return GetSizeInBytes(mDictionary); }
        //}
    }
}
