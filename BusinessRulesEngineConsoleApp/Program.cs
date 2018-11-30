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

        static Program()
        {
            container = new Container();

            container.Register<IEmailService, EmailService>();
            container.Register<IRulesEngineRunner, RulesEngineRunner>();
            container.Register<IRulesEngineService, RulesEngineService>();
            container.Register<IModel, Model>();

            container.Verify();
        }

        static void Main(string[] args)
        {
            var rulesEngineRunner = container.GetInstance<IRulesEngineRunner>();
            rulesEngineRunner.RunEngine();
        }
    }
}
