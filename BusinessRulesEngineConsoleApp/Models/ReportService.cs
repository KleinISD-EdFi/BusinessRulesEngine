using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessRulesEngineConsoleApp.Database;
using Engine.Models;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IReportService
    {
        void CreateReport(List<int> ruleValidationIds, List<Collection> collections);
    }

    public class ReportService : IReportService
    {
        private readonly string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");
        private List<RuleValidationDetail> _validationDetails;
        private List<RuleValidation> _ruleValidations;
        private List<RuleValidationRuleComponent> _ruleComponents;
        // maybe move email list to somewhere else.
        private List<string> _emails;
        private IEmailService _emailService;

        private readonly List<string> _columns = new List<string>()
        {
            "Collection",
            "Id",
            "Rule",
            "Message"
        };

        public void CreateReport(List<int> ruleValidationIds, List<Collection> collections)
        {
            var sb = new StringBuilder();
            SetAllValidationDetails();
            SetAllRuleValidations();
            SetAllRuleComponents();
            SetAllEmails();

            var validationDetailsToEmail = new List<RuleValidationDetail>();

            foreach(var ruleValidation in ruleValidationIds)
                validationDetailsToEmail.AddRange(GetDetailsFromRuleValidationId(ruleValidation));

            // email validationDetailsToEmail
            foreach (var column in _columns)
                sb.Append(column + ",");

            sb.AppendLine();
            foreach (var ruleValidation in validationDetailsToEmail)
            {
                var reportData = new ReportData
                {
                    Collection = GetCollectionNameFromRuleValidationId(ruleValidation.RuleValidationId),
                    Rule = GetRuleNameFromRuleComponentId(ruleValidation.RuleId),
                    Id =  ruleValidation.Id,
                    Message = ruleValidation.Message
                };

                sb.AppendLine(reportData.CsvString);
            }

            File.WriteAllText(@"C:\temp\InvalidRecords.csv", sb.ToString());
        }

        // Validation Results
        private void SetAllValidationDetails()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                _validationDetails = odsRawDbContext.RuleValidationDetails.ToList();
            }
        }

        // Collections
        private void SetAllRuleValidations()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                _ruleValidations = odsRawDbContext.RuleValidations.ToList();
            }
        }

        // Rules
        private void SetAllRuleComponents()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                _ruleComponents = odsRawDbContext.RuleValidationRuleComponents.ToList();
            }
        }

        private void SetAllEmails()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                var records = odsRawDbContext.RuleValidationRecipients.ToList();
                records.ForEach(record => _emails.Add(record.EmailAddress));
            }
        }

        private List<RuleValidationDetail> GetDetailsFromRuleValidationId(int ruleValidationId)
        {
            return _validationDetails.Where(record => record.RuleValidationId == ruleValidationId).ToList();
        }

        private string GetCollectionNameFromRuleValidationId(long ruleValidationId)
        {
            return _ruleValidations.First(record => record.RuleValidationId == ruleValidationId).CollectionId;
        }

        private string GetRuleNameFromRuleComponentId(string ruleId)
        {
            return _ruleComponents.First(record => record.RuleId == ruleId).RulesetId;
        }
    }
}