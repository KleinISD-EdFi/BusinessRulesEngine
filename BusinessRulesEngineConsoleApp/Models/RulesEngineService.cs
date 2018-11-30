using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Engine.Models;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using BusinessRulesEngineConsoleApp.Database;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IRulesEngineService
    {
        int RunEngine(string collectionId);
        List<Collection> GetCollections();
    }

    public class RulesEngineService : IRulesEngineService
    {
        protected readonly IModel _engineObjectModel;
        protected string _fourDigitOdsDbYear = DateTime.Now.ToString("yyyy");

        // public RulesEngineService(Model engineObjectModel, string fourDigitOdsDbYear)
        // {
        //     EngineObjectModel = engineObjectModel;
        //     FourDigitOdsDbYear = fourDigitOdsDbYear;
        // }

        public RulesEngineService(IModel engineObjectModel)
        {
            _engineObjectModel = engineObjectModel;
        }

        public List<Collection> GetCollections()
        {
            return _engineObjectModel.GetCollections().ToList();
        }

        public int RunEngine(string collectionId)
        {
            // ValidationReportSummary newReportSummary = null;
            using (var odsRawDbContext = new RawOdsDbContext(_fourDigitOdsDbYear))
            {
                var newRuleValidationExecution = new RuleValidation { CollectionId = collectionId };
                odsRawDbContext.RuleValidations.Add(newRuleValidationExecution);
                odsRawDbContext.SaveChanges();

                var rules = _engineObjectModel.GetRules(collectionId).ToArray();
                var ruleComponents = rules.SelectMany(r => r.Components.Distinct().Select(c => new { r.RulesetId, r.RuleId, Component = c }));
                foreach (var singleRuleNeedingToBeValidated in ruleComponents)
                {
                    odsRawDbContext.RuleValidationRuleComponents.Add(new RuleValidationRuleComponent
                    {
                        RuleValidationId = newRuleValidationExecution.RuleValidationId,
                        RulesetId = singleRuleNeedingToBeValidated.RulesetId,
                        RuleId = singleRuleNeedingToBeValidated.RuleId,
                        Component = singleRuleNeedingToBeValidated.Component
                    });
                }
                odsRawDbContext.SaveChanges();

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
}