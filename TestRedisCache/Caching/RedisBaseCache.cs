using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using TestRedisCache.Serialization;

namespace TestRedisCache.Caching
{
    public abstract class RedisBaseCache<TKey, TValue>
    {
        #region Declarations
        protected ConnectionMultiplexer RedisConnection = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");

        public delegate TValue DataRetriever(TKey Value);
        // public delegate TValue DataRetrieverWithTimeout(TKey Value, ref TimeSpan xbTimeout);

        protected DataRetriever mRetriever;
        //protected DataRetrieverWithTimeout mRetrieverWithTimeout;
        protected TimeSpan mCacheExpiry;
        private object mCacheInfoSyncRoot = new object();

        private int mEstimatedSize = -1;
        #endregion

        #region Constructors
        protected RedisBaseCache(TimeSpan CacheExpiryDurationOrZero)
        {
            this.RedisConnection.PreserveAsyncOrder = true;
            mCacheExpiry = CacheExpiryDurationOrZero > new TimeSpan(0) ? CacheExpiryDurationOrZero : TimeSpan.MaxValue;
        }

        protected RedisBaseCache(TimeSpan CacheExpiryDurationOrZero, DataRetriever ContentRetriever)
        : this(CacheExpiryDurationOrZero)
        {
            this.RedisConnection.PreserveAsyncOrder = true;

            mRetriever = ContentRetriever;
            if (mRetriever != null)
            {
                Name = mRetriever.Method.DeclaringType + "." + mRetriever.Method.Name;
            }
        }
        #endregion
        public string Name { get; private set; }
        #region Abstract Methods
        protected abstract void StoreInCache(RedisCacheItem Item);

        protected abstract RedisCacheItem RetrieveFromCache(TKey Key);

        protected abstract void RemoveFromCache(TKey Key);

        public abstract bool ContainsKey(TKey Key);

        public abstract void Clear();
        #endregion

        ///========================================================================
        /// Method : Retrieve
        /// 
        /// <summary>
        ///   Retrieves an item from the cache, if it's not cached or has expired
        ///   then the delegate is used to read the value
        /// </summary>
        ///========================================================================
        public virtual TValue Retrieve(TKey Key)
        {
            return Retrieve(Key, false);
        }

        ///========================================================================
        /// Method : Retrieve
        /// 
        /// <summary>
        ///   Retrieves an item from the cache, if it's not cached or has expired
        ///   then the delegate is used to read the value. Optionally allows
        ///   caller to force the item to be refreshed
        /// </summary>
        ///========================================================================
        public virtual TValue Retrieve(TKey Key, bool ForceRefresh)
        {

            //========================================================================
            // If we do have a cached item then note as much here - we may later need
            // to remove it from the cache if it is expired and we can't retrieve an
            // up to date version
            //========================================================================
            RedisCacheItem lItem = RetrieveFromCache(Key);
            bool lMayRemoveItem = (lItem != null);

            if (lItem == null || lItem.TimeOut < DateTime.UtcNow || ForceRefresh)
            {
                lItem = null;

                if (mRetriever != null)
                {
                    if (lMayRemoveItem)
                        RemoveFromCache(Key);

                    DateTime lStart = DateTime.UtcNow;
                    TimeSpan lTimeOut = GetTimeout() - lStart;
                    TValue lValue = mRetriever != null ? mRetriever(Key) : default(TValue);
                    TimeSpan lRetrievalTime = DateTime.UtcNow - lStart;

                    lItem = new RedisCacheItem(Key, lValue, lStart.Add(lTimeOut))
                    {
                        FetchTimeInMillisecs = (int)lRetrievalTime.TotalMilliseconds
                    };
                    StoreInCache(lItem);
                }
                else if (lMayRemoveItem)
                {
                    //========================================================================
                    // The item is expired. Should remove it now since it's no use any more
                    //========================================================================
                    RemoveFromCache(Key);
                }
            }

            return (lItem == null) ? default(TValue) : lItem.Value;
        }

