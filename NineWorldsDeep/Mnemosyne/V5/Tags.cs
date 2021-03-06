﻿using NineWorldsDeep.Core;
using NineWorldsDeep.Tapestry.NodeUI;
using NineWorldsDeep.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineWorldsDeep.Mnemosyne.V5
{
    public class Tags
    {
        public static void UpdateTagStringForHash(string hash, string oldTagString, string newTagString)
        {
            int mediaIdForHash = Hashes.MediaIdForHash(hash);

            List<MediaTagging> taggings = 
                TagStringChangesToTaggings(mediaIdForHash, oldTagString, newTagString);

            Configuration.DB.MediaSubset.EnsureMediaTaggings(taggings);
        }

        private static List<MediaTagging> TagStringChangesToTaggings(int mediaIdForHash, string oldTagString, string newTagString)
        {
            //get tags that changed (in one not the other, check both for new and removed)
            string[] seps = { "," };

            //var oldTags = oldTagString
            //    .ToLower()
            //    .Split(seps, StringSplitOptions.RemoveEmptyEntries)
            //    .Select(x => x.Trim())
            //    .Where(x => !string.IsNullOrWhiteSpace(x));

            //var newTags = newTagString
            //    .ToLower()
            //    .Split(seps, StringSplitOptions.RemoveEmptyEntries)
            //    .Select(x => x.Trim())
            //    .Where(x => !string.IsNullOrWhiteSpace(x));

            var oldTags = oldTagString
                .Split(seps, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var newTags = newTagString
                .Split(seps, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));

            List<Tag> tagsToBeTagged = new List<Tag>();
            List<Tag> tagsToBeUntagged = new List<Tag>();

            foreach (string tag in newTags)
            {
                if (!oldTags.Contains(tag))
                {
                    //its a new one
                    tagsToBeTagged.Add(new Tag() { TagValue = tag });
                }
            }

            foreach (string tag in oldTags)
            {
                if (!newTags.Contains(tag))
                {
                    //its been removed
                    tagsToBeUntagged.Add(new Tag() { TagValue = tag });
                }
            }

            var db = Configuration.DB.MediaSubset;

            //pass the list into the db.PopulateTagIds(List<Tag>)
            db.PopulateTagIds(tagsToBeTagged);
            db.PopulateTagIds(tagsToBeUntagged);

            //create media taggings for just the changed ones
            //each needs MediaId and MediaTagId set, also TaggedAt and UntaggedAt
            //should be set if removed or added accordingly
            List<MediaTagging> taggings = new List<MediaTagging>();

            foreach (Tag tag in tagsToBeTagged)
            {
                var mt = new MediaTagging()
                {
                    MediaId = mediaIdForHash,
                    MediaTagId = tag.TagId
                };

                mt.Tag();
                taggings.Add(mt);
            }

            foreach (Tag tag in tagsToBeUntagged)
            {
                var mt = new MediaTagging()
                {
                    MediaId = mediaIdForHash,
                    MediaTagId = tag.TagId
                };

                mt.Untag();
                taggings.Add(mt);
            }

            return taggings;
        }
        
        public static string ToTagString(List<MediaTagging> list)
        {
            return string.Join(", ", 
                list.Where(y => y.IsTagged())
                    .Select(x => x.MediaTagValue));
        }

        public static List<string> ToTagList(string tagString)
        {
            List<string> tags = new List<string>();

            foreach(string tag in tagString.Split(','))
            {
                tags.Add(tag.Trim());
            }

            return tags;
        }

        public static TagStringHashIndex GetTagStringHashIndexFromXml(SyncProfile sp, IAsyncStatusResponsive ui)
        {
            TagStringHashIndex tshIndex = new TagStringHashIndex();

            var allPaths = Configuration.GetMnemosyneXmlImportPaths(sp);
            var count = 0;
            var total = allPaths.Count();

            //for each xmlFile in profile folder 
            //  for each media Xml.RetrieveMedia(new XDocument(xmlFile))
            //      tshIndex.GetMediaByHash(media.MediaHash).Merge(media)

            string detail = "reading mnemosyne xml files";
            ui.StatusDetailUpdate(detail);

            foreach (string path in allPaths)
            {
                count++;

                if (!string.IsNullOrWhiteSpace(path))
                {
                    string prefix = "file " + count + " of " + total;

                    ui.StatusDetailUpdate(prefix);

                    string fileName = System.IO.Path.GetFileName(path);

                    //XDocument doc = Xml.Xml.DocumentFromPath(path);

                    //List<Media> allMedia = Xml.Xml.RetrieveMedia(doc);

                    List<Media> allMedia =
                        Xml.Xml.RetrieveMediaWithReaderAsync(path, ui);
                    
                    foreach(Media m in allMedia)
                    {
                        tshIndex.Add(m);
                    }
                }
            }

            return tshIndex;
        }

        public static List<string> GetRemovedTagValues(string oldTagString, string newTagString)
        {
            List<string> oldTags = Tags.ToTagList(oldTagString);
            List<string> newTags = Tags.ToTagList(newTagString);

            List<string> removedTags = new List<string>();

            foreach (string tag in oldTags)
            {
                if (!newTags.Contains(tag))
                {
                    removedTags.Add(tag);
                }
            }

            return removedTags;
        }

        public static List<string> GetAddedTagValues(string oldTagString, string newTagString)
        {
            List<string> oldTags = Tags.ToTagList(oldTagString);
            List<string> newTags = Tags.ToTagList(newTagString);

            List<string> addedTags = new List<string>();

            foreach (string tag in newTags)
            {
                if (!oldTags.Contains(tag))
                {
                    addedTags.Add(tag);
                }
            }

            return addedTags;
        }
    }
}
