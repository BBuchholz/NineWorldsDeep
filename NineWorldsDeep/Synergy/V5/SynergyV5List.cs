﻿using NineWorldsDeep.Core;
using NineWorldsDeep.Db.Sqlite;
using NineWorldsDeep.Tapestry.NodeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineWorldsDeep.Synergy.V5
{
    public class SynergyV5List
    {
        public static string LIST_STATUS_ACTIVE = "Active";
        public static string LIST_STATUS_SHELVED = "Shelved";

        public string ListName { get; private set; }
        public int ListId { get; set; }
        public DateTime? ActivatedAt { get; private set; }
        public DateTime? ShelvedAt { get; private set; }
        public List<SynergyV5ListItem> ListItems { get; private set; }

        public string Status
        {
            get
            {
                if(ShelvedAt == null)
                {
                    return LIST_STATUS_ACTIVE;
                }

                //shelved is not null
                if(ActivatedAt == null)
                {
                    return LIST_STATUS_SHELVED;
                }

                //neither is null
                if(ActivatedAt >= ShelvedAt)
                {
                    return LIST_STATUS_ACTIVE;
                }
                else
                {
                    return LIST_STATUS_SHELVED;
                }
            }
        }


        public SynergyV5List(string listName)
        {
            ListName = listName;
            ListId = -1;
            ListItems = new List<SynergyV5ListItem>();
        }

        //async version of Sync()
        public void SyncAsync(SynergyV5SubsetDb db, IAsyncStatusResponsive ui)
        {
            //await Task.Run(() =>
            //{
                //from gauntlet
                if (TimeStamp.IsTimeStampedList_YYYYMMDD(this))
                {
                    ui.StatusDetailUpdate("processing timestamped list: " + ListName);

                    //load if not
                    db.SyncAsync(this, ui);

                    Shelve();

                    db.SyncAsync(this, ui);

                    ListId = -1;
                    ListName = TimeStamp.StripTimeStamp_YYYYMMDD(ListName);

                    foreach (SynergyV5ListItem sli in ListItems)
                    {

                        sli.ClearIds();
                    }

                    Activate();
                }

                ui.StatusDetailUpdate("syncing list: " + ListName);

                db.SyncAsync(this, ui);

                ui.StatusDetailUpdate("finished syncing list: " + ListName);
            //});            
        }

        public void Sync(SynergyV5SubsetDb db)
        {
            //from gauntlet
            if (TimeStamp.IsTimeStampedList_YYYYMMDD(this))
            {

                //load if not
                db.Sync(this);

                Shelve();

                db.Sync(this);

                ListId = -1;
                ListName = TimeStamp.StripTimeStamp_YYYYMMDD(ListName);

                foreach (SynergyV5ListItem sli in ListItems)
                {

                    sli.ClearIds();
                }

                Activate();
            }

            db.Sync(this);
        }

        public void Add(int index, SynergyV5ListItem sli)
        {
            if (IsNotDuplicate(sli))
            {
                if(index > ListItems.Count)
                {
                    index = ListItems.Count;
                }

                ListItems.Insert(index, sli);
            }
        }

        private bool IsNotDuplicate(SynergyV5ListItem sli)
        {
            bool exists = false;

            foreach(SynergyV5ListItem existingListItem in ListItems)
            {
                string existingValue = existingListItem.ItemValue;

                if(existingValue.Equals(sli.ItemValue, StringComparison.CurrentCultureIgnoreCase))
                {
                    exists = true;
                }
            }

            return !exists;
        }

        /// <summary>
        /// will resolve conflicts, newest date will always take precedence
        /// passing null values allowed as well to just set one or the other
        /// null values always resolve to the non-null value(unless both null)
        /// 
        /// NOTE: THIS CONVERTS TO UTC, WHICH IS DEPENDENT ON DateTime.Kind
        /// This property defaults to local, so if you are passing a UTC time, make
        /// sure that the Kind is set to UTC or the conversion will
        /// be off. 
        /// 
        /// </summary>
        /// <param name="newActivatedAt"></param>
        /// <param name="newShelvedAt"></param>
        public void SetTimeStamps(DateTime? newActivatedAt, DateTime? newShelvedAt)
        {
            if(newActivatedAt != null)
            {   
                if(newActivatedAt.Value.Kind != DateTimeKind.Utc)
                {
                    newActivatedAt = newActivatedAt.Value.ToUniversalTime();
                }         

                if(ActivatedAt == null || 
                   DateTime.Compare(ActivatedAt.Value, newActivatedAt.Value) < 0)
                {
                    //ActivatedAt is older or null
                    ActivatedAt = newActivatedAt;
                }
            }

            if(newShelvedAt != null)
            {
                if(newShelvedAt.Value.Kind != DateTimeKind.Utc)
                {
                    newShelvedAt = newShelvedAt.Value.ToUniversalTime();
                }

                if(ShelvedAt == null || 
                   DateTime.Compare(ShelvedAt.Value, newShelvedAt.Value) < 0)
                {
                    //ShelvedAt is older or null
                    ShelvedAt = newShelvedAt;
                }
            }
        }

        public void Shelve()
        {
            SetTimeStamps(null, TimeStamp.NowUTC());
        }

        public void Activate()
        {
            SetTimeStamps(TimeStamp.NowUTC(), null);
        }
    }
}
