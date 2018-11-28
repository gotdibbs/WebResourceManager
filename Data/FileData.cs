using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebResourceManager.Helpers;
using WebResourceManager.Models;

namespace WebResourceManager.Data
{
    internal static class FileData
    {
        internal static WebResource[] GetFiles(Project project)
        {
            if (!Directory.Exists(project.Path))
            {
                throw new ArgumentException("Specified directory does not exist. Please check your configuration.");
            }

            return Constants.ValidExtensions
                .SelectMany(extension => GetFilteredFiles(project.Path, $"*.{extension}"))
                .Distinct()
                .Select(resourcePath => new WebResource(project, resourcePath))
                .ToArray();
        }

        private static List<string> GetFilteredFiles(string folder, string search)
        {
            var result = new List<string>();

            // Check folder for package.json or .gitignore, assume that means a sub-project we should ignore.
            if (File.Exists(Path.Combine(folder, ".gitignore")) || File.Exists(Path.Combine(folder, "package.json")))
            {
                return result;
            }

            result.AddRange(Directory.GetFiles(folder, search, SearchOption.TopDirectoryOnly));

            foreach (var directory in Directory.GetDirectories(folder))
            {
                result.AddRange(GetFilteredFiles(directory, search));
            }

            return result;
        }

        internal static void SaveWebResource(Project project, WebResource existing)
        {
            var projectPath = project.Path;
            if (!projectPath.EndsWith("\\"))
            {
                projectPath += "\\";
            }

            var remoteToLocalPath = existing.RemoteName.Replace('/', '\\');
            if (remoteToLocalPath.StartsWith("\\"))
            {
                remoteToLocalPath = remoteToLocalPath.Substring(remoteToLocalPath.IndexOf('\\') + 1);
            }

            var fullPath = Path.Combine(projectPath, remoteToLocalPath);

            var isNonstandard = false;

            if (string.IsNullOrEmpty(Path.GetExtension(fullPath)))
            {
                fullPath += $".{Constants.WebResourceExtensionMap[existing.WebResourceType]}";

                isNonstandard = true;
            }

            if (existing.RemoteName.LastIndexOf('/') < 0)
            {
                isNonstandard = true;
            }

            if (isNonstandard)
            {
                var o = new Override
                {
                    LocalRelativePath = fullPath.Replace(project.Path, string.Empty),
                    RemoteName = existing.RemoteName
                };

                project.Overrides.Add(o);

                project.Save();
            }

            WriteBase64(fullPath, existing.Content);
        }

        internal static string CreateTempFile(WebResource existing)
        {
            var tempFolder = Path.GetTempPath();
            var fileName = Path.GetFileName(existing.Name);

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
            }

            var tempFile = Path.Combine(tempFolder, fileName);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            File.WriteAllBytes(tempFile, Convert.FromBase64String(existing.Content));
            return tempFile;
        }

        internal static T ReadJson<T>(string path)
        {
            // Try to load from local file
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<T>(json);
            }

            return default(T);
        }

        internal static void WriteJson<T>(string path, T obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        internal static void WriteBase64(string path, string content)
        {
            var di = Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Encoding.UTF8.GetString(Convert.FromBase64String(content)));
        }
    }
}
