namespace CodeModelFromDB
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AWModel : DbContext
    {
        public AWModel()
            : base("name=AWModel")
        {
        }

        public virtual DbSet<Expense> Expenses { get; set; }
        public virtual DbSet<ExpenseGroup> ExpenseGroups { get; set; }
        public virtual DbSet<ExpenseGroupStatu> ExpenseGroupStatus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 0);

            modelBuilder.Entity<ExpenseGroup>()
                .HasMany(e => e.Expenses)
                .WithRequired(e => e.ExpenseGroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ExpenseGroupStatu>()
                .HasMany(e => e.ExpenseGroups)
                .WithRequired(e => e.ExpenseGroupStatu)
                .HasForeignKey(e => e.ExpenseGroupStatusId)
                .WillCascadeOnDelete(false);
        }
    }
}
