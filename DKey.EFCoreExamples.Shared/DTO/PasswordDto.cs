namespace DKey.EFCoreExamples.Shared.DTO;

public class PasswordDto
{
    public LoginMethod Provider { get; set; }
    public string? PasswordHashOrKey { get; set; }
}