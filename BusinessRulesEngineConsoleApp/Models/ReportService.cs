using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
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
        void CreateAndEmailReport(List<int> ruleValidationIds, List<Collection> collections);
        void SetFourDigitOdsYear(string fourDigitOdsYear);
    }

    public class ReportService : IReportService
    {
        private string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");
        private List<RuleValidationDetail> ValidationDetails { get; set; }
        private List<RuleValidation> RuleValidations { get; set; }
        private List<RuleValidationRuleComponent> RuleComponents { get; set; }
        private readonly List<string> _emailRecipients = new List<string>();
        private IEmailService _emailService;
        public readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private readonly List<string> _columns = new List<string>()
        {
            "Collection",
            "Id",
            "Rule",
            "Message"
        };

        public void SetFourDigitOdsYear(string fourDigitOdsYear)
        {
            if (fourDigitOdsYear != null)
                _fourDigitOdsDbYear = fourDigitOdsYear;
        }

        public void CreateAndEmailReport(List<int> ruleValidationIds, List<Collection> collections)
        {
            var sb = new StringBuilder();
            
            InitialiseProperties();

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

            var csvName = SaveCsvReportToDisk(sb.ToString());

            EmailReport(csvName);

            Log.Info("CLEARING rules tables.");
            ClearEngineTables();
        }

        private void EmailReport(string csvName)
        {
            _emailService.SendReportEmail(_emailRecipients, csvName, CreateEmailBody());
        }

        private string CreateEmailBody()
        {
            var errorCount = ValidationDetails.Count;
            var collectionList = new List<string>();

            foreach (var ruleValidation in RuleValidations)
            {
                collectionList.Add(ruleValidation.CollectionId);
            }

            return $@"<p><strong>Report Summary</strong></p>
                      <p>-------------------------------------------------------------</p>
                      <p><br />Total number of errors : <strong>{errorCount}</strong></p>
                      <p>Collections ran : <strong>{string.Join(",", collectionList)}</strong></p>";
        }

        private void ClearEngineTables()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                foreach (var ruleValidation in RuleValidations)
                {
                    odsRawDbContext.RuleValidations.Remove(odsRawDbContext.RuleValidations.First(r => r.RuleValidationId == ruleValidation.RuleValidationId));
                    odsRawDbContext.SaveChanges();
                }
            }

            Log.Info("CLEARED rules tables.");
        }

        private void InitialiseProperties()
        {
            SetAllValidationDetails();
            SetAllRuleValidations();
            SetAllRuleComponents();
            SetAllEmailRecipients();

            _emailService = new EmailService();
        }

        /// <param name="csvText">Text to save into a csv file.</param>
        /// <returns>Name of csv file.</returns>
        private string SaveCsvReportToDisk(string csvText)
        {
            string csvName = $"Rules Engine Report {DateTime.Now:MM-dd-yyyy}.csv";
            var directoryToSave = $"{ConfigurationManager.AppSettings["ReportDirectory"]}\\{csvName}";
            File.WriteAllText(directoryToSave, csvText);

            return csvName;
        }

        // Validation Results
        private void SetAllValidationDetails()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                ValidationDetails = odsRawDbContext.RuleValidationDetails.ToList();
            }
        }

        // Collections
        private void SetAllRuleValidations()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                RuleValidations = odsRawDbContext.RuleValidations.ToList();
            }
        }

        // Rules
        private void SetAllRuleComponents()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                RuleComponents = odsRawDbContext.RuleValidationRuleComponents.ToList();
            }
        }

        private void SetAllEmailRecipients()
        {
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                var records = odsRawDbContext.RuleValidationRecipients.ToList();
                records.ForEach(record => _emailRecipients.Add(record.EmailAddress));
            }
        }

        private List<RuleValidationDetail> GetDetailsFromRuleValidationId(int ruleValidationId)
        {
            return ValidationDetails.Where(record => record.RuleValidationId == ruleValidationId).ToList();
        }

        private string GetCollectionNameFromRuleValidationId(long ruleValidationId)
        {
            return RuleValidations.First(record => record.RuleValidationId == ruleValidationId).CollectionId;
        }

        private string GetRuleNameFromRuleComponentId(string ruleId)
        {
            return RuleComponents.First(record => record.RuleId == ruleId).RulesetId;
        }
    }
}