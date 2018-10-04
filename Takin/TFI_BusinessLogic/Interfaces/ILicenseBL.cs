using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ILicenseBL : IBaseBL
    {
        LicenseViewModel UpdateLicense(UpdateLicenseCommand command);
        LicenseCalculationViewModel LicenseCalculation(LicenseCalculationQuery query, User user);
    }
}
