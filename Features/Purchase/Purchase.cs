using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FromScratchAssessment2024
{
    public class Purchase : Entity
    {
        [Required]
        [ForeignKey("Customer")]
        public Guid CustomerId { get; set; }

        [Precision(14,2)]
        public decimal TotalAmount { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }

        public ICollection<PurchaseItem> PurchaseItems { get; set; }
        
    }

    [PrimaryKey(nameof(PurchaseId), nameof(ProductId))]
    public class PurchaseItem
    {
        [Required]
        [ForeignKey("Purchase")]
        public Guid PurchaseId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        [JsonIgnore]
        public Purchase? Purchase { get; set; }

        public Product? Product { get; set; }
    }
}
