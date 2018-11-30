using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessRulesEngineConsoleApp.Database;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IReportService
    {
        void CreateReport(List<int> ruleValidationIds);
    }

    public class ReportService : IReportService
    {
        private readonly string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");
        private List<RuleValidationDetail> _validationDetails;
        private List<string> _emails;

        public void CreateReport(List<int> ruleValidationIds)
        {
            // var props = invalidRecords.First().GetType().GetProperties();

            var sb = new StringBuilder();
            SetAllValidationDetails();
            SetAllEmails();

            var validationDetailsToEmail = new List<RuleValidationDetail>();

            foreach(var ruleValidation in ruleValidationIds)
                validationDetailsToEmail.AddRange(GetDetailsFromRuleValidationId(ruleValidation));

            // email validationDetailsToEmail

            File.WriteAllText(@"C:\temp\InvalidRecords.csv", sb.ToString());
        }

        private void SetAllValidationDetails()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                _validationDetails = odsRawDbContext.RuleValidationDetails.ToList();
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
    }
}