using CsvHelper.Configuration.Attributes;

namespace MaintenanceScriptsCore
{
    public class CsvModelBase
    {
        [Index(0)]
        public string FirstName { get; set; }
        [Index(1)]
        public string LastName { get; set; }
        [Index(2)]
        public string Email { get; set; }
    }
}