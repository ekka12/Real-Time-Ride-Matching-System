using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssignmentApi.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int LocationId { get; set; }

        public string? SessionValue { get; set; }

        [Required]
        [MaxLength(1)]
        public string UserActiveOnline { get; set; } = "N";

        [Required]
        [MaxLength(150)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string UserPhoneNo { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }
    }
}
