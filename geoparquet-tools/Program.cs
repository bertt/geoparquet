using ParquetSharp;
using System.CommandLine;
using GeoParquet;

namespace geoparquet_tools;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("geoparquet-tools");
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
            var gpq = new ParquetFileReader(file.FullName);
            var geoParquet = gpq.GetGeoMetadataAsString();

            if (geoParquet != null)
            {
                Console.Write("GeoMetaData: " + geoParquet);
            }
            else
            {
                Console.WriteLine("No geo metadata found");
            }
        }
    }
}
