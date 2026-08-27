using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Models.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 100000)]
        public int QuantityOnHand { get; set; }

        [Required]
        [Range(1, 10000)]
        public int MinStockLevel { get; set; }

        [StringLength(50)]
        public string AisleLocation { get; set; } = "Aisle A-1, Bin 01";

        public DateTime? ExpiryDate { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; } = "/images/products/default.jpg";

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }

        [ValidateNever]
        public int? SupplierID { get; set; }

        [ForeignKey("SupplierID")]
        [ValidateNever]
        public Supplier? Supplier { get; set; }

        [NotMapped]
        public bool IsLowStock => QuantityOnHand <= MinStockLevel;
    }
}
