﻿using NineWorldsDeep.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Collections.ObjectModel;
using NineWorldsDeep.Sqlite;

namespace NineWorldsDeep.Synergy
{
    public class SynergySqliteDbAdapter
    {
        public IEnumerable<SynergyList> GetActiveLists()
        {
            return GetLists(true);
        }

        public IEnumerable<SynergyList> GetLists(bool active)
        {
            Dictionary<string, SynergyList> lists =
                new Dictionary<string, SynergyList>();

            try
            {
                using (var conn =
                    new SQLiteConnection(@"Data Source=" +
                        Configuration.GetSqliteDbPath("nwd")))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            cmd.CommandText =
                                //"SELECT l.ListName, " +
                                //       "i.ItemValue, " +
                                //       "f.CompletedAt, " +
                                //       "f.ArchivedAt " +
                                //"FROM List l " +
                                //"JOIN Fragment f ON l.ListId = f.ListId " +
                                //"JOIN Item i ON f.ItemId = i.ItemId " +
                                //"WHERE l.ListActive = @active " +
                                //"AND (f.CompletedAt IS NULL OR " +
                                //     "f.CompletedAt = '') " +
                                //"AND (f.ArchivedAt IS NULL OR " +
                                //     "f.ArchivedAt = '')";
                                "SELECT l.ListName, " +
                                       "i.ItemValue, " +
                                       "f.CompletedAt, " +
                                       "f.ArchivedAt " +
                                "FROM List l " +
                                "JOIN Fragment f ON l.ListId = f.ListId " +
                                "JOIN Item i ON f.ItemId = i.ItemId " +
                                "WHERE l.ListActive = @active " +
                                "AND (f.CompletedAt IS NULL OR " +
                                     "f.CompletedAt = '') " +
                                "AND (f.ArchivedAt IS NULL OR " +
                                     "f.ArchivedAt = '')";

                            cmd.Parameters.AddWithValue("@active", active);

                            using(var rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    string listName = rdr.GetString(0);
                                    string itemValue = rdr.GetString(1);
                                    string completedAt = "";
                                    string archivedAt = "";

                                    if (!rdr.IsDBNull(2))
                                    {
                                        completedAt = rdr.GetString(2);
                                    }

                                    if (!rdr.IsDBNull(3))
                                    {
                                        archivedAt = rdr.GetString(3);
                                    }

                                    if (!lists.ContainsKey(listName))
                                    {
                                        lists[listName] = new SynergyList()
                                        {
                                            Name = listName
                                        };
                                    }

                                    SynergyItem si = new SynergyItem();

                                    si.TurnOffFragmentUpdating();
                                    si.Item = itemValue;
                                    si.CompletedAt = completedAt;
                                    si.ArchivedAt = archivedAt;
                                    si.TurnOnFragmentUpdatingAndUpdate();

                                    lists[listName].AddItem(si);
                                }                                
                            }

                            transaction.Commit();
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                //lets just throw it for now, but put something here eventually
                throw ex;
            }

            return lists.Values
                .OrderBy(x => x.Name)
                .ToList<SynergyList>();
        }

