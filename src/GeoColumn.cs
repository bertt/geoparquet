using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoParquet
{
    public partial class GeoColumn
    {

        [JsonPropertyName("encoding")]
        [Required(AllowEmptyStrings = true)]
        [RegularExpression(@"^(WKB|point|linestring|polygon|multipoint|multilinestring|multipolygon)$")]
        public string Encoding { get; set; }


        [JsonPropertyName("geometry_types")]
        [Required]
        public ICollection<string> Geometry_types { get; set; } = new System.Collections.ObjectModel.Collection<string>();

        [JsonPropertyName("edges")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Edges Edges { get; set; }


        [JsonPropertyName("orientation")]
        public string Orientation { get; set; }


        [JsonPropertyName("bbox")]
        public double[] Bbox { get; set; }


        [JsonPropertyName("epoch")]
        public double Epoch { get; set; }


        [JsonPropertyName("covering")]
        public Covering Covering { get; set; }


        [JsonPropertyName("additionalProperties")]
        public bool AdditionalProperties { get; set; }
    }
}


