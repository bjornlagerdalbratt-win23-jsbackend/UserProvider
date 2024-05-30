using System.Diagnostics.Contracts;

namespace UserProvider.Models;

public class DeleteUserRequest
{
    public string Email { get; set; } = null!;
}
