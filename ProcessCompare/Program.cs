﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;

namespace ProcessCompare
{
    class Program
    {
        private static Settings _settings;
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("ProcessCompare.log")
                .MinimumLevel.Verbose()
                .CreateLogger();

            Log.Information("Starting ProcessCompare {args}", args);

            _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "settings.json")));
            var comparer = new Comparer(_settings);
            comparer.DoCompare();

            var migrationScriptGenerator = new MigrationScriptGenerator(_settings);
            migrationScriptGenerator.GenerateMigrationScripts();

            Log.Information("Done.  Press any key to close.");
            Console.ReadLine();
        }

    }
}
