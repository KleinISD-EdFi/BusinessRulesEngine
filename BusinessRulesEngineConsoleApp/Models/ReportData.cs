namespace BusinessRulesEngineConsoleApp.Models
{
    // Returns the record data in CSV form.
    public class ReportData
    {
        public string Collection { get; set; }
        public string Rule { get; set; }
        public long Id { get; set; }
        public string Message { get; set; }
        public string Table { get; set; }

        public string CsvString => $"{Collection},{Id},{Rule},{Message}";
    }

}
