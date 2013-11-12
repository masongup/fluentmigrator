using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace FluentMigrator.T4.GenerateMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            //pathArg should be set to the path of the project file the migration should be added to, relative
            //to the current working directory the app is executing in
            string pathArg = args.SingleOrDefault(arg => arg.StartsWith("-project="));
            if (pathArg != null)
                pathArg = pathArg.Replace("-project=", String.Empty);
            if (pathArg == null)
                pathArg = ConfigurationManager.AppSettings["ProjectPath"];
            if (pathArg == null)
                throw new Exception("Unable to determine the path to the project! Please specify in app.config or commmand line param.");

            string projectPath = Path.Combine(Directory.GetCurrentDirectory(), pathArg);
            
            var migCode = new InitialMigrationCode { IgnoreInfo = GetOldTables() };
            string result = migCode.TransformText();
            string timestampString = migCode.GetCurrentTimeStamp();

            string fileName = "MigrationCode-" + timestampString + ".cs";
            using (var tw = new StreamWriter(Path.Combine(Path.GetDirectoryName(projectPath), fileName)))
            {
                tw.Write(result);
                tw.Close();
            }

            AddFileToProject(projectPath, fileName);
        }

        private static void AddFileToProject(string projectPath, string fileName)
        {
            var projectFile = XElement.Load(projectPath);
            XNamespace projNameSpace = projectFile.Name.Namespace;
            var compileGroupElements = projectFile.Elements(projNameSpace.GetName("ItemGroup"));
            var compileGroupElement = compileGroupElements.Single(ig => ig.Elements(projNameSpace.GetName("Compile")).Any());
            compileGroupElement.Add(new XElement(projNameSpace.GetName("Compile"), new XAttribute("Include", fileName)));
            projectFile.Save(projectPath);
        }

        private static Tables GetOldTables()
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                var connSection = ConfigurationManager.ConnectionStrings;
                var connectionSettings = connSection["BeforeConnection"];
                var generator = new CodeGenerator(connectionSettings.ConnectionString, connectionSettings.ProviderName, sw, null);
                var tables = generator.LoadTables();
                sw.Close();
                return tables;
            }
        }
    }
}
