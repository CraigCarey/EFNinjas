using System.Data.Entity;
using NinjaDomain.Classes;

namespace NinjaDomain.DataModel
{
    public class NinjaContext : DbContext
    {
        // When we define a DbContext we need to tell it what DbSets are in the model
        // EF will use this DbContext class (and the schema of the Classes it uses) to infer a db model
        public DbSet<Ninja> Ninjas { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<NinjaEquipment> Equipment { get; set; }
    }
}
