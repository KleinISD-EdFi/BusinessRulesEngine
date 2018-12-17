using System.Data.Entity;
using BusinessRulesEngineConsoleApp.Models;

namespace BusinessRulesEngineConsoleApp.Database
{
    public class RawOdsDbContext : DbContext
    {
        public RawOdsDbContext(string fourDigitYear) : base(
            new OdsConfigurationValues().GetRawOdsConnectionString(fourDigitYear))
        {
            System.Data.Entity.Database.SetInitializer<RawOdsDbContext>(null);
        }

        public RawOdsDbContext()
        {
            // Fixes a known bug in which EntityFramework.SqlServer.dll is not copied into consumer even when CopyLocal is True.
            var includeSqlServerDLLInConsumer = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        public virtual DbSet<RuleValidation> RuleValidations { get; set; }
        public virtual DbSet<RuleValidationDetail> RuleValidationDetails { get; set; }
        public virtual DbSet<RuleValidationRuleComponent> RuleValidationRuleComponents { get; set; }
        public virtual DbSet<RuleValidationRecipients> RuleValidationRecipients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RuleValidationRecipients>();

            modelBuilder.Entity<RuleValidationDetail>()
                .HasRequired(dt => dt.RuleValidation)
                .WithMany(dt => dt.RuleValidationDetails)
                .HasForeignKey(dt => dt.RuleValidationId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RuleValidationRuleComponent>()
                .HasRequired(dt => dt.RuleValidation)
                .WithMany(dt => dt.RuleValidationRuleComponents)
                .HasForeignKey(dt => dt.RuleValidationId)
                .WillCascadeOnDelete(true);
        }
    }
}