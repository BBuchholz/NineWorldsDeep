﻿using NineWorldsDeep.Core;
using NineWorldsDeep.Synergy.V5;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NineWorldsDeep.Mnemosyne.V5;
using System.Xml;
using NineWorldsDeep.Tapestry.NodeUI;
using NineWorldsDeep.Hierophant;
using NineWorldsDeep.Archivist;
using NineWorldsDeep.Xml.Archivist;

namespace NineWorldsDeep.Xml
{
    public class Xml
    {
        #region "contract"

        //tags -
        private static string TAG_NWD = "nwd";

        //public static string TAG_SOURCE = "source";
        //public static string TAG_SOURCE_LOCATION_SUBSET_ENTRY = "sourceLocationSubsetEntry";
        //public static string TAG_SOURCE_EXCERPT = "sourceExcerpt";
        //public static string TAG_SOURCE_EXCERPT_VALUE = "sourceExcerptValue";
        //public static string TAG_SOURCE_EXCERPT_ANNOTATION = "sourceExcerptAnnotation";

        public static string TAG_MNEMOSYNE_SUBSET = "mnemosyneSubset";
        private static string TAG_MEDIA = "media";
        private static string TAG_TAG = "tag";
        private static string TAG_MEDIA_DEVICE = "mediaDevice";
        private static string TAG_PATH = "path";

        public static string TAG_SYNERGY_SUBSET = "synergySubset";
        private static string TAG_SYNERGY_LIST = "synergyList";
        private static string TAG_SYNERGY_ITEM = "synergyItem";
        private static string TAG_ITEM_VALUE = "itemValue";
        private static string TAG_TO_DO = "toDo";

        private static string TAG_HIEROPHANT_SUBSET = "hierophantSubset";
        private static string TAG_SEMANTIC_MAP = "semanticMap";
        private static string TAG_SEMANTIC_MAP_NAME = "semanticMapName";
        private static string TAG_SEMANTIC_DEFINITIONS = "semanticDefinitions";
        private static string TAG_SEMANTIC_DEFINITION = "semanticDefinition";
        private static string TAG_SEMANTIC_KEY = "semanticKey";
        private static string TAG_COLUMNS = "columns";
        private static string TAG_COLUMN = "column";
        private static string TAG_COLUMN_NAME = "columnName";
        private static string TAG_COLUMN_VALUE = "columnValue";
        private static string TAG_SEMANTIC_GROUPS = "semanticGroups";
        private static string TAG_SEMANTIC_GROUP = "semanticGroup";
        private static string TAG_SEMANTIC_GROUP_NAME = "semanticGroupName";
        private static string TAG_SEMANTIC_KEYS = "semanticKeys";


        //attributes
        private static string ATTRIBUTE_SHA1_HASH = "sha1Hash";
        private static string ATTRIBUTE_TAGS = "tags";
        private static string ATTRIBUTE_LIST_NAME = "listName";
        private static string ATTRIBUTE_ACTIVATED_AT = "activatedAt";
        private static string ATTRIBUTE_SHELVED_AT = "shelvedAt";
        private static string ATTRIBUTE_POSITION = "position";
        private static string ATTRIBUTE_COMPLETED_AT = "completedAt";
        private static string ATTRIBUTE_ARCHIVED_AT = "archivedAt";

        private static string ATTRIBUTE_TAG_VALUE = "tagValue";
        private static string ATTRIBUTE_TAGGED_AT = "taggedAt";
        private static string ATTRIBUTE_UNTAGGED_AT = "untaggedAt";
        private static string ATTRIBUTE_DESCRIPTION = "description";
        private static string ATTRIBUTE_VALUE = "value";
        private static string ATTRIBUTE_VERIFIED_PRESENT = "verifiedPresent";
        private static string ATTRIBUTE_VERIFIED_MISSING = "verifiedMissing";

        private static string ATTRIBUTE_FILE_NAME = "fileName";



        #endregion

        #region Archivist

