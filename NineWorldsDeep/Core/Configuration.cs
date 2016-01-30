﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineWorldsDeep.Core
{
    public class Configuration
    {
        private static bool _testMode = false;

        //TODO: hard-coded values need to be transformed into config files and defaults

        public static string AbletonProjectsFolder { get { return @"C:\NWD-AUX\abletonProjects"; } }
        public static string AudacityProjectsFolder { get { return @"C:\NWD-AUX\audacityProjects"; } }

        public static string AudacityWavExportsFolder { get { return @"C:\NWD-AUX\audacityWavExports"; } }

        public static string CubaseProjectsFolder { get { return @"C:\NWD-AUX\cubaseProjects"; } }
        public static string FLStudioProjectsFolder { get { return @"C:\NWD-AUX\flStudioProjects"; } }

        public static string ImagesFolder { get { return ProcessTestMode(@"NWD-AUX\images"); } }

        public static void EnsureDirectories()
        {
            //make sure directories exist
            Directory.CreateDirectory(Configuration.PhoneSyncSynergyFolder);
            Directory.CreateDirectory(Configuration.PhoneSyncSynergyArchivedFolder);
            Directory.CreateDirectory(Configuration.TabletSyncSynergyFolder);
            Directory.CreateDirectory(Configuration.TabletSyncSynergyArchivedFolder);
        }

        public static string GetPhoneSyncConfigFilePath(string fileNameWithoutExtension)
        {
            return PhoneSyncConfigFolder + "\\" + fileNameWithoutExtension + ".txt";
        }

        public static string PhoneSyncConfigFolder
        {
            get
            {
                return ProcessTestMode(@"NWD-SYNC\phone\NWD\config");
            }
        }

        public static string MySqlProjectsFolder
        {
            get
            {
                return @"C:\NWD\GERM\code\sql\";
            }
        }

        public static string PhoneSyncSynergyArchivedFolder
        {
            get
            {
                return ProcessTestMode(@"NWD-SYNC\phone\NWD\synergy\archived");
            }
        }

        private static string ProcessTestMode(string pathWithoutRoot)
        {
            string path = @"C:\";

            if (TestMode)
            {
                path += @"NWD-SNDBX\";
            }

            path += pathWithoutRoot;

            return path;
        }

        public static string TabletSyncSynergyArchivedFolder
        {
            get
            {
                return ProcessTestMode(@"NWD-SYNC\tablet\NWD\synergy\archived");
            }
        }

        public static string TabletSyncSynergyFolder
        {
            get
            {
                return ProcessTestMode(@"NWD-SYNC\tablet\NWD\synergy");
            }
        }

        public static string PhoneSyncSynergyFolder
        {
            get
            {
                return ProcessTestMode(@"NWD-SYNC\phone\NWD\synergy");
            }
        }

        public static string GetPhoneSyncSynergyArchiveFilePath(string listName)
        {
            return PhoneSyncSynergyArchivedFolder + "\\" + listName + ".txt";
        }

        public static string GetPhoneSyncSynergyFilePath(string listName)
        {
            return PhoneSyncSynergyFolder + "\\" + listName + ".txt";
        }

        public static string GetTabletSyncSynergyFilePath(string listName)
        {
            return TabletSyncSynergyFolder + "\\" + listName + ".txt";
        }

        public static string GetTabletSyncSynergyArchiveFilePath(string listName)
        {
            return TabletSyncSynergyArchivedFolder + "\\" + listName + ".txt";
        }

        public static string VisualStudioProjectsFolder
        {
            get
            {
                return @"C:\Users\Brent\Documents\Visual Studio 2015\Projects";
            }
        }

        public static string GetTagFilePath(string folderPath)
        {
            return folderPath + "\\fileTags.xml";
        }

        public static string VoiceMemosFolder
        {
            get
            {
                return @"C:\NWD-AUX\voicememos";
            }
        }

        public static string VoiceMemoTagFilePath
        {
            get
            {
                return VoiceMemosFolder + @"\fileTags.xml";
            }
        }

        public static string PhoneSyncDisplayNameIndexFile
        {
            get
            {
                return @"C:\NWD-SYNC\phone\NWD\config\DisplayNameIndex.txt";
            }
        }

        public static string PhoneSyncFileHashIndexFile
        {
            get
            {
                return @"C:\NWD-SYNC\phone\NWD\config\FileHashIndex.txt";
            }
        }

        public static bool TestMode
        {
            get
            {
                return _testMode;
            }
            set
            {
                _testMode = value;
            }
        }

    }
}
