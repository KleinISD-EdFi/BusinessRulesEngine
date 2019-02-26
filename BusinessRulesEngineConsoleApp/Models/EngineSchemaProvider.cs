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
