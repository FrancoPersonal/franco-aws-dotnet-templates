using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FixTagSolution
{
    public static class ReplaceTagsBulider
    {

        private static IEnumerable<XElement> GetProjectElements(XDocument doc) => doc.Descendants().Where(o => !o.HasElements);


        public static IEnumerable<XElement> ReplaceBaseTags(XDocument doc, IConfiguration config)
        {
            var elements = ReplaceTagsBulider.GetProjectElements(doc);
            Dictionary<string, string> basetags = config.GetSection("basesettings").GetChildren().ToDictionary(x => x.Key, y => y.Value);
            foreach (XElement element in elements)
            {
                KeyValuePair<string, string> tag = basetags.FirstOrDefault(x => element.Value.StartsWith(x.Key));
                if (tag.Value != null)
                {
                    element.Value = tag.Value;
                }
            }
            return elements;
        }

        public static Dictionary<string, string> GetMissingReferences(string rootdir,out Dictionary<string, XDocument> projectsDocs, IConfiguration config)
        {
            projectsDocs = new Dictionary<string,XDocument>();
            string[] files = Directory.GetFiles(rootdir, "*.csproj", SearchOption.AllDirectories);
            Dictionary<string, string> missingReferences = new Dictionary<string, string>();            
            foreach (var file in files)
            {
                XDocument projectdoc = XDocument.Load(file);
                projectsDocs.Add(file, projectdoc);
                var elementsMissing = ReplaceTagsBulider
                    .ReplaceBaseTags(projectdoc, config)
                    .ReplaceMissingReferences(missingReferences);
            }

            return missingReferences;
        }


        public static Dictionary<string, string> ReplaceMissingReferences(this IEnumerable<XElement> elements, Dictionary<string, string> missingReferences)
        {
            foreach (XElement element in elements.Where(
                e=>e.HasAttributes &&
                e.Attributes().Any(a=>a.Name == "Version" && !a.Value.StartsWith("$"))                 
                ))
            {
                //if ( !element.Attributes()
                //    .Any(a => a.Name == "Version") ||
                //    element.Attribute("Version").Value.StartsWith("$")
                //    )
                //{
                //    continue;
                //}
                
                var key = element.Attribute("Include").Value.Replace(".", "");
                var value = element.Attribute("Version").Value;
                if (!missingReferences.ContainsKey(key))
                {
                    missingReferences.Add(key, value);
                }
                element.Attribute("Version").Value = ($@"$({key})");


            }
            return missingReferences;
        }

        public static void SaveProjects(this Dictionary<string, XDocument> projectsDocs)
        {

            foreach (var doc in projectsDocs)
            {
                doc.Value.Save(doc.Key);
            }
        }


        public static void UpdateProperties(this Dictionary<string, string> missingReferences, string propertiesPath)
        {
            var properties = XDocument.Load(propertiesPath);

            XElement packageSection = properties.Descendants()
                .FirstOrDefault(o => o.HasAttributes && o.Attributes()
                .Any(a => a.Name == "Label" && a.Value == "Packages"));

            var toUpdate = packageSection.Descendants();


            foreach (var element in missingReferences)
            {
                if (!toUpdate.Any(a => a.Name == element.Key))
                {
                    packageSection.Add(new XElement(element.Key, element.Value));
                }
            }
            properties.Save(propertiesPath);
        }

    }
}
