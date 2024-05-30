using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; } = "avatar.png";
    public string? UserAddressId { get; set; }
    public UserAddress? Address { get; set; }
}
