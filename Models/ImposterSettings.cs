using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using WebResourceManager.Helpers;

namespace WebResourceManager.Models
{
    [DataContract]
    public class ImposterSettings
    {
        private const string FILE_NAME = "settings.json";
        public static string FOLDER_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Imposter.Fiddler");
        private static string FILE_PATH = Path.Combine(FOLDER_PATH, FILE_NAME);

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "profiles")]
        public List<ImposterProfile> Profiles { get; set; }

        public static ImposterSettings Load()
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
                    Alert.Show("Looks like Imposter for Fiddler isn't installed! Maybe try installing it first?");
                }
            }
            catch (Exception ex)
            {
                Alert.Show("A problem was encountered while attempting to load Imposter's profiles. Detail: " + ex.Message);
            }

            return null;
        }

        private static ImposterSettings Read()
        {
            var settingsJson = File.ReadAllText(FILE_PATH);

            var json = new DataContractJsonSerializer(typeof(ImposterSettings));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(settingsJson)))
            {
                var settings = (ImposterSettings)json.ReadObject(stream);

                return settings;
            }
        }
    }
}
