using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GeoParquet
{
    public partial class GeoParquet
    {

        [JsonPropertyName("version")]
        [Required(AllowEmptyStrings = true)]
        public string Version { get; set; }


        [JsonPropertyName("primary_column")]
        [Required]
        public string Primary_column { get; set; }


        [JsonPropertyName("columns")]
        [Required]
        public IDictionary<string, GeoColumn> Columns { get; set; } = new Dictionary<string, GeoColumn>();

        private IDictionary<string, object> _additionalProperties;

        [JsonPropertyName("additionalProperties")]
        public bool AdditionalProperties { get; set; }
    }
}