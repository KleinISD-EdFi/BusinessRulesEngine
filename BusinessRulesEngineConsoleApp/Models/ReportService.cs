using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BusinessRulesEngineConsoleApp.Database;
using Engine.Models;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IReportService
    {
        void CreateAndEmailReport(List<int> ruleValidationIds, List<Collection> collections, string fourDigitOdsYear);
    }

    public class ReportService : IReportService
    {
        private string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");
        private readonly string _directory = ConfigurationManager.AppSettings["ReportDirectory"];
        private List<RuleValidationDetail> ValidationDetails { get; set; }
        private List<RuleValidation> RuleValidations { get; set; }
        private List<RuleValidationRuleComponent> RuleComponents { get; set; }
        private readonly List<string> _emailRecipients = new List<string>();
        private IEmailService _emailService;
        private string _odsName;
        public readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private readonly List<string> _columns = new List<string>()
        {
            "Collection",
            "Id",
            "Rule",
            "Message"
        };

        public void CreateAndEmailReport(List<int> ruleValidationIds, List<Collection> collections, string fourDigitOdsYear)
        {
            var sb = new StringBuilder();

            InitialiseProperties(fourDigitOdsYear);

            var validationDetailsToEmail = new List<RuleValidationDetail>();

            foreach(var ruleValidation in ruleValidationIds)
                validationDetailsToEmail.AddRange(GetDetailsFromRuleValidationId(ruleValidation));

            foreach (var column in _columns)
                sb.Append(column + ",");

            sb.AppendLine();
            foreach (var ruleValidation in validationDetailsToEmail)
            {
                var reportData = new ReportData
                {
                    Collection = GetCollectionNameFromRuleValidationId(ruleValidation.RuleValidationId),
                    Rule = GetRuleNameFromRuleComponentId(ruleValidation.RuleId),
                    Id = ruleValidation.Id,
                    Message = ruleValidation.Message
                };

                sb.AppendLine(reportData.CsvString);
            }

            var csvName = SaveCsvReportToDisk(sb.ToString());
            ClearEngineTables();

            // Check to see if there are any errors in the .csv
            // to decide on which email to send
            EmailReport(csvName, validationDetailsToEmail.Count == 0);

        }

        private void InitialiseProperties(string fourDigitOdsYear)
        {
            SetAllValidationDetails();
            SetAllRuleValidations();
            SetAllRuleComponents();
            SetAllEmailRecipients();

            if (fourDigitOdsYear != null)
                _fourDigitOdsDbYear = fourDigitOdsYear;

            _odsName = $"EdFi_Ods_{_fourDigitOdsDbYear}";
            _emailService = new EmailService();
        }

        /// <param name="csvText">Text to save into a csv file.</param>
        /// <returns>Name of csv file.</returns>
        private string SaveCsvReportToDisk(string csvText)
        {
            string csvName = $"{_odsName} - {DateTime.Now:MM-dd-yyyy} Rules Engine Report.csv";

            Log.Info($"Saving report to disk at {_directory}\\{csvName}");
            CheckDirectory();

            var directoryToSave = $"{_directory}\\{csvName}";
            File.WriteAllText(directoryToSave, csvText);

            return csvName;
        }

        private void CheckDirectory()
        {
            if (!Directory.Exists(_directory))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(_directory);
                Log.Info($"Report directory was missing. Creating directory at {_directory}");
            }
        }

        private void ClearEngineTables()
        {
            Log.Info("CLEARING rules tables.");

            using (var validationResultsDbContext = new ValidationResultsDbContext())
            {
                foreach (var ruleValidation in RuleValidations)
                {
                    validationResultsDbContext.RuleValidations.Remove(validationResultsDbContext.RuleValidations.First(r => r.RuleValidationId == ruleValidation.RuleValidationId));
                    validationResultsDbContext.SaveChanges();
                }
            }

            Log.Info("CLEARED rules tables.");
        }

        private void EmailReport(string csvName, bool errors)
        {
            Log.Info($"Sending email to {string.Join(",", _emailRecipients)}.");
            if(errors)
                _emailService.SendReportEmail(_emailRecipients, csvName, CreateEmailBodyWithErrors());
            else
                _emailService.SendReportEmail(_emailRecipients, csvName, CreateEmailBodyWithNoErrors());
        }

        private string CreateEmailBodyWithErrors()
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
                      <p>Ods Name : <strong>{_odsName}</strong></p>
                      <p>Collections ran : <strong>{string.Join(",", collectionList)}</strong></p>";
        }

        private string CreateEmailBodyWithNoErrors()
        {
            return $@"<p><strong>Business Rules Engine found 0 errors.</strong></p>
                      <p>Ods Name : <strong>{_odsName}</strong></p>";
        }

        // Validation Results
        private void SetAllValidationDetails()
        {
            using (var validationResultsDbContext = new ValidationResultsDbContext())
            {
                ValidationDetails = validationResultsDbContext.RuleValidationDetails.ToList();
            }
        }

        // Collections
        private void SetAllRuleValidations()
        {
            using (var validationResultsDbContext = new ValidationResultsDbContext())
            {
                RuleValidations = validationResultsDbContext.RuleValidations.ToList();
            }
        }

        // Rules
        private void SetAllRuleComponents()
        {
            using (var validationResultsDbContext = new ValidationResultsDbContext())
            {
                RuleComponents = validationResultsDbContext.RuleValidationRuleComponents.ToList();
            }
        }

        private void SetAllEmailRecipients()
        {
            using (var validationResultsDbContext = new ValidationResultsDbContext())
            {
                var records = validationResultsDbContext.RuleValidationRecipients.ToList();
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