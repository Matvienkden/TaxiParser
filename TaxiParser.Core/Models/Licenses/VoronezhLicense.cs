using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiParser.Core.Models.Licenses
{

    public class VoronezhLicense: ILicense
    {
        public string LicenseState { get; set; }        
        public string LicenseNumber { get; set; }
        public string LicenseRegNumber { get; set; } // Рег.номер
        public string LicenseOrderNumber { get; set; } // Дата приказа
        public DateTime? LicenseStart { get; set; } // Дата приказа
        public DateTime? LicenseEnd { get; set; }

        public string CarNumber { get; set; }
        public string CarVendor { get; set; }
        public string CarModel { get; set; }
        public int CarYear { get; set; }

        public string OwnerOgrn { get; set; }
        public string OwnerOgrnip { get; set; }
        public string OwnerCompanyName { get; set; }
        public string OwnerFio { get; set; }
        public string OwnerAddress { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerContacts { get; set; }

    }

}
