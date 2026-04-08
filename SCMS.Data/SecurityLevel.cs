// File: /SecurityLevel.cs
using System.ComponentModel.DataAnnotations;

namespace SCMS.Data
{
    public class SecurityLevel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = "";

        [MaxLength(256)]
        public string? Description { get; set; }

        // Set true for Administrator, User, Anonymous to prevent deletion or changes
        public bool IsSystem { get; set; }
    }
}
