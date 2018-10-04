using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class LicenseViewModel : BaseViewModel
    {
        public UserLicence License { get; set; }
    }
}
