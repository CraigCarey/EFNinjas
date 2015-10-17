using NinjaDomain.Classes;
using NinjaDomain.Classes.Interfaces;
using System;
using System.Data.Entity;
using System.Linq;

namespace NinjaDomain.DataModel
{
    public class NinjaContext : DbContext
    {
        // When we define a DbContext we need to tell it what DbSets are in the model
        // EF will use this DbContext class (and the schema of the Classes it uses) to infer a db model
        public DbSet<Ninja> Ninjas { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<NinjaEquipment> Equipment { get; set; }

        // called when the model for a derived context has been initialized, but before the model has been locked down and used to initialize the context
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // don't persist the IsDirty property in the db
            modelBuilder.Types().Configure(c => c.Ignore("IsDirty"));
            base.OnModelCreating(modelBuilder);
        }

        // force EF to update logging fields each time SaveChanges is called
        public override int SaveChanges()
        {
            // logic before standard SaveChanges stuff is executed

            // for everything that implements the IModificationHistory and is known to be .Added or .Modified
            foreach (var history in this.ChangeTracker.Entries()
              .Where(e => e.Entity is IModificationHistory
                  && (e.State == EntityState.Added
                    || e.State == EntityState.Modified))
              .Select(e => e.Entity as IModificationHistory))
            {
                history.DateModified = DateTime.Now;

                // minValue means DateCreated hasn't been set, since DateTimes can't be null

                if (history.DateCreated == DateTime.MinValue)
                {
                    history.DateCreated = DateTime.Now;
                }
            }

            // the internal call to SaveChanges
            int result = base.SaveChanges();

            // reset IsDirty flag when data has been persisted
            foreach (var history in this.ChangeTracker.Entries()
                                          .Where(e => e.Entity is IModificationHistory)
                                          .Select(e => e.Entity as IModificationHistory))
            {
                history.IsDirty = false;
            }
            return result;
        }
    }
}
