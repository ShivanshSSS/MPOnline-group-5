using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Inventory.Models.Models
{
    public class WarehouseOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending"; // Pending, Picking, Packed, Dispatched, Delivered

        public decimal TotalAmount { get; set; }

        public int DarkStoreId { get; set; } = 1;

        [ForeignKey("DarkStoreId")]
        [ValidateNever]
        public DarkStore? DarkStore { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
