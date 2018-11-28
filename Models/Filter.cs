using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WebResourceManager.Models
{
    public class Filter
    {
        public string Name { get; set; }

        public Func<WebResource, bool> ApplyFilter { get; set; }

        public static List<Filter> GetAllFilters()
        {
            return new List<Filter>
            {
                new Filter
                {
                    Name = "All",
                    ApplyFilter = (webresource) =>
                    {
                        return true;
                    }
                },
                new Filter
                {
                    Name = "Existing",
                    ApplyFilter = (webresource) =>
                    {
                        return webresource.Status == Constants.WebResourceStatus.Exists;
                    }
                },
                new Filter
                {
                    Name = "Local Only",
                    ApplyFilter = (webresource) =>
                    {
                        return !string.IsNullOrEmpty(webresource.FilePath);
                    }
                },
                new Filter
                {
                    Name = "New",
                    ApplyFilter = (webresource) =>
                    {
                        return webresource.Status == Constants.WebResourceStatus.New;
                    }
                },
                new Filter
                {
                    Name = "Not in solution",
                    ApplyFilter = (webresource) =>
                    {
                        return webresource.Status == Constants.WebResourceStatus.NotInSolution;
                    }
                },
                new Filter
                {
                    Name = "Remote Only",
                    ApplyFilter = (webresource) =>
                    {
                        return string.IsNullOrEmpty(webresource.FilePath);
                    }
                },
                new Filter
                {
                    Name = "Selected",
                    ApplyFilter = (webresource) =>
                    {
                        return webresource.IsSelected;
                    }
                }
            };
        }
    }
}
