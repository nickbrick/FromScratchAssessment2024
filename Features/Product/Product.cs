using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FromScratchAssessment2024
{
    public class Product : Entity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        [Precision(14,2)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }
        [JsonIgnore]
        public ICollection<PurchaseItem>? PurchaseItems { get; set; }
    }

}
