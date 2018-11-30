using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Language;

namespace BusinessRulesEngineConsoleApp.Models
{
    public class EngineSchemaProvider : ISchemaProvider
    {
        public string Value { get; }

        public EngineSchemaProvider()
        {
            Value = new RulesEngineConfiguration().RuleEngineResultsSchema;
        }


    }
}