        internal static List<ArchivistXmlSource> 
            RetrieveArchivistSourcesWithReaderAsync(
                string documentPath, 
                IAsyncStatusResponsive ui)
        {
            // mimics Xml.RetrieveMediaWithReaderAsync(...)
            List<ArchivistXmlSource> allSources = new List<ArchivistXmlSource>();

            //read once first to count elements

            int totalElements = 0;
            int count = 0;

            using (XmlReader reader = XmlReader.Create(documentPath))
            {
                while (reader.ReadToFollowing(TAG_SOURCE))
                {
                    totalElements++;
                }
            }

            using (XmlReader reader = XmlReader.Create(documentPath))
            {
                while (reader.Read())
                {
                    if (reader.Name == TAG_SOURCE)
                    {
                        count++;

                        XElement sourceEl = (XElement)XNode.ReadFrom(reader);

                        string type = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_TYPE);
                        string author = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_AUTHOR);
                        string director = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_DIRECTOR);
                        string title = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_TITLE);
                        string year = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_YEAR);
                        string url = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_URL);
                        string retrievalDate = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_RETRIEVAL_DATE);
                        string sourceTag = GetAttributeValueIfExists(sourceEl, ATTRIBUTE_TAG);

                        ui.StatusDetailUpdate(
                            "processing " + count + " of " +
                            totalElements + " xml source elements, type is: " +
                            type);

                        ArchivistXmlSource source = new ArchivistXmlSource()
                        {
                            SourceType = type,
                            Author = author,
                            Director = director,
                            Title = title,
                            Year = year,
                            Url = url,
                            RetrievalDate = retrievalDate,
                            SourceTag = sourceTag
                        };

                        //get location entries
                        foreach (XElement slseEl in sourceEl.Elements(TAG_SOURCE_LOCATION_SUBSET_ENTRY))
                        {
                            string location = GetAttributeValueIfExists(slseEl, ATTRIBUTE_LOCATION);
                            string locationSubset = GetAttributeValueIfExists(slseEl, ATTRIBUTE_LOCATION_SUBSET);
                            string locationSubsetEntry = GetAttributeValueIfExists(slseEl, ATTRIBUTE_LOCATION_SUBSET_ENTRY);
                            string verifiedPresent = GetAttributeValueIfExists(slseEl, ATTRIBUTE_VERIFIED_PRESENT);
                            string verifiedMissing = GetAttributeValueIfExists(slseEl, ATTRIBUTE_VERIFIED_MISSING);

                            ArchivistXmlLocationEntry slse = new ArchivistXmlLocationEntry()
                            {
                                Location = location,
                                LocationSubset = locationSubset,
                                LocationSubsetEntry = locationSubsetEntry,
                                VerifiedPresent = verifiedPresent,
                                VerifiedMissing = verifiedMissing
                            };

                            source.Add(slse);
                        }

                        //get excerpts
                        foreach(XElement excerptEl in sourceEl.Elements(TAG_SOURCE_EXCERPT))
                        {
                            string pages = GetAttributeValueIfExists(excerptEl, ATTRIBUTE_PAGES);
                            string beginTime = GetAttributeValueIfExists(excerptEl, ATTRIBUTE_BEGIN_TIME);
                            string endTime = GetAttributeValueIfExists(excerptEl, ATTRIBUTE_END_TIME);

                            XElement excerptValueEl = excerptEl.Element(TAG_SOURCE_EXCERPT_VALUE);
                            
                            ArchivistXmlSourceExcerpt excerpt = new ArchivistXmlSourceExcerpt()
                            {
                                Pages = pages,
                                BeginTime = beginTime,
                                EndTime = endTime
                            };

                            if (excerptValueEl != null)
                            {
                                string excerptValue = excerptValueEl.Value;

                                excerpt.ExcerptValue = excerptValue;
                            }

                            //get annotations
                            foreach(XElement annotationEl in excerptEl.Elements(TAG_SOURCE_EXCERPT_ANNOTATION))
                            {
                                XElement valueEl = annotationEl.Element(TAG_SOURCE_EXCERPT_ANNOTATION_VALUE);

                                if (valueEl != null)
                                {
                                    string annotationValue = valueEl.Value;

                                    ArchivistXmlExcerptAnnotation annotation =
                                        new ArchivistXmlExcerptAnnotation()
                                        {
                                            AnnotationValue = annotationValue
                                        };

                                    excerpt.Add(annotation);
                                }
                            }

                            //get tags
                            foreach(XElement tagEl in excerptEl.Elements(TAG_TAG))
                            {
                                string tagValue = GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAG_VALUE);
                                string taggedAt = GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAGGED_AT);
                                string untaggedAt = GetAttributeValueIfExists(tagEl, ATTRIBUTE_UNTAGGED_AT);

                                ArchivistXmlTag tag = new ArchivistXmlTag()
                                {
                                    TagValue = tagValue,
                                    TaggedAt = taggedAt,
                                    UntaggedAt = untaggedAt
                                };

                                excerpt.Add(tag);
                            }

                            source.Add(excerpt);
                        }

                        allSources.Add(source);
                    }
                }
            }

            return allSources;
        }

        #endregion

        public static XDocument DocumentFromPath(string xmlFilePath)
        {
            return XDocument.Load(xmlFilePath);
        }

        public static void AddSynergySubsetElement(XDocument doc, 
                                                   List<SynergyV5List> lists)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// expects UTC date string
        /// </summary>
        /// <param name="utcDateString">format: YYYY-MM-DD HH:MM:SS, UTC</param>
        /// <returns></returns>
        public static DateTime? ToTime(string utcDateString)
        {
            //DateTime? output = null;

            //try
            //{
            //    output = DateTime.ParseExact(dateString,
            //                          "yyyy-MM-dd HH:mm:ss",
            //                          CultureInfo.InvariantCulture);
            //}
            //catch (Exception)
            //{
            //    //do nothing
            //}

            //return output;

            return TimeStamp.YYYY_MM_DD_HH_MM_SS_UTC_ToDateTime(utcDateString);
        }


        public static XElement Export(SynergyV5List lst)
        {
            XElement synergyListEl =
                    new XElement(TAG_SYNERGY_LIST);

            synergyListEl.Add(new XAttribute(ATTRIBUTE_LIST_NAME, lst.ListName));

            string activatedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(lst.ActivatedAt);
            string shelvedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(lst.ShelvedAt);

            synergyListEl.Add(
                new XAttribute(ATTRIBUTE_ACTIVATED_AT, activatedAt));
            synergyListEl.Add(
                new XAttribute(ATTRIBUTE_SHELVED_AT, shelvedAt));

            for (int i = 0; i < lst.ListItems.Count; i++)
            {
                SynergyV5ListItem item = lst.ListItems[i];

                XElement synergyItemEl =
                    new XElement(TAG_SYNERGY_ITEM);

                synergyItemEl.Add(new XAttribute(ATTRIBUTE_POSITION, i));

                XElement itemValueEl =
                    new XElement(TAG_ITEM_VALUE);

                itemValueEl.SetValue(item.ItemValue);

                synergyItemEl.Add(itemValueEl);

                SynergyV5ToDo toDo = item.ToDo;

                if (toDo != null)
                {
                    string toDoActivatedAt =
                        TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.ActivatedAt);
                    string toDoCompletedAt =
                        TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.CompletedAt);
                    string toDoArchivedAt =
                        TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.ArchivedAt);

                    XElement toDoEl =
                        new XElement(TAG_TO_DO,
                            new XAttribute(ATTRIBUTE_ACTIVATED_AT, toDoActivatedAt),
                            new XAttribute(ATTRIBUTE_COMPLETED_AT, toDoCompletedAt),
                            new XAttribute(ATTRIBUTE_ARCHIVED_AT, toDoArchivedAt));

                    synergyItemEl.Add(toDoEl);
                }

                synergyListEl.Add(synergyItemEl);
            }

            return synergyListEl;
        }

        public static IEnumerable<SemanticMap> ImportHierophantSemanticMaps(string path)
        {
            //mimic RetrieveMediaWithReaderAsync()
            List<SemanticMap> allMaps = new List<SemanticMap>();
            
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if(reader.Name == TAG_SEMANTIC_MAP)
                    {
                        XElement semanticMapEl = (XElement)XNode.ReadFrom(reader);
                        SemanticMap semanticMap = new SemanticMap();

                        XElement semanticMapNameEl =
                            semanticMapEl.Element(TAG_SEMANTIC_MAP_NAME);

                        if(semanticMapNameEl != null)
                        {
                            semanticMap.Name = semanticMapNameEl.Value;
                        }

                        //process semanticDefinitionsEl
                        XElement semanticDefinitionsEl =
                            semanticMapEl.Element(TAG_SEMANTIC_DEFINITIONS);

                        if(semanticDefinitionsEl != null)
                        {
                            foreach(XElement semanticDefinitionEl in
                                semanticDefinitionsEl.Elements(
                                    TAG_SEMANTIC_DEFINITION))
                            {
                                semanticMap.Add(
                                    ImportSemanticDefinition(
                                        semanticDefinitionEl));
                            }
                        }

                        //process semanticGroupsEl
                        XElement semanticGroupsEl =
                            semanticMapEl.Element(TAG_SEMANTIC_GROUPS);

                        if(semanticGroupsEl != null)
                        {
                            foreach(XElement semanticGroupEl in
                                semanticGroupsEl.Elements(
                                    TAG_SEMANTIC_GROUP))
                            {
                                string groupName = 
                                    semanticGroupEl.Element(TAG_SEMANTIC_GROUP_NAME).Value;

                                if (!string.IsNullOrWhiteSpace(groupName))
                                {
                                    XElement semanticKeysEl =
                                        semanticGroupEl.Element(TAG_SEMANTIC_KEYS);

                                    if (semanticKeysEl != null)
                                    {
                                        foreach (XElement semanticKeyEl in
                                            semanticKeysEl.Elements(TAG_SEMANTIC_KEY))
                                        {
                                            SemanticKey semKey = ImportSemanticKey(semanticKeyEl);

                                            SemanticDefinition def = semanticMap[semKey];

                                            if (def != null)
                                            {
                                                semanticMap.SemanticGroup(groupName).Add(def);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        allMaps.Add(semanticMap);
                    }
                }
            }

            return allMaps;
        }

        
        private static SemanticKey ImportSemanticKey(XElement semanticKeyEl)
        {
            return new SemanticKey(semanticKeyEl.Value);
        }

        private static SemanticDefinition ImportSemanticDefinition(XElement semanticDefinitionEl)
        {            
            SemanticKey semKey = 
                ImportSemanticKey(semanticDefinitionEl.Element(TAG_SEMANTIC_KEY));

            SemanticDefinition semDef = new SemanticDefinition(semKey);

            XElement columnsEl = semanticDefinitionEl.Element(TAG_COLUMNS);

            foreach(XElement columnEl in columnsEl.Elements(TAG_COLUMN))
            {
                string columnName = columnEl.Element(TAG_COLUMN_NAME).Value;
                string columnValue = columnEl.Element(TAG_COLUMN_VALUE).Value;

                semDef[columnName] = columnValue;
            }

            return semDef;
        }

        public static XElement Export(List<SynergyV5List> synergyV5Lists)
        {
            XElement synergySubsetEl = new XElement(TAG_SYNERGY_SUBSET);

            foreach (SynergyV5List lst in synergyV5Lists)
            {
                synergySubsetEl.Add(Export(lst));
            }

            return synergySubsetEl;
        }


        //public static XElement Export(List<SynergyV5List> synergyV5Lists)
        //{
        //    XElement synergySubsetEl = new XElement(TAG_SYNERGY_SUBSET);

        //    foreach (SynergyV5List lst in synergyV5Lists)
        //    {
        //        XElement synergyListEl = 
        //            new XElement(TAG_SYNERGY_LIST);

        //        synergyListEl.Add(new XAttribute(ATTRIBUTE_LIST_NAME, lst.ListName));

        //        string activatedAt = 
        //            TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(lst.ActivatedAt);
        //        string shelvedAt =
        //            TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(lst.ShelvedAt);

        //        synergyListEl.Add(
        //            new XAttribute(ATTRIBUTE_ACTIVATED_AT, activatedAt));
        //        synergyListEl.Add(
        //            new XAttribute(ATTRIBUTE_SHELVED_AT, shelvedAt));

        //        for(int i = 0; i < lst.ListItems.Count; i++)
        //        {
        //            SynergyV5ListItem item = lst.ListItems[i];

        //            XElement synergyItemEl =
        //                new XElement(TAG_SYNERGY_ITEM);

        //            synergyItemEl.Add(new XAttribute(ATTRIBUTE_POSITION, i));

        //            XElement itemValueEl =
        //                new XElement(TAG_ITEM_VALUE);

        //            itemValueEl.SetValue(item.ItemValue);

        //            synergyItemEl.Add(itemValueEl);

        //            SynergyV5ToDo toDo = item.ToDo;

        //            if(toDo != null)
        //            {
        //                string toDoActivatedAt =
        //                    TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.ActivatedAt);
        //                string toDoCompletedAt =
        //                    TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.CompletedAt);
        //                string toDoArchivedAt =
        //                    TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(toDo.ArchivedAt); 

        //                XElement toDoEl =
        //                    new XElement(TAG_TO_DO,
        //                        new XAttribute(ATTRIBUTE_ACTIVATED_AT, toDoActivatedAt),
        //                        new XAttribute(ATTRIBUTE_COMPLETED_AT, toDoCompletedAt),
        //                        new XAttribute(ATTRIBUTE_ARCHIVED_AT, toDoArchivedAt));

        //                synergyItemEl.Add(toDoEl);
        //            }

        //            synergyListEl.Add(synergyItemEl);
        //        }

        //        synergySubsetEl.Add(synergyListEl);
        //    }

        //    return synergySubsetEl;
        //}

        public static List<SynergyV5List> RetrieveSynergyV5Lists(XDocument doc, string prefixMessage = "", IAsyncStatusResponsive statusEnabledUi = null)
        {
            List<SynergyV5List> allLists = new List<SynergyV5List>();

            try
            {
                foreach (XElement listEl in doc.Descendants(TAG_SYNERGY_LIST))
                {
                    string listName = listEl.Attribute(ATTRIBUTE_LIST_NAME).Value;
                    string activatedAt = listEl.Attribute(ATTRIBUTE_ACTIVATED_AT).Value;
                    string shelvedAt = listEl.Attribute(ATTRIBUTE_SHELVED_AT).Value;

                    if (statusEnabledUi != null) statusEnabledUi.StatusDetailUpdate(prefixMessage + "found list: " + listName);

                    DateTime? activatedAtTime = ToTime(activatedAt);
                    DateTime? shelvedAtTime = ToTime(shelvedAt);

                    SynergyV5List lst =
                        new SynergyV5List(listName);

                    lst.SetTimeStamps(activatedAtTime, shelvedAtTime);

                    foreach (XElement itemEl in listEl.Descendants(TAG_SYNERGY_ITEM))
                    {
                        string position = itemEl.Attribute(ATTRIBUTE_POSITION).Value;

                        string itemValue =
                            itemEl.Descendants(TAG_ITEM_VALUE).First().Value;

                        var toDos = itemEl.Descendants(TAG_TO_DO);

                        if (toDos.Count() > 0)
                        {
                            XElement toDoEl = toDos.First();

                            string itemActivatedAt = toDoEl.Attribute(ATTRIBUTE_ACTIVATED_AT).Value;
                            string completedAt = toDoEl.Attribute(ATTRIBUTE_COMPLETED_AT).Value;
                            string archivedAt = toDoEl.Attribute(ATTRIBUTE_ARCHIVED_AT).Value;

                            DateTime? itemActivatedAtTime = ToTime(itemActivatedAt);
                            DateTime? completedAtTime = ToTime(completedAt);
                            DateTime? archivedAtTime = ToTime(archivedAt);

                            SynergyV5ListItem item =
                                new SynergyV5ListItem(itemValue,
                                                      itemActivatedAtTime,
                                                      completedAtTime,
                                                      archivedAtTime);

                            lst.ListItems.Add(item);
                        }
                        else
                        {
                            lst.ListItems.Add(new SynergyV5ListItem(itemValue));
                        }
                    }

                    allLists.Add(lst);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;

                if(statusEnabledUi != null)
                {
                    statusEnabledUi.StatusDetailUpdate(prefixMessage + " ERROR: " + msg);
                }
            }

            return allLists;
        }

        /// <summary>
        /// returns the value of an attribute if found, and an 
        /// empty string if the attribute does not exist
        /// </summary>
        /// <param name="el"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        private static string GetAttributeValueIfExists(
            XElement el, string attributeName)
        {
            var attr = el.Attribute(attributeName);

            if(attr != null)
            {
                return attr.Value;
            }
            else
            {
                return "";
            }
        }
        
        public static List<Media> RetrieveMediaWithReaderAsync(
            string documentPath, IAsyncStatusResponsive ui)
        {
            List<Media> allMedia = new List<Media>();

            //read once first to count elements

            int totalElements = 0;
            int count = 0;
            
            using (XmlReader reader = XmlReader.Create(documentPath))
            {
                while (reader.ReadToFollowing(TAG_MEDIA))
                {
                    totalElements++;
                }
            }

            using (XmlReader reader = XmlReader.Create(documentPath))
            {
                //reader.ReadStartElement(TAG_NWD);
                //reader.ReadStartElement(TAG_MNEMOSYNE_SUBSET);

                while (reader.Read())
                {
                    //for testing
                    //if(count > 200)
                    //{
                    //    break;
                    //}

                    if (reader.Name == TAG_MEDIA)
                    {
                        count++;
                        
                        XElement mediaEl = (XElement)XNode.ReadFrom(reader);

                        string sha1Hash =
                            GetAttributeValueIfExists(mediaEl, ATTRIBUTE_SHA1_HASH);

                        //for testing
                        //if(sha1Hash.Equals("83559ec05eecef4625acde9e76bc1f06f118fecd", StringComparison.CurrentCultureIgnoreCase))
                        //{
                        //    bool testing = true;
                        //}

                        string fileName =
                            GetAttributeValueIfExists(mediaEl, ATTRIBUTE_FILE_NAME);

                        string description =
                            GetAttributeValueIfExists(mediaEl, ATTRIBUTE_DESCRIPTION);

                        ui.StatusDetailUpdate(
                            "processing " + count + " of " + 
                            totalElements + " xml media elements with hash: " + 
                            sha1Hash);

                        Media media = new Media()
                        {
                            MediaHash = sha1Hash,
                            MediaDescription = description,
                            MediaFileName = fileName
                        };

                        //get tags
                        foreach (XElement tagEl in mediaEl.Elements(TAG_TAG))
                        {

                            //tagValue
                            //taggedAt
                            //untaggedAt
                            //mediaHash
                            string tagValue =
                                GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAG_VALUE);

                            string taggedAt =
                                GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAGGED_AT);

                            string untaggedAt =
                                GetAttributeValueIfExists(tagEl, ATTRIBUTE_UNTAGGED_AT);

                            DateTime? taggedAtTime = ToTime(taggedAt);
                            DateTime? untaggedAtTime = ToTime(untaggedAt);

                            MediaTagging tagging = new MediaTagging()
                            {
                                MediaHash = media.MediaHash,
                                MediaTagValue = tagValue
                            };

                            tagging.SetTimeStamps(taggedAtTime, untaggedAtTime);

                            media.Add(tagging);
                        }

                        //get device paths
                        foreach (XElement mediaDeviceEl in mediaEl.Elements(TAG_MEDIA_DEVICE))
                        {
                            string deviceName =
                                GetAttributeValueIfExists(mediaDeviceEl, ATTRIBUTE_DESCRIPTION);

                            foreach (XElement pathEl in mediaDeviceEl.Descendants(TAG_PATH))
                            {
                                string pathValue =
                                    GetAttributeValueIfExists(pathEl, ATTRIBUTE_VALUE);

                                string verifiedPresent =
                                    GetAttributeValueIfExists(pathEl, ATTRIBUTE_VERIFIED_PRESENT);

                                string verifiedMissing =
                                    GetAttributeValueIfExists(pathEl, ATTRIBUTE_VERIFIED_MISSING);

                                DateTime? verifiedPresentTime = ToTime(verifiedPresent);
                                DateTime? verifiedMissingTime = ToTime(verifiedMissing);

                                DevicePath dp = new DevicePath()
                                {
                                    DeviceName = deviceName,
                                    DevicePathValue = pathValue
                                };

                                dp.SetTimeStamps(verifiedPresentTime, verifiedMissingTime);

                                media.Add(dp);
                            }
                        }

                        allMedia.Add(media);
                    }
                }

                //reader.ReadEndElement();
                //reader.ReadEndElement();
            }

            return allMedia;
        }


        public static List<Media> RetrieveMedia(XDocument doc)
        {
            List<Media> allMedia = new List<Media>();

            foreach(XElement mediaEl in doc.Elements(TAG_MEDIA))
            {
                string sha1Hash = 
                    GetAttributeValueIfExists(mediaEl, ATTRIBUTE_SHA1_HASH);

                string fileName =
                    GetAttributeValueIfExists(mediaEl, ATTRIBUTE_FILE_NAME);

                string description =
                    GetAttributeValueIfExists(mediaEl, ATTRIBUTE_DESCRIPTION);

                Media media = new Media()
                {
                    MediaHash = sha1Hash,
                    MediaDescription = description,
                    MediaFileName = fileName
                };

                //get tags
                foreach(XElement tagEl in mediaEl.Elements(TAG_TAG)){

                    //tagValue
                    //taggedAt
                    //untaggedAt
                    //mediaHash
                    string tagValue = 
                        GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAG_VALUE);

                    string taggedAt =
                        GetAttributeValueIfExists(tagEl, ATTRIBUTE_TAGGED_AT);

                    string untaggedAt =
                        GetAttributeValueIfExists(tagEl, ATTRIBUTE_UNTAGGED_AT);

                    DateTime? taggedAtTime = ToTime(taggedAt);
                    DateTime? untaggedAtTime = ToTime(untaggedAt);

                    MediaTagging tagging = new MediaTagging()
                    {
                        MediaHash = media.MediaHash,
                        MediaTagValue = tagValue
                    };

                    tagging.SetTimeStamps(taggedAtTime, untaggedAtTime);

                    media.Add(tagging);
                }

                //get device paths
                foreach(XElement mediaDeviceEl in mediaEl.Elements(TAG_MEDIA_DEVICE))
                {
                    string deviceName =
                        GetAttributeValueIfExists(mediaDeviceEl, ATTRIBUTE_DESCRIPTION);

                    foreach(XElement pathEl in mediaDeviceEl.Descendants(TAG_PATH))
                    {
                        string pathValue =
                            GetAttributeValueIfExists(pathEl, ATTRIBUTE_VALUE);

                        string verifiedPresent =
                            GetAttributeValueIfExists(pathEl, ATTRIBUTE_VERIFIED_PRESENT);

                        string verifiedMissing =
                            GetAttributeValueIfExists(pathEl, ATTRIBUTE_VERIFIED_MISSING);

                        DateTime? verifiedPresentTime = ToTime(verifiedPresent);
                        DateTime? verifiedMissingTime = ToTime(verifiedMissing);

                        DevicePath dp = new DevicePath()
                        {
                            DeviceName = deviceName,
                            DevicePathValue = pathValue
                        };

                        dp.SetTimeStamps(verifiedPresentTime, verifiedMissingTime);

                        media.Add(dp);
                    }
                }

                allMedia.Add(media);
            }

            return allMedia;
        }

        public static XElement CreateMediaElement(string hash)
        {
            XElement mediaEl = new XElement(TAG_MEDIA);

            mediaEl.Add(new XAttribute(ATTRIBUTE_SHA1_HASH, hash));

            return mediaEl;
        }

        public static XElement CreateTagElement(MediaTagging tag)
        {
            XElement tagEl = new XElement(TAG_TAG);

            //tagValue
            //taggedAt
            //untaggedAt

            string taggedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(tag.TaggedAt);

            string untaggedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(tag.UntaggedAt);

            tagEl.Add(new XAttribute(ATTRIBUTE_TAG_VALUE, tag.MediaTagValue));
            tagEl.Add(new XAttribute(ATTRIBUTE_TAGGED_AT, taggedAt));
            tagEl.Add(new XAttribute(ATTRIBUTE_UNTAGGED_AT, untaggedAt));

            return tagEl;
        }

        public static XElement CreateDeviceElement(string deviceName)
        {
            XElement mediaDeviceEl = new XElement(TAG_MEDIA_DEVICE);

            mediaDeviceEl.Add(new XAttribute(ATTRIBUTE_DESCRIPTION, deviceName));

            return mediaDeviceEl;
        }

        public static XElement CreatePathElement(DevicePath path)
        {
            XElement pathEl = new XElement(TAG_PATH);

            string verifiedPresent =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(path.VerifiedPresent);

            string verifiedMissing =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(path.VerifiedMissing);

            pathEl.Add(new XAttribute(ATTRIBUTE_VALUE, path.DevicePathValue));

            pathEl.Add(
                new XAttribute(ATTRIBUTE_VERIFIED_PRESENT, verifiedPresent));
            
            pathEl.Add(
                new XAttribute(ATTRIBUTE_VERIFIED_MISSING, verifiedMissing));
            
            return pathEl;
        }

        public static XDocument Export(SemanticMap semMap)
        {
            var lst = new List<SemanticMap>();
            lst.Add(semMap);
            return Export(lst);
        }
        public static XDocument Export(IEnumerable<SemanticMap> semanticMaps)
        {
            /*
             * 
             * desired output:
             * 
             * <nwd>
             *   <hierophantSubset>
             *     <semanticMap>
             *       <semanticMapName>name</semanticMapName>
             *       <semanticDefinitions>
             *         <semanticDefinition>
             *           <semanticKey>key as string</semanticKey>
             *           <columns>
             *             <column>
             *               <columnName>column name</columnName>
             *               <columnValue>column value</columnValue>
             *             </column>
             *           </columns>
             *         </semanticDefinition>
             *         <semanticDefinition/>
             *         <semanticDefinition/>
             *       </semanticDefinitions>
             *       <semanticGroups>
             *         <semanticGroup>
             *           <semanticGroupName>name</semanticGroupName>
             *           <semanticKeys>
             *             <semanticKey>just the key as string here</semanticKey>
             *             <semanticKey/>
             *             <semanticKey/>
             *           </semanticKeys>
             *         </semanticGroup>
             *         <semanticGroup/>
             *         <semanticGroup/>
             *       </semanticGroups>
             *     </semanticMap>
             *     <semanticMap/>
             *     <semanticMap/>
             *   </hierophantSubset>
             * </nwd>
             * 
            */
            
            XElement hierophantSubsetEl = new XElement(TAG_HIEROPHANT_SUBSET);
            
            foreach (var semanticMap in semanticMaps)
            {
                XElement semanticMapEl = new XElement(TAG_SEMANTIC_MAP);
                hierophantSubsetEl.Add(semanticMapEl);
                                
                XElement semanticMapNameEl =
                    new XElement(TAG_SEMANTIC_MAP_NAME);

                semanticMapEl.Add(semanticMapNameEl);

                semanticMapNameEl.Value = semanticMap.Name;

                XElement semanticDefinitionsEl =
                    new XElement(TAG_SEMANTIC_DEFINITIONS);

                semanticMapEl.Add(semanticDefinitionsEl);

                foreach (var def in semanticMap.SemanticDefinitions)
                {
                    semanticDefinitionsEl.Add(CreateSemanticDefinitionElement(def));
                }

                XElement semanticGroupsEl = new XElement(TAG_SEMANTIC_GROUPS);
                semanticMapEl.Add(semanticGroupsEl);

                foreach (var semGroupName in semanticMap.SemanticGroupNames)
                {
                    semanticGroupsEl.Add(
                        CreateSemanticGroupElement(
                            semanticMap.SemanticGroup(semGroupName),
                            semGroupName));
                }
            }

            return new XDocument(new XElement(TAG_NWD, hierophantSubsetEl));
        }

        private static XElement CreateSemanticGroupElement(
            SemanticMap semanticMapForGroup, string semanticGroupName)
        {
            XElement semanticKeysEl = new XElement(TAG_SEMANTIC_KEYS);

            XElement semanticGroupEl = 
                new XElement(TAG_SEMANTIC_GROUP,
                    new XElement(TAG_SEMANTIC_GROUP_NAME, semanticGroupName),
                    semanticKeysEl);

            foreach(var semKey in semanticMapForGroup.SemanticKeys)
            {
                semanticKeysEl.Add( 
                    new XElement(TAG_SEMANTIC_KEY, semKey.ToString()));
            }

            return semanticGroupEl;
        }

        private static XElement CreateSemanticDefinitionElement(SemanticDefinition def)
        {
            XElement semanticDefinitionEl =
                new XElement(TAG_SEMANTIC_DEFINITION);
            
            semanticDefinitionEl.Add(
                new XElement(TAG_SEMANTIC_KEY, def.SemanticKey.ToString()));
            
            XElement columnsEl = new XElement(TAG_COLUMNS);
            semanticDefinitionEl.Add(columnsEl);

            foreach(var colName in def.ColumnNames)
            {
                columnsEl.Add(
                    new XElement(TAG_COLUMN, 
                        new XElement(TAG_COLUMN_NAME, colName),
                        new XElement(TAG_COLUMN_VALUE, def[colName].ToString())));
            }

            return semanticDefinitionEl;
        }


        #region Archivist

        #region Archivist Tags and Attributes
        
        public static string TAG_ARCHIVIST_SUBSET = "archivistSubset";
        private static readonly string TAG_SOURCE = "source";
        private static readonly string TAG_SOURCE_LOCATION_SUBSET_ENTRY = "sourceLocationSubsetEntry";
        private static readonly string TAG_SOURCE_EXCERPT = "sourceExcerpt";
        private static readonly string TAG_SOURCE_EXCERPT_VALUE = "sourceExcerptValue";
        private static readonly string TAG_SOURCE_EXCERPT_ANNOTATION = "sourceExcerptAnnotation";
        private static readonly string TAG_SOURCE_EXCERPT_ANNOTATION_VALUE = "sourceExcerptAnnotationValue";

        private static readonly string ATTRIBUTE_TYPE = "type";
        private static readonly string ATTRIBUTE_AUTHOR = "author";
        private static readonly string ATTRIBUTE_DIRECTOR = "director";
        private static readonly string ATTRIBUTE_TITLE = "title";
        private static readonly string ATTRIBUTE_YEAR = "year";
        private static readonly string ATTRIBUTE_URL = "url";
        private static readonly string ATTRIBUTE_RETRIEVAL_DATE = "retrievalDate";
        private static readonly string ATTRIBUTE_TAG = "tag";
        private static readonly string ATTRIBUTE_LOCATION = "location";
        private static readonly string ATTRIBUTE_LOCATION_SUBSET = "locationSubset";
        private static readonly string ATTRIBUTE_LOCATION_SUBSET_ENTRY = "locationSubsetEntry";
        private static readonly string ATTRIBUTE_PAGES = "pages";
        private static readonly string ATTRIBUTE_BEGIN_TIME = "beginTime";
        private static readonly string ATTRIBUTE_END_TIME = "endTime";
        private static readonly string ATTRIBUTE_LINKED_AT = "linkedAt";
        private static readonly string ATTRIBUTE_UNLINKED_AT = "unlinkedAt";

        #endregion

        internal static XElement CreateSourceElement(ArchivistSource source, string sourceTypeValue)
        {
            XElement sourceEl = new XElement(TAG_SOURCE);

            sourceEl.Add(new XAttribute(ATTRIBUTE_TYPE, sourceTypeValue));
            sourceEl.Add(new XAttribute(ATTRIBUTE_AUTHOR, source.Author));
            sourceEl.Add(new XAttribute(ATTRIBUTE_DIRECTOR, source.Director));
            sourceEl.Add(new XAttribute(ATTRIBUTE_TITLE, source.Title));
            sourceEl.Add(new XAttribute(ATTRIBUTE_YEAR, source.Year));
            sourceEl.Add(new XAttribute(ATTRIBUTE_URL, source.Url));
            sourceEl.Add(new XAttribute(ATTRIBUTE_RETRIEVAL_DATE, source.RetrievalDate));
            sourceEl.Add(new XAttribute(ATTRIBUTE_TAG, source.SourceTag));

            return sourceEl;
        }
        
        internal static XElement CreateSourceLocationSubsetEntryElement(ArchivistSourceLocationSubsetEntry locationEntry)
        {
            XElement sourceLocationSubsetEntryEl =
                new XElement(TAG_SOURCE_LOCATION_SUBSET_ENTRY);

            sourceLocationSubsetEntryEl.Add(new XAttribute(ATTRIBUTE_LOCATION, locationEntry.SourceLocationValue));
            sourceLocationSubsetEntryEl.Add(new XAttribute(ATTRIBUTE_LOCATION_SUBSET, locationEntry.SourceLocationSubsetValue));
            sourceLocationSubsetEntryEl.Add(new XAttribute(ATTRIBUTE_LOCATION_SUBSET_ENTRY, locationEntry.SourceLocationSubsetEntryValue));


            string verifiedPresent =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(locationEntry.VerifiedPresent);

            string verifiedMissing =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(locationEntry.VerifiedMissing);


            sourceLocationSubsetEntryEl.Add(new XAttribute(ATTRIBUTE_VERIFIED_PRESENT, verifiedPresent));
            sourceLocationSubsetEntryEl.Add(new XAttribute(ATTRIBUTE_VERIFIED_MISSING, verifiedMissing));

            return sourceLocationSubsetEntryEl;
        }

        internal static XElement CreateSourceExcerptElement(ArchivistSourceExcerpt sourceExcerpt)
        {
            XElement sourceExcerptEl =
                new XElement(TAG_SOURCE_EXCERPT);

            sourceExcerptEl.Add(new XAttribute(ATTRIBUTE_PAGES, sourceExcerpt.ExcerptPages));
            sourceExcerptEl.Add(new XAttribute(ATTRIBUTE_BEGIN_TIME, sourceExcerpt.ExcerptBeginTime));
            sourceExcerptEl.Add(new XAttribute(ATTRIBUTE_END_TIME, sourceExcerpt.ExcerptEndTime));

            return sourceExcerptEl;
        }
        
        internal static XElement CreateSourceExcerptValueElement(string excerptValue)
        {
            XElement sourceExcerptValueEl =
                new XElement(TAG_SOURCE_EXCERPT_VALUE);

            sourceExcerptValueEl.SetValue(excerptValue);

            return sourceExcerptValueEl;
        }

        internal static XElement CreateSourceExcerptTagElement(TaggingXmlWrapper tag)
        {
            XElement tagEl = new XElement(TAG_TAG);
            

            string taggedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(tag.TaggedAt);

            string untaggedAt =
                TimeStamp.To_UTC_YYYY_MM_DD_HH_MM_SS(tag.UntaggedAt);

            tagEl.Add(new XAttribute(ATTRIBUTE_TAG_VALUE, tag.MediaTagValue));
            tagEl.Add(new XAttribute(ATTRIBUTE_TAGGED_AT, taggedAt));
            tagEl.Add(new XAttribute(ATTRIBUTE_UNTAGGED_AT, untaggedAt));

            return tagEl;
        }
        
        internal static XElement CreateSourceExcerptAnnotationElement(ArchivistSourceExcerptAnnotation annotation)
        {
            XElement sourceExcerptAnnotationEl =
                new XElement(TAG_SOURCE_EXCERPT_ANNOTATION);
            
            XElement sourceAnnotationValueEl =
                new XElement(TAG_SOURCE_EXCERPT_ANNOTATION_VALUE);
            sourceAnnotationValueEl.SetValue(annotation.SourceAnnotationValue);

            sourceExcerptAnnotationEl.Add(sourceAnnotationValueEl);

            return sourceExcerptAnnotationEl;
        }

        #endregion
    }
}
