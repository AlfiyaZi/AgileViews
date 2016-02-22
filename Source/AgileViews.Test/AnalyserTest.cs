﻿using System.Linq;
using AgileViews.Export.Jekyll;
using AgileViews.Export.Svg;
using AgileViews.Model;
using AgileViews.Scrape;
using Xunit;

namespace AgileViews.Test
{
    public class AnalyserTest
    {
        [Fact]
        public void FoundProjects()
        {
            // arrange
            var analyzer = new Analyser(@"..\..\..\AgileViews.sln");

            // act
            var projects = analyzer.Projects(p => 
            p.Name.Contains("Agile") && !p.Name.Contains("Test"));

            var classes = analyzer.Classes(projects, c => true).ToList();
            var interfaces = analyzer.Interfaces(projects, i => true).ToList();

            var workspace = new Workspace();
            var model = workspace.GetModel();

            model.Add(projects.Single());
            model.AddAll(classes);
            model.AddAll(interfaces);

            foreach (var c in classes)
            {
                model.AddAll(c.RelationshipsFromClass());
            }

            model.ResolveNodes();

//            var dict = projects.ToDictionary(p => p.Id, p => p);
//            var elements = projects.ToDictionary(p => p.Id, p => system.AddContainer(p.Name, p.AssemblyName));
//
//            foreach (var p in projects)
//            {
//                foreach (var reference in p.ProjectReferences)
//                {
//                    if (dict.ContainsKey(reference.ProjectId))
//                    {
//                        elements[p.Id].Uses(elements[reference.ProjectId], "reference");
//                    }
//                }
//            }

            var view = workspace.CreateView(projects.Single());
            view.AddChildren();

            // assert
            var exporter = new JekyllExporter(new JekyllExporterConfiguration(@"D:\Projects\AgileArchitect\Documentation\jekyll"));
            exporter.Export(view);
        }

//        [Fact]
//        public void TestExport()
//        {
//            var workspace = new Workspace();
//
//            var model = workspace.GetModel();
//            var user = model.AddPerson("Employee Pharmacy", "An employee in a pharmacy", Location.External);
//            var ncontrol = model.AddSystem("NControl", "Medication Related HealthCare System", Location.Internal);
//            var ncasso = model.AddSystem("NCasso", "Healhcare Insurer Declaration Portal", Location.External);
//
//            user.Uses(ncontrol, "uses");
//            ncontrol.Uses(ncasso, "uses");
//
//            var view = workspace.CreateContextView(ncontrol);
//
//            view.AddAllSystems();
//            view.AddAllPeople();
//
//            var exporter = new JekyllExporter(new JekyllExporterConfiguration(@"D:\Projects\AgileArchitect\Documentation\jekyll"));
//            exporter.Export(view, new SvgExporter());
//        }
    }
}
