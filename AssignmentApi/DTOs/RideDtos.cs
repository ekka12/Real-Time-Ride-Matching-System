using System.ComponentModel.DataAnnotations;

namespace AssignmentApi.DTOs
{
    public class AddLocationRequest
    {
        [Required]
        [StringLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [RegularExpression("^(Y|N)$")]
        public string Active { get; set; } = "Y";
    }

    public class AddRoleRequest
    {
        [Required]
        [StringLength(20)]
        public string RoleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [RegularExpression("^(Y|N)$")]
        public string Active { get; set; } = "Y";
    }

    public class UpdateDriverStatusRequest
    {
        [Required]
        [RegularExpression("^(Y|N)$")]
        public string UserActiveOnline { get; set; } = "N";
    }

    public class UpdateUserLocationRequest
    {
        [Required]
        public int LocationId { get; set; }
    }

    public class NearbyDriversRequest
    {
        public int? RiderId { get; set; }
        public int? PickupLocationId { get; set; }
    }

    public class ValidateRideRequestDto
    {
        [Required]
        public int PickupLocationId { get; set; }

        [Required]
        public int DropLocationId { get; set; }
    }
}
