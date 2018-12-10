using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngineConsoleApp.Models
{
    // Returns the record data in CSV form.
    public class ReportData
    {
        public string Collection { get; set; }
        public string Rule { get; set; }
        public string Id { get; set; }
        public string Table { get; set; }
        public string Message { get; set; }

        // public string CsvString => $"{Collection},{Rule},rules.{Table},{Id},{Message}";
    }

}
