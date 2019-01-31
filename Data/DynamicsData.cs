using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebResourceManager.Helpers;
using WebResourceManager.Models;

namespace WebResourceManager.Data
{
    public static class DynamicsData
    {
        public static List<Solution> GetSolutions(CrmServiceClient client)
        {
            if (client == null)
            {
                return new List<Solution>();
            }

            var fetch =
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                    <entity name='solution'>
	                    <attribute name='friendlyname' />
                        <attribute name='uniquename' />
                        <order attribute='friendlyname' descending='false' />
		                <filter type='and'>
		                  <condition attribute='isvisible' operator='eq' value='true' />
                          <condition attribute='ismanaged' operator='eq' value='false' />
                          <condition attribute='uniquename' operator='ne' value='Default' />
		                </filter>
                        <link-entity name='publisher' to='publisherid' from='publisherid' link-type='inner'>
                          <attribute name='customizationprefix' alias='customizationprefix' />
                        </link-entity>
                    </entity>
                </fetch>";

            return client.RetrieveMultiple(new FetchExpression(fetch)).Entities.Select(
                s => new Solution
                {
                    Id = s.Id,
                    Name = s.GetAttributeValue<string>("friendlyname"),
                    UniqueName = s.GetAttributeValue<string>("uniquename"),
                    CustomizationPrefix = s.GetAttributeValue<string>("customizationprefix")
                }).ToList();
        }

        public static WebResource GetExisting(CrmServiceClient client, string name)
        {
            if (client == null)
            {
                return null;
            }

            var query = $@"<fetch count='1'>
	                        <entity name='webresource'>
		                        <attribute name='name' />
                                <attribute name='webresourcetype' />
                                <attribute name='webresourceid' />
                                <attribute name='modifiedby' />
                                <attribute name='modifiedon' />
                                <filter>
                                    <condition attribute='name' operator='eq' value='{name}' />
                                </filter>
                                <link-entity name='systemuser' from='systemuserid' to='modifiedby'>
                                    <attribute name='fullname' alias='fullname' />
                                </link-entity>
	                        </entity>
                        </fetch>";

            var result = client.RetrieveMultiple(new FetchExpression(query));

            if (result == null || result.Entities == null || result.Entities.Count != 1)
            {
                return null;
            }

            return result.Entities.Select(e => new WebResource
            {
                Id = e.Id,
                Name = e.GetAttributeValue<string>("name"),
                ModifiedBy = e.GetAttributeValue<string>("fullname") ?? "Unknown",
                ModifiedOn = e.GetAttributeValue<DateTime>("modifiedon"),
                WebResourceType = (WebResourceType)e.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value,
                RowVersion = e.RowVersion
            }).First();
        }

        public static WebResource GetExistingContent(CrmServiceClient client, Guid id)
        {
            if (client == null)
            {
                return null;
            }

            var result = client.Retrieve("webresource", id, new ColumnSet("name", "webresourcetype", "content", "modifiedon", "modifiedby"));

            if (result == null)
            {
                return null;
            }

            return new WebResource
            {
                Id = result.Id,
                Name = result.GetAttributeValue<string>("name"),
                ModifiedBy = result.GetAttributeValue<string>("fullname") ?? "Unknown",
                ModifiedOn = result.GetAttributeValue<DateTime>("modifiedon"),
                Content = result.GetAttributeValue<string>("content"),
                WebResourceType = (WebResourceType)result.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value,
                RowVersion = result.RowVersion
            };
        }

        public static List<WebResource> GetAllBySolutionId(CrmServiceClient client, Guid solutionId)
        {
            if (client == null)
            {
                return new List<WebResource>();
            }

            var query = $@"<fetch>
	                        <entity name='webresource'>
                                <attribute name='name' />
                                <attribute name='webresourcetype' />
                                <attribute name='webresourceid' />
                                <attribute name='modifiedby' />
                                <attribute name='modifiedon' />
                                <link-entity name='solutioncomponent' from='objectid' to='webresourceid'>
                                    <filter>
                                        <condition attribute='solutionid' operator='eq' value='{solutionId}' />
                                    </filter>
                                </link-entity>
                                <link-entity name='systemuser' from='systemuserid' to='modifiedby'>
                                    <attribute name='fullname' alias='fullname' />
                                </link-entity>
	                        </entity>
                        </fetch>";

            var result = client.RetrieveMultiple(new FetchExpression(query));

            if (result == null || result.Entities == null || result.Entities.Count == 0)
            {
                return new List<WebResource>();
            }

            return result.Entities.Select(e => new WebResource
            {
                Id = e.Id,
                Name = e.GetAttributeValue<string>("name"),
                ModifiedBy = e.GetAttributeValue<string>("fullname") ?? "Unknown",
                ModifiedOn = e.GetAttributeValue<DateTime>("modifiedon"),
                WebResourceType = (WebResourceType)e.GetAttributeValue<OptionSetValue>("webresourcetype")?.Value,
                RowVersion = e.RowVersion
            }).ToList();
        }

        public static void Upsert(CrmServiceClient client, WebResource resource, Solution solution)
        {
            if (client == null)
            {
                return;
            }

            var entity = resource.ToEntity();

            var existing = GetExisting(client, resource.Name);

            if (resource.Create && existing != null)
            {
                Alert.Show($"The resource '{resource.Name}' appears to have been created by {existing.ModifiedBy} since we last checked the server. Create failed.");
                return;
            }
            else if (existing != null && resource.RowVersion != existing.RowVersion)
            {
                Alert.Show($"The resource '{resource.Name}' appears to have been changed by {existing.ModifiedBy} since we last checked the server. Overwrite prevented.");
            }

            if (resource.Create)
            {
                var id = client.Create(entity);
                resource.Id = id;
            }
            else
            {
                client.Update(entity);
            }

            if (solution != null &&
                !string.IsNullOrEmpty(solution.UniqueName) &&
                solution.UniqueName != "Default")
            {
                client.Execute(new AddSolutionComponentRequest
                {
                    ComponentType = (int)ComponentType.WebResource,
                    SolutionUniqueName = solution.UniqueName,
                    ComponentId = resource.Id.Value
                });
            }
        }

        public static void Publish(CrmServiceClient client, List<WebResource> webResources)
        {
            if (client == null)
            {
                return;
            }

            var resouresToPublish = webResources.Where(wr => !wr.Create);

            var paramXml = new StringBuilder();
            paramXml.AppendLine("<importexportxml>");
            paramXml.AppendLine("   <entities />");
            paramXml.AppendLine("   <optionsets />");
            paramXml.AppendLine("   <webresources>");
            foreach (var resource in resouresToPublish)
            {
                paramXml.AppendLine($"       <webresource>{resource.Id.ToString()}</webresource>");
            }
            paramXml.AppendLine("   </webresources>");
            paramXml.AppendLine("</importexportxml>");

            client.Execute(new PublishXmlRequest()
            {
                ParameterXml = paramXml.ToString()
            });
        }
    }
}