        public void Save(IEnumerable<SynergyList> _lists)
        {
            try
            {
                using (var conn =
                    new SQLiteConnection(@"Data Source=" +
                        Configuration.GetSqliteDbPath("nwd")))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        using (var transaction = conn.BeginTransaction())
                        {

                            foreach(var lst in _lists)
                            {
                                int listId = EnsureIdForListName(lst.Name, cmd);

                                foreach(var si in lst.Items)
                                {
                                    int itemId = EnsureIdForItemValue(si.Item, cmd);

                                    UpsertFragment(listId, itemId, si, cmd);
                                }
                            }

                            transaction.Commit();
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                //lets just throw it for now, but put something here eventually
                throw ex;
            }
        }

        public void UpdateActiveInactive(IEnumerable<SynergyList> setToActive, IEnumerable<SynergyList> setToInactive)
        {
            try
            {
                using (var conn =
                    new SQLiteConnection(@"Data Source=" +
                        Configuration.GetSqliteDbPath("nwd")))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            foreach (SynergyList sl in setToActive)
                            {
                                SetActive(sl.Name, true, cmd);
                            }

                            foreach (SynergyList sl in setToInactive)
                            {
                                SetActive(sl.Name, false, cmd);
                            }

                            transaction.Commit();
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                //lets just throw it for now, but put something here eventually
                throw ex;
            }
        }

        private void SetActive(string listName, bool active, SQLiteCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText =
                //"UPDATE List SET ListActive = @active WHERE ListName = @listName";
                "UPDATE " + NwdContract.TABLE_LIST +
                " SET " + NwdContract.COLUMN_LIST_ACTIVE + " = @active " +
                "WHERE " + NwdContract.COLUMN_LIST_NAME + " = @listName";
            cmd.Parameters.AddWithValue("@active", active);
            cmd.Parameters.AddWithValue("@listName", listName);
            cmd.ExecuteNonQuery();
        }

        private void UpsertFragment(int listId, int itemId, SynergyItem si, SQLiteCommand cmd)
        {
            //need to do a separate update attemp for
            //each value to avoid overwriting existing
            //values or ignoring updates to rows
            //with some values already populated
            //but others still null

            //store updatedAt, we will update this along with
            //fragment value as those are not conditional
            //updates (always overwritten on every storage op)
            string updatedAt = 
                NwdUtils.GetTimeStamp_yyyyMMddHHmmss();

            //attempt update for CompletedAt
            cmd.Parameters.Clear();
            cmd.CommandText =
                                //"UPDATE OR IGNORE Fragment " +
                                //"SET CompletedAt = @completedAt " +
                                //"WHERE ListId = @listId " +
                                //"AND ItemId = @itemId " +
                                //"AND CompletedAt IS NOT @completedAt " +
                                //"AND (CompletedAt IS NULL " +
                                //     "OR CompletedAt = '') ";
                "UPDATE OR IGNORE " + NwdContract.TABLE_FRAGMENT + " " +
                "SET " + NwdContract.COLUMN_COMPLETED_AT + " = @completedAt " +
                "WHERE " + NwdContract.COLUMN_LIST_ID + " = @listId " +
                "AND " + NwdContract.COLUMN_ITEM_ID + " = @itemId " +
                "AND " + NwdContract.COLUMN_COMPLETED_AT + " IS NOT @completedAt " +
                "AND (" + NwdContract.COLUMN_COMPLETED_AT + " IS NULL " +
                     "OR " + NwdContract.COLUMN_COMPLETED_AT + " = '') ";

            cmd.Parameters.AddWithValue("@completedAt", si.CompletedAt);
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.ExecuteNonQuery();

            //attempt update for ArchivedAt
            cmd.Parameters.Clear();
            cmd.CommandText =
                //"UPDATE OR IGNORE Fragment " +
                //"SET ArchivedAt = @archivedAt " +
                //"WHERE ListId = @listId " +
                //"AND ItemId = @itemId " +
                //"AND ArchivedAt IS NOT @archivedAt " +
                //"AND (ArchivedAt IS NULL " +
                //     "OR ArchivedAt = '') ";
                "UPDATE OR IGNORE " + NwdContract.TABLE_FRAGMENT + " " +
                "SET " + NwdContract.COLUMN_ARCHIVED_AT + " = @archivedAt " +
                "WHERE " + NwdContract.COLUMN_LIST_ID + " = @listId " +
                "AND " + NwdContract.COLUMN_ITEM_ID + " = @itemId " +
                "AND " + NwdContract.COLUMN_ARCHIVED_AT + " IS NOT @archivedAt " +
                "AND (" + NwdContract.COLUMN_ARCHIVED_AT + " IS NULL " +
                     "OR " + NwdContract.COLUMN_ARCHIVED_AT + " = '') ";

            cmd.Parameters.AddWithValue("@archivedAt", si.ArchivedAt);
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.ExecuteNonQuery();
            
            //attempt update for Fragment
            cmd.Parameters.Clear();
            cmd.CommandText =
                //"UPDATE OR IGNORE Fragment " +
                //"SET FragmentValue = @fragVal, " +
                //    "UpdatedAt = @updatedAt " +
                //"WHERE ListId = @listId " +
                //"AND ItemId = @itemId ";
                "UPDATE OR IGNORE " + NwdContract.TABLE_FRAGMENT + " " +
                "SET " + NwdContract.COLUMN_FRAGMENT_VALUE + " = @fragVal, " +
                    NwdContract.COLUMN_UPDATED_AT + " = @updatedAt " +
                "WHERE " + NwdContract.COLUMN_LIST_ID + " = @listId " +
                "AND " + NwdContract.COLUMN_ITEM_ID + " = @itemId ";

            cmd.Parameters.AddWithValue("@fragVal", si.Fragment);
            cmd.Parameters.AddWithValue("@updatedAt", updatedAt);
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.ExecuteNonQuery();

            //if all the above fail and are ignored, this will work
            //attempt insert for Fragment
            cmd.Parameters.Clear();
            cmd.CommandText =
            //"INSERT OR IGNORE INTO Fragment (ListId, " +
            //                                "ItemId, " +
            //                                "FragmentValue, " +
            //                                "CompletedAt, " +
            //                                "ArchivedAt, " +
            //                                "UpdatedAt) " +
            //"VALUES (@listId, " +
            //        "@itemId, " +
            //        "@fragVal, " +
            //        "@completedAt, " +
            //        "@archivedAt, " +
            //        "@updatedAt) ";
            "INSERT OR IGNORE INTO " + NwdContract.TABLE_FRAGMENT +
                " (" + NwdContract.COLUMN_LIST_ID + ", " +
                    "" + NwdContract.COLUMN_ITEM_ID + ", " +
                    "" + NwdContract.COLUMN_FRAGMENT_VALUE + ", " +
                    "" + NwdContract.COLUMN_COMPLETED_AT + ", " +
                    "" + NwdContract.COLUMN_ARCHIVED_AT + ", " +
                    "" + NwdContract.COLUMN_UPDATED_AT + ") " +
            "VALUES (@listId, " +
                    "@itemId, " +
                    "@fragVal, " +
                    "@completedAt, " +
                    "@archivedAt, " +
                    "@updatedAt) ";

            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@fragVal", si.Fragment);
            cmd.Parameters.AddWithValue("@completedAt", si.CompletedAt);
            cmd.Parameters.AddWithValue("@archivedAt", si.ArchivedAt);
            cmd.Parameters.AddWithValue("@updatedAt", updatedAt);
            cmd.ExecuteNonQuery();
        }

        private int EnsureIdForItemValue(string item, SQLiteCommand cmd)
        {
            return EnsureIdForValue(NwdContract.TABLE_ITEM,
                                    NwdContract.COLUMN_ITEM_ID,
                                    NwdContract.COLUMN_ITEM_VALUE,
                                    item,
                                    cmd);
            //int id = -1;

            ////insert or ignore
            //cmd.Parameters.Clear();
            //cmd.CommandText =
            //    "INSERT OR IGNORE INTO Item (ItemValue) VALUES (@itemVal)";
            //cmd.Parameters.AddWithValue("@itemVal", item);
            //cmd.ExecuteNonQuery();

            ////select value
            //cmd.Parameters.Clear(); //since we will be reusing command
            //cmd.CommandText = "SELECT ItemId FROM Item " +
            //                  "WHERE ItemValue = @itemVal";
            //cmd.Parameters.AddWithValue("@itemVal", item);

            //using (var rdr = cmd.ExecuteReader())
            //{
            //    if (rdr.Read()) //if statement cause we only need the first
            //    {
            //        id = rdr.GetInt32(0);
            //    }
            //}

            //return id;
        }

        private int EnsureIdForListName(string name, SQLiteCommand cmd)
        {
            //return EnsureIdForValue("List", 
            //                        "ListId", 
            //                        "ListName", 
            //                        name, 
            //                        cmd);
            
            return EnsureIdForValue(NwdContract.TABLE_LIST,
                                    NwdContract.COLUMN_LIST_ID,
                                    NwdContract.COLUMN_LIST_NAME,
                                    name,
                                    cmd);
        }

        private int EnsureIdForValue(string tableName, 
                                     string idColumnName, 
                                     string valueColumnName,
                                     string valueToEnsure, 
                                     SQLiteCommand cmd)
        {
            //prevent invalid names for columns and tables (according to NWD naming conventions)
            NwdUtils.ValidateColumnAndTableName(tableName);
            NwdUtils.ValidateColumnAndTableName(idColumnName);
            NwdUtils.ValidateColumnAndTableName(valueColumnName);

            int id = -1;

            //insert or ignore
            cmd.Parameters.Clear();
            cmd.CommandText =
                "INSERT OR IGNORE INTO "+ tableName + " (" + valueColumnName + ") VALUES (@colVal)";
            cmd.Parameters.AddWithValue("@colVal", valueToEnsure);
            cmd.ExecuteNonQuery();

            //select value
            cmd.Parameters.Clear(); //since we will be reusing command
            cmd.CommandText = "SELECT " + idColumnName + " FROM " + tableName + " " +
                              "WHERE " + valueColumnName + " = @colVal";
            cmd.Parameters.AddWithValue("@colVal", valueToEnsure);

            using (var rdr = cmd.ExecuteReader())
            {
                if (rdr.Read()) //if statement cause we only need the first
                {
                    id = rdr.GetInt32(0);
                }
            }

            return id;
        }
    }
    
    /////////////////////////////////////connection/transaction template
    //    try
    //    {
    //        using (var conn =
    //            new SQLiteConnection(@"Data Source=" +
    //                Configuration.GetSqliteDbPath("nwd")))
    //        {
    //            conn.Open();

    //            using (var cmd = new SQLiteCommand(conn))
    //            {
    //                using (var transaction = conn.BeginTransaction())
    //                {

    //////////////////////////////////////////CODE HERE//////////////////////////////////////

    //                    transaction.Commit();
    //                }
    //            }

    //            conn.Close();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        //lets just throw it for now, but put something here eventually
    //        throw ex;
    //    }
}