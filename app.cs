#:property PublishAot=false
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

foreach (var f in geo.Features)
{
    f.Properties.UmapOptions = UmapOptions.Default;
    f.Properties.Name = Guid.NewGuid().ToString();
    f.Properties.Description = "This is a description {{https://upload.wikimedia.org/wikipedia/commons/b/b6/Image_created_with_a_mobile_phone.png}}";
}

using var outputFs = File.Create("output.geojson");
await JsonSerializer.SerializeAsync(outputFs, geo, serializerOptions);

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
    public UmapOptions? UmapOptions { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}


class UmapOptions
{
    public string Color { get; set; }
    public string PopupShape { get; set; }
    public string IconClass { get; set; }
    public string PopupTemplate { get; set; }

    public static UmapOptions Default { get; } = new UmapOptions
    {
        Color = "blue",
        PopupShape = "Panel",
        IconClass = "Drop",
        PopupTemplate = "OSM"
    };
}