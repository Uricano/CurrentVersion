using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class LicenceServiceViewModel : BaseViewModel
    {
        public List<LicenceService> Services { get; set; }
    }
}
