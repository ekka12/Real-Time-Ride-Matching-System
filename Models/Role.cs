using System;
using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1)]
        public string Active { get; set; } = "Y";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
