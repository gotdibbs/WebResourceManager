using GalaSoft.MvvmLight;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebResourceManager.Helpers;

namespace WebResourceManager.Models
{
    public class WebResource : ObservableObject
    {
        public string RemoteName { get; set; }

        public string Name
        {
            get
            {
                return $"{CustomizationPrefix}_{RemoteName}";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    CustomizationPrefix = null;
                    RemoteName = null;
                }
                else
                {
                    var parts = value.Split('_');
                    CustomizationPrefix = parts[0];
                    RemoteName = value.Replace($"{parts[0]}_", string.Empty);
                }
            }
        }

        public string FileName
        {
            get
            {
                return Path.GetFileName(FilePath);
            }
        }

        private string _localRelativePath;
        public string LocalRelativePath
        {
            get { return _localRelativePath; }
            set { Set(ref _localRelativePath, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string CustomizationPrefix { get; set; }

        public WebResourceType WebResourceType { get; set; }

        public Guid? Id { get; set; }

        public bool Create { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        public string RowVersion { get; set; }

        public string Content { get; set; }

        public WebResource()
        {

        }

        internal WebResource(Project project, string resourcePath)
        {
            FilePath = resourcePath;
            CustomizationPrefix = project.CustomizationPrefix;
            LocalRelativePath = FilePath.Replace(project.Path, string.Empty);

            ModifiedOn = File.GetLastWriteTime(resourcePath);

            Override overrideConfig = project?.Overrides?.FirstOrDefault(o => o.LocalRelativePath == LocalRelativePath);

            if (overrideConfig != null)
            {
                if (string.IsNullOrEmpty(overrideConfig.RemoteName))
                {
                    Alert.Show($"Encountered an override for {FilePath} but it was invalid. Please review and try again.");
                }
                else
                {
                    RemoteName = overrideConfig.RemoteName;
                    Description = overrideConfig.Description;
                }
            }

            if (string.IsNullOrEmpty(RemoteName))
            {
                RemoteName = LocalRelativePath.Replace("\\", "/");

                if (!string.IsNullOrEmpty(project.Namespace))
                {
                    RemoteName = project.Namespace + RemoteName;
                }
            }

            WebResourceType = ConvertStringExtension(Path.GetExtension(FilePath));

            Validate();
        }

        private WebResourceType ConvertStringExtension(string extensionValue)
        {
            switch (extensionValue.Replace(".", string.Empty).ToLower())
            {
                case "css":
                    return WebResourceType.CSS;
                case "xml":
                    return WebResourceType.XML;
                case "gif":
                    return WebResourceType.GIF;
                case "htm":
                    return WebResourceType.HTML;
                case "html":
                    return WebResourceType.HTML;
                case "ico":
                    return WebResourceType.ICO;
                case "jpg":
                    return WebResourceType.JPG;
                case "png":
                    return WebResourceType.PNG;
                case "js":
                    return WebResourceType.JavaScript;
                case "xsl":
                    return WebResourceType.Stylesheet_XSL;
                case "svg":
                    return WebResourceType.Vector;
                case "resx":
                    return WebResourceType.String;
                default:
                    throw new ArgumentOutOfRangeException("extensionValue", $"{extensionValue} is not recognized as a valid file extension for a Web Resource.");
            }
        }

        public void Validate()
        {
            Regex inValidWRNameRegex = new Regex("[^a-z0-9A-Z_\\./]|[/]{2,}",
                (RegexOptions.Compiled | RegexOptions.CultureInvariant));

            // Test valid characters
            if (inValidWRNameRegex.IsMatch(RemoteName))
            {
                throw new Exception($"File at path '{FilePath}' contains an invalid character. Please check for invalid characters, rename the file, and try again.");
            }

            // Test length
            if (RemoteName.Length > 100)
            {
                throw new Exception($"File at path '{FilePath}' results in a name that is too long. Please consider renaming the file or reorganizing your project directories.");
            }
        }

        public Entity ToEntity()
        {
            if (string.IsNullOrEmpty(CustomizationPrefix))
            {
                throw new ArgumentNullException("Received invalid customization prefix, cannot continue");
            }

            var entity = new Entity(Constants.WebResourceLogicalName);
            entity["content"] = GetContents();
            entity["description"] = Description;
            entity["name"] = Name;
            entity["displayname"] = Path.GetFileName(RemoteName);
            entity["webresourcetype"] = new OptionSetValue((int)WebResourceType);

            if (Id != null)
            {
                entity["webresourceid"] = entity.Id = Id.Value;
            }

            return entity;
        }

        public string GetContents()
        {
            // TODO: allow minification switch

            return Convert.ToBase64String(new UTF8Encoding().GetBytes(File.ReadAllText(FilePath)));
        }

        internal void LoadPropertiesFromRemote(WebResource resource)
        {
            Id = resource.Id;
            Create = resource.Id != null;
            ModifiedBy = resource.ModifiedBy;
            ModifiedOn = resource.ModifiedOn;
            Content = resource.Content;
            Name = resource.Name;
            RowVersion = resource.RowVersion;
        }
    }
}
