using System;
using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        public decimal Longitude { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        [MaxLength(1)]
        public string Active { get; set; } = "Y";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
