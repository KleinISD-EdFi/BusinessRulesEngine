using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessRulesEngineConsoleApp.Models;
using Engine.Models;
using SimpleInjector;

namespace BusinessRulesEngineConsoleApp
{
    static class Program
    {
        static readonly Container container;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Program()
        {
            container = new Container();

            container.Register<IModel, Model>();
            container.Register<IEmailService, EmailService>();
            container.Register<IReportService, ReportService>();
            container.Register<IRulesEngineRunner, RulesEngineRunner>();
            container.Register<IRulesEngineService, RulesEngineService>();
            
            container.Verify();
        }

        static void Main(string[] args)
        {
            Log.Info($"NEW run starting at {DateTime.Now}");
            var rulesEngineRunner = container.GetInstance<IRulesEngineRunner>();
            rulesEngineRunner.RunEngine();
        }
    }
}
