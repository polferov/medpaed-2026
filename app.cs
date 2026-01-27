#:property PublishAot=false
#:package CsvHelper@33.1.0
#nullable disable
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

var serializerOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

using var fs = File.OpenRead("input.geojson");
var geo = await JsonSerializer.DeserializeAsync<GeoJsonRoot>(fs, serializerOptions);

// read corallation-filled-out.csv as Corallation[] using CsvHelper
using var csvFs = File.OpenRead("corallation-filled-out.csv");
using var reader = new StreamReader(csvFs);
using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
var corallations = csv.GetRecords<Corallation>().ToArray();

foreach (var c in corallations)
{
    c.Category = c.Category.Trim();
}



if (corallations.Length != geo.Features.Length)
{
    throw new Exception("Corallation count does not match feature count");
}

var categories = new Dictionary<string, string>
{
    ["Besonderheiten"] = "red",
    ["Ende"] = "blue",
    ["Fabrikgebäude"] = "black",
    ["Gesundheitseinrichtungen"] = "orange",
    ["Stadt im Umbruch"] = "blue",
    ["Infrastruktur"] = "purple",
    ["Straßennamen"] = "turquoise",
    ["Verkehrswege"] = "grey",
    ["Bebauung"] = "pink",
    ["Gewerbe"] = "green",
};
foreach (var c in corallations.Select(c => c.Category).Distinct().Except(categories.Keys).Order().ToList())
{
    Console.WriteLine($"Unhadled Category: '{c}'");
}

foreach (var entry in geo.Features.Zip(corallations))
{
    var f = entry.First;
    var c = entry.Second;
    f.Properties.UmapOptions = UmapOptions.Default;
    f.Properties.Name = c.Name;
    if (!string.IsNullOrWhiteSpace(c.Category))
        f.Properties.UmapOptions.Color = categories.GetValueOrDefault(c.Category, "black");
    f.Properties.Description =
    $"""
    {c.Description}

    Category: {c.Category}
    
    """;
    foreach (var img in c.ImageId.Split(' '))
    {
        if (!File.Exists($"img/{img}"))
        {
            Console.WriteLine($"Image not found: {img}");
            continue;
        }

        var url = $"https://raw.githubusercontent.com/polferov/medpaed-2026/refs/heads/main/img/{img}";
        f.Properties.Description +=
        $$$"""
        
        {{{{{url}}}}}
        """;
    }

}


using var outputFs = File.Create("output.geojson");
await JsonSerializer.SerializeAsync(outputFs, geo, serializerOptions);

[Obsolete("Was used for setup only")]
void CreateCsv(GeoJsonRoot geo)
{
    using var csv = File.OpenWrite("corallation.csv");
    using var writer = new StreamWriter(csv);
    writer.WriteLine("Id,Lazy Title,Name,Description,Category, ImageId");
    foreach (var f in geo.Features)
    {
        writer.WriteLine(
            $"""
            "{f.Properties.Id}","{f.Properties.Text}","",""
            """
        );
    }
}
_ = nameof(CreateCsv); // to avoid unused warning

class GeoJsonRoot
{
    public string Type { get; set; }
    public GeoJsonFeature[] Features { get; set; }
}

class GeoJsonFeature
{
    public string Type { get; set; }
    public JsonNode Geometry { get; set; }
    public GeoJsonProperties Properties { get; set; }
}

class GeoJsonProperties
{
    public int Id { get; set; }
    public int ButtonId { get; set; }
    public string ButtonLabel { get; set; }
    public int Count { get; set; }
    public string Text { get; set; }
    public DateTime IsoDate { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    [JsonPropertyName("_umap_options")]
    public UmapOptions UmapOptions { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}


class UmapOptions
{
    public string Color { get; set; }
    public string PopupShape { get; set; }
    public string IconClass { get; set; }
    public string PopupTemplate { get; set; }

    public static UmapOptions Default => new UmapOptions
    {
        Color = "turquoise",
        PopupShape = "Panel", // Large or Panel
        IconClass = "Circle"
    };
}

class Corallation
{
    public int Id { get; set; }
    public string LazyTitle { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string ImageId { get; set; }
}