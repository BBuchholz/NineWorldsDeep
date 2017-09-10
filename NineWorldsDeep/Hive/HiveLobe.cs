﻿using System;
using NineWorldsDeep.Mnemosyne.V5;
using System.Collections.Generic;

namespace NineWorldsDeep.Hive
{
    public abstract class HiveLobe
    {
        public string Name { private set; get; }
        public HiveRoot HiveRoot { private set; get; }
        protected List<HiveSpore> SporesInternal { private set; get; }
        public IEnumerable<HiveSpore> Spores { get { return SporesInternal; } }

        public HiveLobe(string name, HiveRoot hr)
        {
            this.Name = name;
            this.HiveRoot = hr;
            this.SporesInternal = new List<HiveSpore>();
        }
        
        /// <summary>
        /// Any HiveSpore can be passed in as a parameter, 
        /// only those that fit this particular lobe will be added
        /// 
        /// (spores drift through, lobes collect what they are designed to)
        /// </summary>
        /// <param name="spore"></param>
        public virtual void Add(HiveSpore spore)
        {
            //TODO: validate, check for duplicates, &c.
            SporesInternal.Add(spore);
        }

        public abstract void Collect();
    }
}