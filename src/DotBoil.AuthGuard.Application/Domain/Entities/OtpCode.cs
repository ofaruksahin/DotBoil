using DotBoil.Entities;

namespace DotBoil.AuthGuard.Application.Domain.Entities;

public class OtpCode : BaseEntity
{
    public int UserId { get; set; }
    public string Code { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsExpired { get; set; }
    
    public virtual User User { get; set; }

    public void GenerateOtpCode()
    {
        var random = new Random();
        Code = random.Next(100000, 1000000).ToString();
    }
}