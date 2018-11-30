using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models;
using Engine.Utility;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IRulesEngineRunner
    {
        bool RunEngine();
    }

    public class RulesEngineRunner : IRulesEngineRunner
    {
        private Model _engineObjectModel;
        private IRulesEngineService _rulesEngineService;

        // public RulesEngineRunner()
        // {
        //     Func<Model> modelCreatorDelegate = () => new ModelBuilder(new DirectoryRulesStreams(new RulesEngineConfiguration().RulesFileFolder).Streams).Build(null, new EngineSchemaProvider());
        //     _engineObjectModel = modelCreatorDelegate.Invoke();
        //     _rulesEngineService = new RulesEngineService(_engineObjectModel, _fourDigitOdsDbYear);
        // }
        public RulesEngineRunner(IRulesEngineService rulesEngineService)
        {
            _rulesEngineService = rulesEngineService;
        }

        public bool RunEngine()
        {
            Func<Model> modelCreatorDelegate = () => new ModelBuilder(new DirectoryRulesStreams(new RulesEngineConfiguration().RulesFileFolder).Streams).Build(null, new EngineSchemaProvider());
            _engineObjectModel = modelCreatorDelegate.Invoke();
            _rulesEngineService = new RulesEngineService(_engineObjectModel);

            var collections = _rulesEngineService.GetCollections();
            var ruleValidationIds = new List<int>();
            
            try
            {
                foreach (var collection in collections)
                {
                    ruleValidationIds.Add(_rulesEngineService.RunEngine(collection.CollectionId));
                }
                var reportService = new ReportService();

                reportService.CreateReport(ruleValidationIds);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }   
        }
    }
}
