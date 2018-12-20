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
        bool RunEngine(string fourDigitOdsYear = null);
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

        public bool RunEngine(string fourDigitOdsYear = null)
        {
            Log.Info($"STARTING new run at {DateTime.Now}");

            var collections = _rulesEngineService.GetCollections();
            var ruleValidationIds = new List<int>();

            // If command line argument is successfully passed then set it in the rulesEngineService here.
            if(fourDigitOdsYear != null)
                _rulesEngineService.SetFourDigitOdsYear(fourDigitOdsYear);

            try
            {
                foreach (var collection in collections)
                {
                    ruleValidationIds.Add(_rulesEngineService.RunEngine(collection.CollectionId));
                }

                var reportService = new ReportService();
                reportService.CreateAndEmailReport(ruleValidationIds, collections);

                Log.Info($"COMPLETED at {DateTime.Now}");

                return true;
            }
            catch(Exception ex)
            {
                Log.Error("FAILED");
                Log.Error(ex);

                return false;
            }
        }
    }
}
