using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssignmentApi.Models
{
    public class DriverLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DriverId { get; set; }

        [ForeignKey(nameof(DriverId))]
        public User? Driver { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
