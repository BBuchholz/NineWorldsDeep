﻿using System;
using System.Collections.Generic;
using NineWorldsDeep.Mnemosyne.V5;
using System.Linq;

namespace NineWorldsDeep.Core
{
    public class MultiMap<K, V>
    {
        private Dictionary<K, List<V>> _dict;
        
        public MultiMap(IEqualityComparer<K> iec)
        {
            _dict = new Dictionary<K, List<V>>(iec);
        }

        public MultiMap()
        {
            _dict = new Dictionary<K, List<V>>();
        }

        /// <summary>
        /// adds key value pair to the multimap, passing null as the value for a given key 
        /// will ensure key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(K key, V value)
        {
            List<V> lst;
            if (this._dict.TryGetValue(key, out lst))
            {
                if (!lst.Contains(value))
                {
                    lst.Add(value);
                }
            }
            else
            {
                lst = new List<V>();
                if (value != null)
                {
                    lst.Add(value);
                }
                this._dict[key] = lst;
            }
        }

        public IEnumerable<K> Keys
        {
            get { return this._dict.Keys; }
        }

        public void ClearAll()
        {
            this._dict.Clear();
        }

        public List<V> this[K key]
        {
            get
            {
                List<V> lst;
                if (!this._dict.TryGetValue(key, out lst))
                {
                    lst = new List<V>();
                    this._dict[key] = lst;
                }
                return lst;
            }
        }

        public void RemoveValue(K key, V value)
        {
            if (this._dict.ContainsKey(key))
            {
                this._dict[key].Remove(value);
            }
        }

        public void PurgeValue(V value)
        {
            foreach (K key in this.Keys)
            {
                if (this._dict[key].Contains(value))
                {
                    this._dict[key].Remove(value);
                }
            }
        }

        public void Clear(K key)
        {
            if (this._dict.ContainsKey(key))
            {
                this._dict[key].Clear();
            }
        }

        public void RemoveKey(K key)
        {
            if (this._dict.ContainsKey(key))
            {
                this._dict.Remove(key);
            }
        }

        public bool ContainsKey(K key)
        {
            return this._dict.ContainsKey(key);
        }

        public void AddAll(MultiMap<K, V> anotherMultiMap)
        {
            foreach(K key in anotherMultiMap.Keys)
            {
                foreach(V val in anotherMultiMap[key])
                {
                    Add(key, val);
                }
            }
        }

        public void PutCommaStringValues(K key, string commaSeperatedValues)
        {
            foreach(string untrimmed in commaSeperatedValues.Split(','))
            {
                string trimmed = untrimmed.Trim();

                object obj = (Object)trimmed;

                V cast;

                if(obj is V)
                {
                    cast = (V)obj;
                }
                else
                {                    
                    cast = default(V);
                }
                
                if(cast != null)
                {
                    Add(key, cast);
                }
            }
        }

        internal List<V> AllValues()
        {
            var allVals = _dict.Values
                               .SelectMany(x => x) 
                               .ToList();

            return allVals;
        }
    }
}
