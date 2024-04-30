using Microsoft.EntityFrameworkCore;

namespace APICoches.Infraestructure
{
    public class AppDbContextSeed
    {
        public static void Init(IServiceProvider provider)
        {
            using (var dbContext = provider.GetRequiredService<AppDbContext>())
            {
                dbContext.Database.EnsureCreated();
                dbContext.SaveChanges();
            }
        }
    }
}
