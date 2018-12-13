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
        private IModel _engineObjectModel;
        private IRulesEngineService _rulesEngineService;
        public readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public RulesEngineRunner()
        {
            Func<Model> modelCreatorDelegate = () => new ModelBuilder(new DirectoryRulesStreams(new RulesEngineConfiguration().RulesFileFolder).Streams).Build(null, new EngineSchemaProvider());
            _engineObjectModel = modelCreatorDelegate.Invoke();
            _rulesEngineService = new RulesEngineService(_engineObjectModel);
        }

        public bool RunEngine()
        {
            Log.Info($"NEW run starting at {DateTime.Now}");

            var collections = _rulesEngineService.GetCollections();
            var ruleValidationIds = new List<int>();
            
            try
            {
                foreach (var collection in collections)
                {
                    ruleValidationIds.Add(_rulesEngineService.RunEngine(collection.CollectionId));
                }
                var reportService = new ReportService();

                reportService.CreateReport(ruleValidationIds, collections);

                return true;
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return false;
            }   
        }
    }
}
