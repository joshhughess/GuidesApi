using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using GuidesApi.Data.Models;
using GuidesApi.Extensions;

namespace GuidesApi.Data
{
    public class AppDbContext : AuditDbContext
    {
        private readonly IHttpContextAccessor _accessor;
        public AppDbContext(DbContextOptions options, IHttpContextAccessor accessor) : base(options) => _accessor = accessor;

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            SoftDelete();
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SoftDelete();
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SoftDelete()
        {
            var deletedEntries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Deleted && !e.Entity.ForceDelete)
                .ToList();

            foreach (var entry in deletedEntries)
            {
                entry.Entity.IsDeleted = true;
                entry.State = EntityState.Modified;
            }
        }

        private void UpdateAuditFields()
        {
            var currentUserId = _accessor?.HttpContext?.User?.GetUserId();
            var currentUser = _accessor?.HttpContext?.User?.Identity?.Name;

            var modifiedEntries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);


            foreach (var entry in modifiedEntries.Where(x => !x.Entity.OverrideAudit).ToList())
            {
                entry.Entity.UpdatedById = currentUserId;
                entry.Entity.UpdatedBy = currentUser;
                entry.Entity.UpdatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedById = currentUserId;
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
