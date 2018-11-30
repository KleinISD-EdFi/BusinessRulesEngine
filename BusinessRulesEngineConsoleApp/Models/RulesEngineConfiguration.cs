using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IRulesEngineConfiguration
    {
        string RulesFileFolder { get; }
        string RuleEngineResultsSchema { get; }
    }

    public class RulesEngineConfiguration : IRulesEngineConfiguration
    {
        private readonly string _ruleEngineResultsConnectionString;

        public RulesEngineConfiguration()
        {
            RulesFileFolder = ConfigurationManager.AppSettings["RulesFileFolder"];
            RuleEngineResultsSchema = ConfigurationManager.AppSettings["RulesEngineResultsSchema"];
        }

        public string RulesFileFolder { get; }

        public string RuleEngineResultsSchema { get; }
    }
}
