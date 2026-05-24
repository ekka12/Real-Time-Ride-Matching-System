using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssignmentApi.Models
{
    public class RideRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RiderId { get; set; }

        [ForeignKey(nameof(RiderId))]
        public User? Rider { get; set; }

        public int? DriverId { get; set; }

        [ForeignKey(nameof(DriverId))]
        public User? Driver { get; set; }

        [Required]
        public double PickupLatitude { get; set; }

        [Required]
        public double PickupLongitude { get; set; }

        [Required]
        public double DropoffLatitude { get; set; }

        [Required]
        public double DropoffLongitude { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Matched", "Accepted", "Completed", "Cancelled"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
