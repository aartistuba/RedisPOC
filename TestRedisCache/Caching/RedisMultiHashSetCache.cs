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
    public class RedisMultiHashSetCache<TValue> : RedisMultiKeyBaseCache<TValue>
    {
        protected List<HashSets> hashSets = new List<HashSets>();

        public RedisMultiHashSetCache(string prefix, List<string> keys, TimeSpan xiCacheExpiryDurationOrZero, SingleDataRetriever contentRetriver, DataRetriever allContentRetriever)
          : base(xiCacheExpiryDurationOrZero, contentRetriver, allContentRetriever)
        {
            foreach (var key in keys)
            {
                this.hashSets.Add(new HashSets
                {
                    key = key,
                    HashSetName = prefix + key + "hashSet",
                    isPrimaryHashSet = key.ToLower().Equals(keys[0].ToLower()) ? true : false
                });
            }

        }

        public RedisMultiHashSetCache(string prefix, List<string> keys, TimeSpan xiCacheExpiryDurationOrZero)
          : base(xiCacheExpiryDurationOrZero)
        {

            foreach (var key in keys)
            {
                this.hashSets.Add(new HashSets
                {
                    key = key,
                    HashSetName = prefix + key + "hashSet",
                    isPrimaryHashSet = key.ToLower().Equals(keys[0].ToLower()) ? true : false
                });
            }
        }

        protected override void StoreInCache(RedisCacheItem cacheItem)
        {
            var lDatabase = base.RedisConnection.GetDatabase();
            //FetchPrimaryKeyHashSetDetails
            var PrimaryhashSetDetails = hashSets.Where(h => h.isPrimaryHashSet == true).FirstOrDefault();

            PrimaryhashSetDetails.Value = cacheItem.Keys.ToList().Where(p => p.Key.ToString().ToLower().Equals(PrimaryhashSetDetails.key.ToLower())).FirstOrDefault().Value.ToString();

            foreach (var key in cacheItem.Keys)
            {
                var hashset = hashSets.Where(h => h.key.ToLower().Equals(key.Key.ToLower())).FirstOrDefault();
                if (hashSets != null)
                {
                    if (hashset.isPrimaryHashSet)
                    {
                        //Store Actual Object Against only in primary HashSet
                        lDatabase.HashSet(hashset.HashSetName, new HashEntry[] { new HashEntry(key.Value.ToString(), ConvertToRedisValue(cacheItem)) });
                    }
                    else
                    {
                        //Check if there is already lookup stored if yes no need to update lookup
                        if (!lDatabase.HashExists(hashset.HashSetName, PrimaryhashSetDetails.Value))
                        {  //Store Look up value of primary HashSet in other hashSets
                            lDatabase.HashSet(hashset.HashSetName, new HashEntry[] { new HashEntry(key.Value.ToString(), ConvertToRedisValue(PrimaryhashSetDetails.Value)) });
                        }
                    }
                }
            }
        }

        protected override RedisCacheItem RetrieveFromCache(string KeyClass, object Key)
        {
            var lDatabase = base.RedisConnection.GetDatabase();

            var PrimaryhashSetDetails = hashSets.Where(h => h.isPrimaryHashSet == true).FirstOrDefault();

            if (PrimaryhashSetDetails.key.ToLower().Equals(KeyClass.ToLower()))
            {
                var Item = lDatabase.HashGet(PrimaryhashSetDetails.HashSetName, Key.ToString());

                if (Item.HasValue )
                {
                    var responseItem = Item.IsNull ? null : ConvertFromRedisValue(Item);
                    return responseItem != null ? responseItem : null;
                }
            }
            else
            {
                var hashset = hashSets.Where(h => h.key.ToLower().Equals(KeyClass.ToLower())).FirstOrDefault();
                //here for e.g . HashSetName - PartnerObjectIdHashSet ,Key - 1547
                var Item = lDatabase.HashGet(hashset.HashSetName, Key.ToString());
                if (Item.HasValue)
                {
                    var lookupItem = Item.IsNull ? null : ConvertFromRedisValueToString(Item);
                    if (!String.IsNullOrEmpty(lookupItem))
                    {
                        var result = lDatabase.HashGet(PrimaryhashSetDetails.HashSetName, lookupItem);
                        return result.HasValue ? ConvertFromRedisValue(result) : null;
                    }

                }
            }


            return default(RedisCacheItem);

        }

        protected override void RemoveFromCache(string KeyClass, object Key)
        {
            //TODO: change as per lookup
            //var lDatabase = base.RedisConnection.GetDatabase();
            //lDatabase.HashDelete(this.cacheName, Key.ToString());
        }

        public override bool ContainsKey(string Key)
        {
            //var lDatabase = base.RedisConnection.GetDatabase();
            //return lDatabase.HashExists(this.cacheName, Key.ToString());
            return true;
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
    public class HashSets
    {
        public string key { get; set; }
        public string Value { get; set; }

        public string HashSetName { get; set; }
        public bool isPrimaryHashSet { get; set; }
    }
}
