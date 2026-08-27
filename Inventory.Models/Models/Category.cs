using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconCss { get; set; } = "bi-box-seam";

        public ICollection<Product>? Products { get; set; }
    }
}
