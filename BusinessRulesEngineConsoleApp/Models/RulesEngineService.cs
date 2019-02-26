using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Models;
using System.Data.SqlClient;
using BusinessRulesEngineConsoleApp.Database;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IRulesEngineService
    {
        int RunEngine(string collectionId, string fourDigitOdsDbYear = null);
        void SetFourDigitOdsYear(string fourDigitOdsYear);
        List<Collection> GetCollections();
    }

    public class RulesEngineService : IRulesEngineService
    {
        private readonly IModel _engineObjectModel;
        private string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");

        public RulesEngineService(IModel engineObjectModel)
        {
            _engineObjectModel = engineObjectModel;
        }

        public List<Collection> GetCollections()
        {
            return _engineObjectModel.GetCollections().ToList();
        }

        public int RunEngine(string collectionId, string fourDigitOdsDbYear = null)
        {
            if (fourDigitOdsDbYear != null)
                _fourDigitOdsDbYear = fourDigitOdsDbYear;

            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                using (var validationResultsDbContext = new ValidationResultsDbContext())
                {
                    var newRuleValidationExecution = new RuleValidation { CollectionId = collectionId };
                    validationResultsDbContext.RuleValidations.Add(newRuleValidationExecution);
                    validationResultsDbContext.SaveChanges();

                    var rules = _engineObjectModel.GetRules(collectionId).ToArray();
                    var ruleComponents = rules.SelectMany(r => r.Components.Distinct().Select(c => new { r.RulesetId, r.RuleId, Component = c }));
                    foreach (var singleRuleNeedingToBeValidated in ruleComponents)
                    {
                        validationResultsDbContext.RuleValidationRuleComponents.Add(new RuleValidationRuleComponent
                        {
                            RuleValidationId = newRuleValidationExecution.RuleValidationId,
                            RulesetId = singleRuleNeedingToBeValidated.RulesetId,
                            RuleId = singleRuleNeedingToBeValidated.RuleId,
                            Component = singleRuleNeedingToBeValidated.Component
                        });
                    }
                    validationResultsDbContext.SaveChanges();
                    foreach (var rule in rules)
                    {
                        var detailParams = new List<SqlParameter> { new SqlParameter("@RuleValidationId", newRuleValidationExecution.RuleValidationId) };
                        detailParams.AddRange(_engineObjectModel.GetParameters(collectionId).Select(x => new SqlParameter(x.ParameterName, x.Value)));
                        odsRawDbContext.Database.CommandTimeout = 60;
                        var result = odsRawDbContext.Database.ExecuteSqlCommand(rule.ExecSql, detailParams.ToArray());
                    }
                    return (int)newRuleValidationExecution.RuleValidationId;
                }
            }
        }

        public void SetFourDigitOdsYear(string fourDigitOdsYear)
        {
            if (fourDigitOdsYear != null)
                _fourDigitOdsDbYear = fourDigitOdsYear;
        }
    }
}