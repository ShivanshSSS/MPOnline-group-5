using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.Models
{
    public class DebugEventLog
    {
        [Key]
        public int LogId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = "Info"; // OrderSimulated, StockRestocked, LowStockAlert, SystemEvent

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? DetailsJson { get; set; }
    }
}
