using Cherries.Models.Command;
using Cherries.Models.dbo;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cherries.Services.Interfaces
{
    public interface ILicenseService : IServiceBase
    {
        LicenseViewModel UpdateLicense(UpdateLicenseCommand command);
        LicenseCalculationViewModel LicenseCalculation(LicenseCalculationQuery query, User user);
    }
}
