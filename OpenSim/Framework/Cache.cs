/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenSim.Framework
{
    public delegate bool ExpireDelegate(string index);

    // The delegate we will use for performing fetch from backing store
    //
    public delegate Object FetchDelegate(string index);
    public enum CacheFlags
    {
        CacheMissing = 1,
        AllowUpdate = 2
    }

    // Select classes to store data on different media
    //
    public enum CacheMedium
    {
        Memory = 0,
        File = 1
    }

    // Strategy
    //
    // Conservative = Minimize memory. Expire items quickly.
    // Balanced = Expire items with few hits quickly.
    // Aggressive = Keep cache full. Expire only when over 90% and adding
    //
    public enum CacheStrategy
    {
        Conservative = 0,
        Balanced = 1,
        Aggressive = 2
    }
    // The main cache class. This is the class you instantiate to create
    // a cache
    //
    public class Cache
    {
        public ExpireDelegate OnExpire;

        private TimeSpan m_DefaultTTL = new TimeSpan(0);

        private CacheFlags m_Flags = 0;

        /// <summary>
        /// Must only be accessed under lock.
        /// </summary>
        private List<CacheItemBase> m_Index = new List<CacheItemBase>();
        private ReaderWriterLock m_IndexRwLock = new ReaderWriterLock();

        /// <summary>
        /// Must only be accessed under m_Index lock.
        /// </summary>
        private Dictionary<string, CacheItemBase> m_Lookup =
            new Dictionary<string, CacheItemBase>();

        private CacheMedium m_Medium;
        private int m_Size = 1024;
        private CacheStrategy m_Strategy;
        // Convenience constructors
        //
        public Cache()
        {
            m_Strategy = CacheStrategy.Balanced;
            m_Medium = CacheMedium.Memory;
            m_Flags = 0;
        }

        public Cache(CacheMedium medium) :
            this(medium, CacheStrategy.Balanced)
        {
        }

        public Cache(CacheMedium medium, CacheFlags flags) :
            this(medium, CacheStrategy.Balanced, flags)
        {
        }

        public Cache(CacheMedium medium, CacheStrategy strategy) :
            this(medium, strategy, 0)
        {
        }

        public Cache(CacheStrategy strategy, CacheFlags flags) :
            this(CacheMedium.Memory, strategy, flags)
        {
        }

        public Cache(CacheFlags flags) :
            this(CacheMedium.Memory, CacheStrategy.Balanced, flags)
        {
        }

        public Cache(CacheMedium medium, CacheStrategy strategy,
                        CacheFlags flags)
        {
            m_Strategy = strategy;
            m_Medium = medium;
            m_Flags = flags;
        }

        // Count of the items currently in cache
        //
        public int Count
        {
            get { 
                m_IndexRwLock.AcquireReaderLock(-1); 
                try
                { 
                    return m_Index.Count; 
                }
                finally
                {
                    m_IndexRwLock.ReleaseReaderLock();
                }
            }
        }

        public TimeSpan DefaultTTL
        {
            get { return m_DefaultTTL; }
            set { m_DefaultTTL = value; }
        }

        // Maximum number of items this cache will hold
        //
        public int Size
        {
            get { return m_Size; }
            set { SetSize(value); }
        }

        public void Clear()
        {
            m_IndexRwLock.AcquireWriterLock(-1);
            try
            {
                m_Index.Clear();
                m_Lookup.Clear();
            }
            finally
            {
                m_IndexRwLock.ReleaseWriterLock();
            }
        }

        // Find an object in cache by delegate.
        //
        public Object Find(Predicate<CacheItemBase> d)
        {
            CacheItemBase item;

            m_IndexRwLock.AcquireReaderLock(-1);
            try
            {
                item = m_Index.Find(d);
            }
            finally
            {
                m_IndexRwLock.ReleaseWriterLock();
            }

            if (item == null)
                return null;

            return item.Retrieve();
        }

        // Get an item from cache. Do not try to fetch from source if not
        // present. Just return null
        //
        public virtual Object Get(string index)
        {
            CacheItemBase item = GetItem(index);

            if (item == null)
                return null;

            return item.Retrieve();
        }

        // Fetch an object from backing store if not cached, serve from
        // cache if it is.
        //
        public virtual Object Get(string index, FetchDelegate fetch)
        {
            Object item = Get(index);
            if (item != null)
                return item;

            Object data = fetch(index);
            if (data == null)
            {
                if ((m_Flags & CacheFlags.CacheMissing) != 0)
                {
                    m_IndexRwLock.AcquireWriterLock(-1);
                    try
                    {
                        CacheItemBase missing = new CacheItemBase(index);
                        if (!m_Index.Contains(missing))
                        {
                            m_Index.Add(missing);
                            m_Lookup[index] = missing;
                        }
                    }
                    finally
                    {
                        m_IndexRwLock.ReleaseWriterLock();
                    }
                }
                return null;
            }

            Store(index, data);

            return data;
        }

        public void Invalidate(string uuid)
        {
            m_IndexRwLock.AcquireWriterLock(-1);
            try
            {
                if (!m_Lookup.ContainsKey(uuid))
                    return;

                CacheItemBase item = m_Lookup[uuid];
                m_Lookup.Remove(uuid);
                m_Index.Remove(item);
            }
            finally
            {
                m_IndexRwLock.ReleaseWriterLock();
            }
        }

        public virtual void Store(string index, Object data)
        {
            Type container;

            switch (m_Medium)
            {
                case CacheMedium.Memory:
                    container = typeof(MemoryCacheItem);
                    break;

                case CacheMedium.File:
                    return;

                default:
                    return;
            }

            Store(index, data, container);
        }

        public virtual void Store(string index, Object data, Type container)
        {
            Store(index, data, container, new Object[] { index });
        }

        public virtual void Store(string index, Object data, Type container,
                        Object[] parameters)
        {
            CacheItemBase item;

            m_IndexRwLock.AcquireWriterLock(-1);
            try
            {
                Expire(false);

                if (m_Index.Contains(new CacheItemBase(index)))
                {
                    if ((m_Flags & CacheFlags.AllowUpdate) != 0)
                    {
                        item = GetItem(index);

                        item.hits++;
                        item.lastUsed = DateTime.Now;
                        if (m_DefaultTTL.Ticks != 0)
                            item.expires = DateTime.Now + m_DefaultTTL;

                        item.Store(data);
                    }
                    return;
                }

                item = (CacheItemBase)Activator.CreateInstance(container,
                        parameters);

                if (m_DefaultTTL.Ticks != 0)
                    item.expires = DateTime.Now + m_DefaultTTL;

                m_Index.Add(item);
                m_Lookup[index] = item;
            }
            finally
            {
                m_IndexRwLock.ReleaseWriterLock();
            }

            item.Store(data);
        }

        /// <summary>
        /// Expire items as appropriate.
        /// </summary>
        /// <remarks>
        /// Callers must lock m_Index.
        /// </remarks>
        /// <param name='getting'></param>
        protected virtual void Expire(bool getting)
        {
            if (getting && (m_Strategy == CacheStrategy.Aggressive))
                return;

            if (m_DefaultTTL.Ticks != 0)
            {
                DateTime now = DateTime.Now;

                foreach (CacheItemBase item in new List<CacheItemBase>(m_Index))
                {
                    if (item.expires.Ticks == 0 ||
                            item.expires <= now)
                    {
                        m_Index.Remove(item);
                        m_Lookup.Remove(item.uuid);
                    }
                }
            }

            switch (m_Strategy)
            {
                case CacheStrategy.Aggressive:
                    if (Count < Size)
                        return;

                    m_Index.Sort(new SortLRU());
                    m_Index.Reverse();

                    int target = (int)((float)Size * 0.9);
                    if (target == Count) // Cover ridiculous cache sizes
                        return;

                    ExpireDelegate doExpire = OnExpire;

                    if (doExpire != null)
                    {
                        List<CacheItemBase> candidates =
                                m_Index.GetRange(target, Count - target);

                        foreach (CacheItemBase i in candidates)
                        {
                            if (doExpire(i.uuid))
                            {
                                m_Index.Remove(i);
                                m_Lookup.Remove(i.uuid);
                            }
                        }
                    }
                    else
                    {
                        m_Index.RemoveRange(target, Count - target);

                        m_Lookup.Clear();

                        foreach (CacheItemBase item in m_Index)
                            m_Lookup[item.uuid] = item;
                    }

                    break;

                default:
                    break;
            }
        }

        // Get an item from cache. Return the raw item, not it's data
        //
        protected virtual CacheItemBase GetItem(string index)
        {
            CacheItemBase item = null;

            m_IndexRwLock.AcquireReaderLock(-1);
            try
            {
                if (m_Lookup.ContainsKey(index))
                    item = m_Lookup[index];

                if (item == null)
                {
                    LockCookie lc = m_IndexRwLock.UpgradeToWriterLock(-1);
                    try
                    {
                        Expire(true);
                    }
                    finally
                    {
                        m_IndexRwLock.DowngradeFromWriterLock(ref lc);
                    }
                    return null;
                }

                item.hits++;
                item.lastUsed = DateTime.Now;

                {
                    LockCookie lc = m_IndexRwLock.UpgradeToWriterLock(-1);
                    try
                    {
                        Expire(true);
                    }
                    finally
                    {
                        m_IndexRwLock.DowngradeFromWriterLock(ref lc);
                    }
                }
            }
            finally
            {
                m_IndexRwLock.ReleaseReaderLock();
            }

            return item;
        }

        private void SetSize(int newSize)
        {
            m_IndexRwLock.AcquireWriterLock(-1);
            try
            {
                if (Count <= Size)
                    return;

                m_Index.Sort(new SortLRU());
                m_Index.Reverse();

                m_Index.RemoveRange(newSize, Count - newSize);
                m_Size = newSize;

                m_Lookup.Clear();

                foreach (CacheItemBase item in m_Index)
                    m_Lookup[item.uuid] = item;
            }
            finally
            {
                m_IndexRwLock.ReleaseWriterLock();
            }
        }

        // Comparison interfaces
        //
        private class SortLRU : IComparer<CacheItemBase>
        {
            public int Compare(CacheItemBase a, CacheItemBase b)
            {
                if (a == null && b == null)
                    return 0;
                if (a == null)
                    return -1;
                if (b == null)
                    return 1;

                return (a.lastUsed.CompareTo(b.lastUsed));
            }
        }
    }

    // The base class of all cache objects. Implements comparison and sorting
    // by the string member.
    //
    // This is not abstract because we need to instantiate it briefly as a
    // method parameter
    //
    public class CacheItemBase : IEquatable<CacheItemBase>, IComparable<CacheItemBase>
    {
        public DateTime entered;
        public DateTime expires = new DateTime(0);
        public int hits = 0;
        public DateTime lastUsed;
        public string uuid;
        public CacheItemBase(string index)
        {
            uuid = index;
            entered = DateTime.Now;
            lastUsed = entered;
        }

        public CacheItemBase(string index, DateTime ttl)
        {
            uuid = index;
            entered = DateTime.Now;
            lastUsed = entered;
            expires = ttl;
        }

        public virtual int CompareTo(CacheItemBase item)
        {
            return uuid.CompareTo(item.uuid);
        }

        public virtual bool Equals(CacheItemBase item)
        {
            return uuid == item.uuid;
        }

        public virtual bool IsLocked()
        {
            return false;
        }

        public virtual Object Retrieve()
        {
            return null;
        }

        public virtual void Store(Object data)
        {
        }
    }

    // Simple persistent file storage
    //
    public class FileCacheItem : CacheItemBase
    {
        public FileCacheItem(string index) :
            base(index)
        {
        }

        public FileCacheItem(string index, DateTime ttl) :
            base(index, ttl)
        {
        }

        public FileCacheItem(string index, Object data) :
            base(index)
        {
            Store(data);
        }

        public FileCacheItem(string index, DateTime ttl, Object data) :
            base(index, ttl)
        {
            Store(data);
        }

        public override Object Retrieve()
        {
            //TODO: Add file access code
            return null;
        }

        public override void Store(Object data)
        {
            //TODO: Add file access code
        }
    }

    // Simple in-memory storage. Boxes the object and stores it in a variable
    //
    public class MemoryCacheItem : CacheItemBase
    {
        private Object m_Data;

        public MemoryCacheItem(string index) :
            base(index)
        {
        }

        public MemoryCacheItem(string index, DateTime ttl) :
            base(index, ttl)
        {
        }

        public MemoryCacheItem(string index, Object data) :
            base(index)
        {
            Store(data);
        }

        public MemoryCacheItem(string index, DateTime ttl, Object data) :
            base(index, ttl)
        {
            Store(data);
        }

        public override Object Retrieve()
        {
            return m_Data;
        }

        public override void Store(Object data)
        {
            m_Data = data;
        }
    }
}