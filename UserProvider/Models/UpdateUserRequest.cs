namespace UserProvider.Models;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = null!; 
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Bio {  get; set; }
}