        ///========================================================================
        /// Method : GetTimeout
        /// 
        /// <summary>
        ///   Retrieves the timeout for an item that is about to be stored in
        ///   the cache
        /// </summary>
        ///========================================================================
        protected DateTime GetTimeout()
        {
            return mCacheExpiry == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + mCacheExpiry;
        }

        ///========================================================================
        /// Method : Store
        /// 
        /// <summary>
        ///   Allows callers to explictly store data in the cache if they desire
        /// </summary>
        ///========================================================================
        public void Store(TKey Key, TValue Value)
        {
            DateTime lTimeOut = mCacheExpiry == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + mCacheExpiry;
            var lItem = new RedisCacheItem(Key, Value, lTimeOut);
            StoreInCache(lItem);
        }

        ///========================================================================
        /// Method : Remove
        /// 
        /// <summary>
        ///   Removes an item from the cache
        /// </summary>
        ///========================================================================
        public void Remove(TKey Key)
        {
            RemoveFromCache(Key);
        }

        public TValue this[TKey Key]
        {
            get
            {
                return Retrieve(Key);
            }
            set
            {
                Store(Key, value);
            }
        }

        #region RedisItemClass
        [Serializable]
        public class RedisCacheItem
        {
            public RedisCacheItem(TKey Key, TValue Value, DateTime TimeOut)
            {
                this.Key = Key;
                this.Value = Value;
                this.TimeOut = TimeOut;
                this.AccessCount = 0;
                this.FetchTimeInMillisecs = int.MinValue;
            }

            public TKey Key;
            public TValue Value;
            public DateTime TimeOut;
            public int FetchTimeInMillisecs;
            public int AccessCount;

        }
        #endregion
    }

    public abstract class RedisMultiKeyBaseCache<TValue>
    {
        #region Declarations
        protected ConnectionMultiplexer RedisConnection = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");

        public delegate RedisCacheItem SingleDataRetriever(string KeyClass, object Key);
        public delegate RedisCacheItem[] DataRetriever();

        // public delegate TValue DataRetrieverWithTimeout(TKey Value, ref TimeSpan xbTimeout);

        protected SingleDataRetriever mSingleRetriever;
        protected DataRetriever mRetriever;

        //protected DataRetrieverWithTimeout mRetrieverWithTimeout;
        protected TimeSpan mCacheExpiry;
        private object mCacheInfoSyncRoot = new object();

        private int mEstimatedSize = -1;
        #endregion

        #region Constructors
        protected RedisMultiKeyBaseCache(TimeSpan CacheExpiryDurationOrZero)
        {
            //this.RedisConnection.PreserveAsyncOrder = true;
            mCacheExpiry = CacheExpiryDurationOrZero > new TimeSpan(0) ? CacheExpiryDurationOrZero : TimeSpan.MaxValue;
        }

        protected RedisMultiKeyBaseCache(TimeSpan CacheExpiryDurationOrZero, SingleDataRetriever ContentRetriever, DataRetriever allContentRetriever)
        : this(CacheExpiryDurationOrZero)
        {
            mRetriever = allContentRetriever;
            mSingleRetriever = ContentRetriever;
            if (mSingleRetriever != null)
            {
                Name = mSingleRetriever.Method.DeclaringType + "." + mSingleRetriever.Method.Name;
            }
        }
        #endregion
        public string Name { get; private set; }

        #region Abstract Methods
        protected abstract void StoreInCache(RedisCacheItem Item);

        protected abstract RedisCacheItem RetrieveFromCache(string KeyClass, object Key);

        protected abstract void RemoveFromCache(string KeyClass, object Key);

        public abstract bool ContainsKey(string Key);

        public abstract void Clear();
        #endregion

        ///========================================================================
        /// Method : Retrieve
        /// 
        /// <summary>
        ///   Retrieves an item from the cache, if it's not cached or has expired
        ///   then the delegate is used to read the value
        /// </summary>
        ///========================================================================
        public virtual TValue Retrieve(string KeyClass, object Key)
        {
            return Retrieve(KeyClass, Key, false);
        }

