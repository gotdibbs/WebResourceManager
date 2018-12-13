using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebResourceManager.Data;
using WebResourceManager.Helpers;

namespace WebResourceManager.Models
{
    public class Project
    {
        private const string OVERRIDE_FILE_NAME = ".webresourceoverrides";
        private const string CONNECTION_KEY = "WebResourceManager";

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                {
                    _namespace = string.Empty;
                }
                else
                {
                    _namespace = value.Substring(0, value.LastIndexOf('/'));
                }
            }
        }

        [JsonIgnore]
        public string ConnectionString
        {
            get
            {
                return CredentialManager.GetCredential(ConnectionStringKey);
            }
            set
            {
                CredentialManager.SetCredentials(ConnectionStringKey, value, CredentialManagement.PersistanceType.LocalComputer);
            }
        }

        [JsonIgnore]
        public string ConnectionStringKey
        {
            get { return $"{CONNECTION_KEY}.{Id}"; }
        }

        public string SolutionName { get; set; }

        public string CustomizationPrefix { get; set; }

        public Guid SolutionId { get; set; }

        public Guid ImposterProfileId { get; set; }

        private List<Override> _overrides;
        [JsonIgnore]
        public List<Override> Overrides
        {
            get
            {
                if (_overrides != null)
                {
                    return _overrides;
                }

                var configPath = System.IO.Path.Combine(Path, OVERRIDE_FILE_NAME);

                // Try to load from local file
                _overrides = FileData.ReadJson<List<Override>>(configPath);
                
                if (_overrides != null)
                {
                    return _overrides;
                }

                // Initialize in memory, don't create until we absolutely need it
                _overrides = new List<Override>();

                return _overrides;
            }
            set
            {
                _overrides = value;
            }
        }

        public Project()
        {
            Id = Guid.NewGuid();
            Namespace = string.Empty;
        }

        public void Save()
        {
            var configPath = System.IO.Path.Combine(Path, OVERRIDE_FILE_NAME);
            FileData.WriteJson(configPath, _overrides);
        }
    }
}
