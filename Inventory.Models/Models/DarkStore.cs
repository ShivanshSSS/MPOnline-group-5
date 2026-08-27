using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.Models
{
    public class DarkStore
    {
        [Key]
        public int DarkStoreId { get; set; }

        [Required]
        [StringLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
