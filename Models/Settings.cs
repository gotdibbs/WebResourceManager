using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using WebResourceManager.Data;
using WebResourceManager.Helpers;

namespace WebResourceManager.Models
{
    public class Settings
    {
        private const string FILE_NAME = "settings.json";
        public static string FOLDER_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebResourceManager");
        private static string FILE_PATH = Path.Combine(FOLDER_PATH, FILE_NAME);

        public string Version { get; set; }

        public List<Project> Projects { get; set; }

        public Guid? LastSelectedProjectId { get; set; }

        public Settings()
        {
            Projects = new List<Project>();
            Version = Assembly.GetCallingAssembly().GetName().Version.ToString();
        }

        public static Settings Load()
        {
            try
            {
                if (!Directory.Exists(FOLDER_PATH))
                {
                    Directory.CreateDirectory(FOLDER_PATH);
                }

                if (File.Exists(FILE_PATH))
                {
                    return Read();
                }
                else
                {
                    try
                    {
                        FileData.WriteJson(FILE_PATH, new Settings());

                        return Load();
                    }
                    catch (Exception ex)
                    {
                        Alert.Show("A problem was encountered while attempting to create the settings file. Detail: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A problem was encountered while attempting to load settings. Detail: " + ex.Message);
            }

            return null;
        }

        private static Settings Read()
        {
            var settings = FileData.ReadJson<Settings>(FILE_PATH);

            return Upgrade(settings);
        }

        private static Settings Upgrade(Settings settings)
        {
            if (settings == null)
            {
                return settings;
            }

            if (string.IsNullOrEmpty(settings.Version))
            {
                var result = MessageBox.Show("Invalid version found in settings.json. Drop all settings and recreate?",
                    string.Empty, MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(FILE_PATH);

                    return Load();
                }

                return null;
            }
            else
            {
                // We're up to date
                return settings;
            }
        }

        public void Save()
        {
            try
            {
                FileData.WriteJson(FILE_PATH, this);
            }
            catch (Exception ex)
            {
                Alert.Show("A problem was encountered while attempting to save settings. Detail: " + ex.Message);
            }
        }
    }
}
