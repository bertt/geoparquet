using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoParquet
{
    public partial class Covering
    {
        [JsonPropertyName("bbox")]
        [Required]
        public Bbox Bbox { get; set; } = new Bbox();
    }
}