namespace Apartments.Application.Dtos.AdminDtos;

public class AssignRoleDto
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = default!;
}