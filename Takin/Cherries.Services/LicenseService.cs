using Cherries.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cherries.Models.Command;
using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using TFI.BusinessLogic.Interfaces;
using Cherries.Models.dbo;

namespace Cherries.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ILicenseBL _licenseBL;
        public LicenseService(ILicenseBL licenseBL)
        {
            _licenseBL = licenseBL;
        }

        public LicenseCalculationViewModel LicenseCalculation(LicenseCalculationQuery query, User user)
        {
            var vm = _licenseBL.LicenseCalculation(query, user);
            return vm;
        }

        public LicenseViewModel UpdateLicense(UpdateLicenseCommand command)
        {
            var vm = _licenseBL.UpdateLicense(command);
            return vm;
        }
    }
}
