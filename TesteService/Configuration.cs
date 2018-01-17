using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TesteService
{
    public class Configuration
    {
        private Configuration()
        {
            this.displayName = "";
            this.description = "";
            this.serivceName = "";

            this.runProgramName = "";
            this.runProgramParams = "";
            this.runProgramPath = "";
            this.gracefullyCloseCommand = "";
            this.gracefullyCloseTimeoutSeconds = 0;
        }
        static Configuration()
        {
            instance = new Configuration();
        }

        private static Configuration instance;
        public static Configuration Instance
        {
            get
            {
                if (instance == null) instance = new Configuration();
                return Configuration.instance;
            }
            set { Configuration.instance = value; }
        }

        string displayName;
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        string serivceName;
        public string SerivceName
        {
            get { return serivceName; }
            set { serivceName = value; }
        }

        string runProgramName;
        public string RunProgramName
        {
            get { return runProgramName; }
            set { runProgramName = value; }
        }
        string runProgramParams;
        public string RunProgramParams
        {
            get { return runProgramParams; }
            set { runProgramParams = value; }
        }
        string runProgramPath;
        public string RunProgramPath
        {
            get { return runProgramPath; }
            set { runProgramPath = value; }
        }
        string gracefullyCloseCommand;
        public string GracefullyCloseCommand
        {
            get { return gracefullyCloseCommand; }
            set { gracefullyCloseCommand = value; }
        }
        int gracefullyCloseTimeoutSeconds;
        public int GracefullyCloseTimeoutSeconds
        {
            get { return gracefullyCloseTimeoutSeconds; }
            set { gracefullyCloseTimeoutSeconds = value; }
        }

        internal static void Reset()
        {
            instance = new Configuration();
        }

        public static void LoadFromDiskOrCreate()
        {
            if (File.Exists(Helper.GetCurrDir() + "\\config.xml")) LoadFromDisk();
            else
            {
                SaveToDisk();
            }
        }
        public static void SaveToDisk()
        {
            Helper.SaveToFile<Configuration>(Helper.GetCurrDir() + "\\config.xml", Instance);
        }
        public static void LoadFromDisk()
        {
            try
            {
                Instance = Helper.LoadFromFile<Configuration>(Helper.GetCurrDir() + "\\config.xml");
            }
            catch (Exception ex) { }
        }
    }
}