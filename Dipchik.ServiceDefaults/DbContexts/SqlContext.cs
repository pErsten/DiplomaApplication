using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.Hosting.DbContexts;

public class SqlContext : DbContext
{
    public SqlContext(DbContextOptions<SqlContext> options)
        : base(options)
    {
        
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Language> Languages { get; set; }


    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Account>().Property(x => x.Login).HasMaxLength(100);
        mb.Entity<Account>().Property(x => x.AccountId).HasMaxLength(50);
        mb.Entity<Account>().HasIndex(x => x.Login).IsUnique();
        mb.Entity<Account>().HasIndex(x => x.AccountId).IsUnique();

        base.OnModelCreating(mb);
    }
}