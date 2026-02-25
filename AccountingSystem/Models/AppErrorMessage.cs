using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class AppErrorMessage
    {
        public int AppErrorMessageId { get; set; }

        [Required, StringLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Message { get; set; } = string.Empty;
    }
}
