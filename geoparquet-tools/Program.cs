using ParquetSharp;
using System.CommandLine;
using GeoParquet;
using System.Text.Json;
using System.Reflection;

namespace geoparquet_tools;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"geoparquet-tools");

        var assembly = Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version;

        Console.WriteLine($"Version: " + assemblyVersion);

        if (args.Length == 0)
        {
            args = new string[] { "-h" };
        }

        var fileOption = new Option<FileInfo?>(
            name: "-i",
            description: "The file to read and display on the console.");

        var rootCommand = new RootCommand("geoparquet-tools");
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
                    // Format the JSON for better readability
                    var jDoc = JsonDocument.Parse(geoParquet);
                    using var stream = new MemoryStream();
                    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                    jDoc.WriteTo(writer);
                    writer.Flush();
                    var formatted = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                    Console.WriteLine("GeoMetaData: " + formatted);
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
