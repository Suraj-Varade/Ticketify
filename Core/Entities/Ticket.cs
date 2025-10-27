using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core.Entities
{
    public class Ticket : BaseEntity
    {
        [Required, MaxLength(200)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("createdBy")]
        public int CreatedBy { get; set; }
        
        [JsonPropertyName("assignTo")]
        public int? AssignTo { get; set; }

        [Required, MaxLength(50)]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Open";

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("modifiedAt")]
        public DateTime? ModifiedAt { get; set; }
    }
}