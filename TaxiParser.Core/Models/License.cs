using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiParser.Core.Models
{
    public class License
    {
        public string LicenseNumber { get; set; }
        public string CarNumber { get; set; }
        public string LicenseOwner { get; set; }
        public bool IsCompany { get; set; }
        public string CarVendor { get; set; }
        public string CarModel { get; set; }
        public DateTime LicenseStart { get; set; }
        public DateTime? LicenseEnd { get; set; }
        public string State { get; set; }
    }
}