        ///========================================================================
        /// Method : Retrieve
        /// 
        /// <summary>
        ///   Retrieves an item from the cache, if it's not cached or has expired
        ///   then the delegate is used to read the value. Optionally allows
        ///   caller to force the item to be refreshed
        /// </summary>
        ///========================================================================
        public virtual TValue Retrieve(string KeyClass, object Key, bool ForceRefresh)
        {

            //========================================================================
            // If we do have a cached item then note as much here - we may later need
            // to remove it from the cache if it is expired and we can't retrieve an
            // up to date version
            //========================================================================
            RedisCacheItem lItem = RetrieveFromCache(KeyClass, Key);
            bool lMayRemoveItem = (lItem != null);

            if (lItem == null || lItem.TimeOut < DateTime.UtcNow || ForceRefresh)
            {
                lItem = null;

                if (mRetriever != null)
                {
                    if (lMayRemoveItem)
                        RemoveFromCache(KeyClass, Key);

                    DateTime lStart = DateTime.UtcNow;
                    TimeSpan lTimeOut = GetTimeout() - lStart;
                    RedisCacheItem lValue = mSingleRetriever != null ? mSingleRetriever(KeyClass, Key) : default(RedisCacheItem);
                    TimeSpan lRetrievalTime = DateTime.UtcNow - lStart;

                    lItem = new RedisCacheItem(lValue.Keys, lValue.Value, lStart.Add(lTimeOut))
                    {
                        FetchTimeInMillisecs = (int)lRetrievalTime.TotalMilliseconds
                    };
                    StoreInCache(lItem);
                }
                else if (lMayRemoveItem)
                {
                    //========================================================================
                    // The item is expired. Should remove it now since it's no use any more
                    //========================================================================
                    RemoveFromCache(KeyClass, Key);
                }
            }

            return (lItem == null) ? default(TValue) : lItem.Value;
        }

        ///========================================================================
        /// Method : GetTimeout
        /// 
        /// <summary>
        ///   Retrieves the timeout for an item that is about to be stored in
        ///   the cache
        /// </summary>
        ///========================================================================
        protected DateTime GetTimeout()
        {
            return mCacheExpiry == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + mCacheExpiry;
        }

        ///========================================================================
        /// Method : Store
        /// 
        /// <summary>
        ///   Allows callers to explictly store data in the cache if they desire
        /// </summary>
        ///========================================================================
        //public void Store(string Key, TValue Value)
        //{
        //    DateTime lTimeOut = mCacheExpiry == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + mCacheExpiry;
        //    var lItem = new RedisCacheItem(Key, Value, lTimeOut);
        //    StoreInCache(lItem);
        //}

        ///========================================================================
        /// Method : Remove
        /// 
        /// <summary>
        ///   Removes an item from the cache
        /// </summary>
        ///========================================================================
        public void Remove(string KeyClass,object key)
        {
            RemoveFromCache(KeyClass, key);
        }

        public TValue this[string KeyClass, object Key]
        {
            get
            {
                return Retrieve(KeyClass, Key);
            }
            //set
            //{
            //    Store(Key, value);
            //}
        }

        #region RedisItemClass
        [Serializable]
        public class RedisCacheItem
        {
            public RedisCacheItem(ICollection<KeyValuePair<string, object>> Keys, TValue Value, DateTime TimeOut)
            {
                this.Keys = Keys;
                this.Value = Value;
                this.TimeOut = TimeOut;
                this.AccessCount = 0;
                this.FetchTimeInMillisecs = int.MinValue;
            }

            public ICollection<KeyValuePair<string, object>> Keys { get; set; }

            public TValue Value;
            public DateTime TimeOut;
            public int FetchTimeInMillisecs;
            public int AccessCount;



        }
        #endregion
    }



}
