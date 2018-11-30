using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IOdsConfigurationValues
    {
        string GetRawOdsConnectionString(string fourDigitYear);
    }

    public class OdsConfigurationValues : IOdsConfigurationValues
    {
        private readonly string _rawOdsConnectionString;

        public OdsConfigurationValues()
        {
            _rawOdsConnectionString = ConfigurationManager.ConnectionStrings["RawOdsDbContext"]?.ToString();
        }

        public string GetRawOdsConnectionString(string fourDigitYear)
        {
            return string.Format(_rawOdsConnectionString, fourDigitYear);
        }
    }
}