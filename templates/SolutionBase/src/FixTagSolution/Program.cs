using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FixTagSolution
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var startup= new Startup();
            IServiceCollection services = new ServiceCollection();
            var configBuilder= startup.GetConfig(services);
            IConfiguration config= configBuilder.Build();

      
            Console.WriteLine("Hello World!");
            //ReplaceTags("Name", "Pepe1");
           // ReplaceTags("PackageLicenseExpression", "licence", "../../../../Directory.Build.props");
            ReplaceTagsProjects("../../../", config, "../../../build/dependencies.props");


        }
        static void ReplaceTags(string tagName, string tagValue, string xmlPath= "path_to_xml_file.xml")
        {
            XDocument doc = XDocument.Load(xmlPath);

            //select all leaf elements having value equals "john"
            var elementsToUpdate = doc.Descendants()
                                      .Where(o => o.Name == tagName && !o.HasElements);

            //update elements value
            foreach (XElement element in elementsToUpdate)
            {
                element.Value = tagValue;
            }

            //save the XML back as file
            doc.Save(xmlPath);
        }

        static void ReplaceTagsProjects(string rootdir, IConfiguration config, string propertiesPath= "../../../build/dependencies.props")
        {
            Dictionary<string, XDocument> projectsDocs;
            ReplaceTagsBulider
            .GetMissingReferences(rootdir, out projectsDocs, config)
            .UpdateProperties(propertiesPath);

            projectsDocs.SaveProjects();

        }

      
    }

 


}
