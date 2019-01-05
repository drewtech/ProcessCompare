using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Serilog;

namespace ProcessCompare
{
    internal class MigrationScriptGenerator
    {
        private Settings _settings;

        public MigrationScriptGenerator(Settings settings)
        {
            _settings = settings;
        }


        public void GenerateMigrationScripts()
        {
            Log.Information("Generating migration scripts from csv export.");

            Log.Warning("Deleting any existing migration scripts...");

            DeleteExistingMigrationScripts();

            Log.Warning("Migration scripts deleted.");

            foreach (var file in GetFileList())
            {
                GenerateCreateTableScript(file);
                GeneratePopulateDataScript(file);
            }

            Log.Information("Completed generating migration scripts from csv export.");
        }

        private void GeneratePopulateDataScript(FileInfo file)
        {
            //throw new System.NotImplementedException();
        }

        private void GenerateCreateTableScript(FileInfo file)
        {
            var sb = new StringBuilder();
            var tableName = Path.GetFileNameWithoutExtension(file.Name);

            var columnSizeMap = MapColumnSize(file.FullName);

            sb.AppendLine($"CREATE TABLE {tableName}( ");
            var tableKeys = columnSizeMap.Keys.Where(x => x.EndsWith("_k"));

            foreach (var kvp in columnSizeMap)
            {
                sb.AppendLine($"[{kvp.Key}] varchar({kvp.Value}),");
            }
            sb.AppendLine($"PRIMARY KEY ({string.Join(',', tableKeys)})");
            sb.AppendLine(");");

            File.AppendAllText(Path.Combine(_settings.MigrationScriptLocation, $"{tableName}.sql"), sb.ToString());
        }

        private Dictionary<string, int> MapColumnSize(string fullName)
        {
            var firstRow = true;
            var columnSizeMap = new Dictionary<string, int>();
            using (var reader = new StreamReader(fullName))
            {
                using (var csv = new CsvReader(reader))
                {
                    while (csv.Read())
                    {
                        if (firstRow)
                        {
                            csv.ReadHeader();
                            firstRow = false;
                            continue;
                        }

                        foreach (var header in csv.Context.HeaderRecord)
                        {
                            if (!columnSizeMap.ContainsKey(header))
                                columnSizeMap.Add(header, csv.GetField<string>(header).Length);
                            else
                            {
                                if (csv.GetField<string>(header).Length > columnSizeMap[header])
                                    columnSizeMap[header] = csv.GetField<string>(header).Length;
                            }
                            
                        }
                    }
                }
            }

            return columnSizeMap;
        }

        private IEnumerable<string> GetNormalRows(string[] headerRow)
        {
            return headerRow.Where(x => !x.EndsWith("_k"));
        }

        private void DeleteExistingMigrationScripts()
        {
            var di = new DirectoryInfo(_settings.MigrationScriptLocation);
            foreach (FileInfo file in di.GetFiles("*.sql"))
            {
                file.Delete();
            }
        }

        private IList<FileInfo> GetFileList()
        {
            var d = new DirectoryInfo(_settings.CompareExportFolder);
            return d.GetFiles("*.csv");
        }
    }
}