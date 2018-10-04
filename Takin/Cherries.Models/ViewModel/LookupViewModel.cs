using Cherries.Models.App;
using Cherries.Models.dbo;
using Cherries.Models.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Cherries.Models.App.PortRiskItem;

namespace Cherries.Models.ViewModel
{
    public class LookupViewModel
    {
        public Dictionary<string, List<CatData>> Categories { get; set; }
        public List<cPortRiskItem> PortRisks { get; set; }
        public List<LicenceService> Services { get; set; }
        public List<Country> Countries { get; set; }
        public List<District> Districts { get; set; }
        public List<Gender> Genders { get; set; }
        public List<Currency> Currencies { get; set; }
        public LookupViewModel()
        {
            Categories = new Dictionary<string, List<CatData>>();
            PortRisks = new List<App.PortRiskItem.cPortRiskItem>();
            Services = new List<LicenceService>();
            Countries = new List<Country>();
            Districts = new List<District>();
            Genders = new List<Gender>();
            Currencies = new List<Models.Lookup.Currency>();
        }
    }
}
