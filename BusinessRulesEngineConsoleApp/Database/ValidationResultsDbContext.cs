using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessRulesEngineConsoleApp.Models;
using System.Data.Entity;

namespace BusinessRulesEngineConsoleApp.Database
{
    class ValidationResultsDbContext : DbContext
    {
        public ValidationResultsDbContext() : base(
            ConfigurationManager.ConnectionStrings["ValidationResults"]?.ToString())
        {
            System.Data.Entity.Database.SetInitializer<ValidationResultsDbContext>(null);
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