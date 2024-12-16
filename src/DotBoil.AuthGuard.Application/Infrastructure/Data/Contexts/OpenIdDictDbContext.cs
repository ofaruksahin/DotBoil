using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;

public class OpenIdDictDbContext : DbContext
{
    public OpenIdDictDbContext(DbContextOptions<OpenIdDictDbContext> options) : base(options)
    {
        
    }
}