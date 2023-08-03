using ParquetSharp;
using System.CommandLine;
using GeoParquet;
using System.Text.Json;

namespace geoparquet_tools;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var version = typeof(Program).Assembly.GetName().Version;
        Console.WriteLine($"geoparquet-tools {version}");
        if (args.Length == 0)
        {
            args = new string[] { "-h" };
        }

        var fileOption = new Option<FileInfo?>(
            name: "-i",
            description: "The file to read and display on the console.");

        var rootCommand = new RootCommand("qm-tools - quantized mesh tools");
        rootCommand.AddOption(fileOption);

        rootCommand.SetHandler((file) =>
        {
            ReadFile(file!);
        },
            fileOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void ReadFile(FileInfo file)
    {
        if (file != null)
        {
            if (File.Exists(file.FullName)){
                var gpq = new ParquetFileReader(file.FullName);
                var geoParquet = gpq.GetGeoMetadataAsString();

                if (geoParquet != null)
                {
                    var jDoc= JsonDocument.Parse(geoParquet);
                    var formatted = JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
                    Console.Write("GeoMetaData: " + formatted);
                }
                else
                {
                    Console.WriteLine("No geo metadata found");
                }
            }
            else
            {
                Console.WriteLine("File not found: " + file.FullName);
            }
        }
    }
}
