using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Engine.Models;
using System.Data.SqlClient;
using BusinessRulesEngine.Database;

namespace BusinessRulesEngine.Models
{
    public interface IRulesEngineService
    {
        void RunEngine(string fourDigitOdsDbYear, string collectionId);
        List<Collection> GetCollections();
    }

    public class RulesEngineService : IRulesEngineService
    {
        protected readonly Model _engineObjectModel;

        public RulesEngineService(Model engineObjectModel)
        {
            _engineObjectModel = engineObjectModel;
        }

        public List<Collection> GetCollections()
        {
            return _engineObjectModel.Collections.ToList();
        }

        public void RunEngine(string fourDigitOdsDbYear, string collectionId)
        {
            // ValidationReportSummary newReportSummary = null;
            using (var odsRawDbContext = new RawOdsDbContext(fourDigitOdsDbYear))
            {
                // Run the rules - This code is adapted from an example in the Rule Engine project.
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
                    var detailParams = new List<SqlParameter> { new SqlParameter("@RuleValidationId", collectionId) };
                    detailParams.AddRange(_engineObjectModel.GetParameters(collectionId).Select(x => new SqlParameter(x.ParameterName, x.Value)));
                    odsRawDbContext.Database.CommandTimeout = 60;
                    var result = odsRawDbContext.Database.ExecuteSqlCommand(rule.ExecSql, detailParams.ToArray());

                    // The error is currently that the @RuleValidationId is not being set.

                    // INSERT INTO[rules].[RuleValidationDetail]
                    // SELECT 10014, * FROM(SELECT DISTINCT [Ids].[Id], '1.1.1' [RuleId], CAST(1 AS BIT) [IsError], 'Ben should have last name Brady.' [Message]
                    // FROM(SELECT[Id] FROM [rules].[Student]) [Ids]
                    // LEFT OUTER JOIN[rules].[Student]
                    // ON[Ids].[Id] = [Student].[Id]
                    // WHERE(([Student].[FirstName] = 'Ben')) AND NOT(([Student].[LastSurname] = 'Brady'))
                    // ) _sql
                }
            }
        }
    }
}