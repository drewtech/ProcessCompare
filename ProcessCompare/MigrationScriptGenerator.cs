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

            sb.AppendLine($"CREATE TABLE {tableName}( ");

            using (var reader = new StreamReader(file.FullName))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Read();
                    csv.ReadHeader();
                    string[] headerRow = csv.Context.HeaderRecord;
                    var keys = GetKeys(headerRow);
                    foreach (var row in headerRow)
                    {
                        //TODO don't use varchar(max) for everything
                        sb.AppendLine($"[{row}] varchar(max),");
                    }

                    sb.AppendLine($"PRIMARY KEY ({string.Join(',', keys)})");
                    sb.AppendLine(");");
                }
            }

            File.AppendAllText(Path.Combine(_settings.MigrationScriptLocation, $"{tableName}.sql"), sb.ToString());
        }

        private IEnumerable<string> GetNormalRows(string[] headerRow)
        {
            return headerRow.Where(x => !x.EndsWith("_k"));
        }

        private IEnumerable<string> GetKeys(string[] headerRow)
        {
            return headerRow.Where(x => x.EndsWith("_k"));
        }

        private IList<FileInfo> GetFileList()
        {
            var d = new DirectoryInfo(_settings.CompareExportFolder);
            return d.GetFiles("*.csv");
        }
    }
}