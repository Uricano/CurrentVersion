using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.Mappers
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile() : base("ViewModelToDomainMappingProfile")
        {

        }

        protected override void Configure()
        {
            Mapper.CreateMap<Cherries.Models.App.PortfolioDetails, TFI.Entities.dbo.Portfolio>()
                 .ForMember(x => x.idPortfolio, m => m.MapFrom(n => n.ID))
                 .ForMember(x => x.strCode, m => m.MapFrom(n => n.Code))
                 .ForMember(x => x.strName, m => m.MapFrom(n => n.Name))
                 .ForMember(x => x.idPortfolio, m => m.MapFrom(n => n.ID))
                 .ForMember(x => x.dEquity, m => m.MapFrom(n => n.Equity))
                 .ForMember(x => x.dtLastOpened, m => m.MapFrom(n => n.DateEdited))
                 .ForMember(x => x.dtCreated, m => m.MapFrom(n => n.DateCreated))
                 .ForMember(x => x.dtLastOptimization, m => m.MapFrom(n => n.LastOptimization))
                 .ForMember(x => x.dtEndDate, m => m.MapFrom(n => n.EndDate))
                 .ForMember(x => x.dtStartDate, m => m.MapFrom(n => n.StartDate))
                 .ForMember(x => x.iSecsNum, m => m.MapFrom(n => n.SecsNum))
                 .ForMember(x => x.CalcCurrency, m => m.MapFrom(n => n.CalcCurrency))
                 .ForMember(x => x.dCurrEquity, m => m.MapFrom(n => n.CurrEquity))
                 .ForMember(x => x.dInitRisk, m => m.MapFrom(n => n.PreferedRisk.UpperBound))
                 .ForMember(x => x.dCurrRisk, m => m.MapFrom(n => n.CurrentStDev))
                 .ForMember(x => x.iCalcPreference, m => m.MapFrom(n => (int)n.CalcType))
                 .ForMember(x => x.userId, m => m.MapFrom(n => n.UserID));
            //.ForMember(x => x.iMaxSecs, m => m.MapFrom(n => n.MaxSecs));
            Mapper.CreateMap<Cherries.Models.App.PortfolioDetails, TFI.Entities.dbo.BacktestingPortfolio>()
           .ForMember(x => x.idPortfolio, m => m.MapFrom(n => n.ID))
           .ForMember(x => x.strCode, m => m.MapFrom(n => n.Code))
           .ForMember(x => x.strName, m => m.MapFrom(n => n.Name))
           .ForMember(x => x.idPortfolio, m => m.MapFrom(n => n.ID))
           .ForMember(x => x.dEquity, m => m.MapFrom(n => n.Equity))
           .ForMember(x => x.dtLastOpened, m => m.MapFrom(n => n.DateEdited))
           .ForMember(x => x.dtCreated, m => m.MapFrom(n => n.DateCreated))
           .ForMember(x => x.dtLastOptimization, m => m.MapFrom(n => n.LastOptimization))
           .ForMember(x => x.dtEndDate, m => m.MapFrom(n => n.EndDate))
                 .ForMember(x => x.dtStartDate, m => m.MapFrom(n => n.StartDate))
           .ForMember(x => x.iSecsNum, m => m.MapFrom(n => n.SecsNum))
           .ForMember(x => x.CalcCurrency, m => m.MapFrom(n => n.CalcCurrency))
           .ForMember(x => x.dCurrEquity, m => m.MapFrom(n => n.CurrEquity))
           .ForMember(x => x.dInitRisk, m => m.MapFrom(n => n.PreferedRisk.UpperBound))
           .ForMember(x => x.dCurrRisk, m => m.MapFrom(n => n.CurrentStDev))
           .ForMember(x => x.iCalcPreference, m => m.MapFrom(n => (int)n.CalcType))
           .ForMember(x => x.userId, m => m.MapFrom(n => n.UserID));

            Mapper.CreateMap<Cherries.Models.App.OptimalPortfolioSecurity, TFI.Entities.dbo.PortfolioSecurities>()
                 .BeforeMap((x, v) => { if (v.Securities == null) { v.Securities = new TFI.Entities.dbo.Security { idSecurity = x.idSecurity }; } })
                 .ForMember(x => x.dtEndDate, m => m.MapFrom(n => n.dtPriceEnd))
                 .ForMember(x => x.dtStartDate, m => m.MapFrom(n => n.dtPriceStart))
                 .ForMember(x => x.flLastPrice, m => m.MapFrom(n => n.LastPrice))
                 .ForMember(x => x.flQuantity, m => m.MapFrom(n => n.Quantity))
                 .ForMember(x => x.flRate, m => m.MapFrom(n => n.FinalRate))
                 .ForMember(x => x.flRisk, m => m.MapFrom(n => n.StandardDeviation))
                 .ForMember(x => x.flWeight, m => m.MapFrom(n => n.Weight))
                 .ForMember(x => x.idPortSec, m => { m.Condition(c => c.DestinationValue == null); m.MapFrom(n => Guid.NewGuid()); });

            Mapper.CreateMap<Cherries.Models.App.OptimalPortfolioSecurity, TFI.Entities.dbo.BacktestingPortfolioSecurities>()
                .BeforeMap((x, v) => { if (v.Securities == null) { v.Securities = new TFI.Entities.dbo.Security { idSecurity = x.idSecurity }; } })
                .ForMember(x => x.dtEndDate, m => m.MapFrom(n => n.dtPriceEnd))
                .ForMember(x => x.dtStartDate, m => m.MapFrom(n => n.dtPriceStart))
                .ForMember(x => x.flLastPrice, m => m.MapFrom(n => n.LastPrice))
                .ForMember(x => x.flQuantity, m => m.MapFrom(n => n.Quantity))
                .ForMember(x => x.flRate, m => m.MapFrom(n => n.FinalRate))
                .ForMember(x => x.flRisk, m => m.MapFrom(n => n.StandardDeviation))
                .ForMember(x => x.flWeight, m => m.MapFrom(n => n.Weight))
                .ForMember(x => x.idPortSec, m => { m.Condition(c => c.DestinationValue == null); m.MapFrom(n => Guid.NewGuid()); })
                .AfterMap((x, v) =>
                {
                    v.isBenchmark = x.idSecurityType == 106;
                });

            Mapper.CreateMap<Cherries.Models.Command.BasePortfolioCommand, TFI.Entities.dbo.Portfolio>()
                 .ForMember(x => x.dCurrRisk, m => m.MapFrom(n => n.Risk))
                 .ForMember(x => x.dEquity, m => m.MapFrom(n => n.Equity))
                 .ForMember(x => x.iCalcPreference, m => m.MapFrom(n => (int)n.CalcType));

            Mapper.CreateMap<Cherries.Models.Command.BasePortfolioCommand, Entities.dbo.BacktestingPortfolio>()
                 .ForMember(x => x.dCurrRisk, m => m.MapFrom(n => n.Risk))
                 .ForMember(x => x.dEquity, m => m.MapFrom(n => n.Equity))
                 .ForMember(x => x.iCalcPreference, m => m.MapFrom(n => (int)n.CalcType));


            Mapper.CreateMap<Cherries.Models.dbo.LicenceService, TFI.Entities.dbo.Licservices>();

            Mapper.CreateMap<Cherries.Models.dbo.Transactions, TFI.Entities.dbo.LicTransactions>();

            Mapper.CreateMap<Cherries.Models.dbo.UserLicence, TFI.Entities.dbo.Userlicenses>()
              .ForMember(x => x.dtActivationDate, m => m.MapFrom(n => n.ActivationDate))
              .ForMember(x => x.dtExpirationDate, m => m.MapFrom(n => n.ExpiryDate))
              .ForMember(x => x.isTrial, m => m.MapFrom(n => n.isTrial))
              //.ForMember(x => x.Licenseexchanges, m => m.MapFrom(n => n.Stocks))
              .ForMember(x => x.dtPurchaseDate, m => m.MapFrom(n => n.PurchaseDate))
              .ForMember(x => x.Transaction, m => m.MapFrom(n => n.Transaction))
              .ForMember(x => x.tb_LicServices, m => m.MapFrom(n => n.Service));

            Mapper.CreateMap<Cherries.Models.dbo.User, TFI.Entities.dbo.Users>()
              .ForMember(x => x.Email, m => m.MapFrom(n => n.Email))
              .ForMember(x => x.Name, m => m.MapFrom(n => n.Name))
              //.ForMember(x => x.LastName, m => m.MapFrom(n => n.User.LastName))
              .ForMember(x => x.isTemporary, m => m.MapFrom(n => n.isTemporary))
              .ForMember(x => x.Username, m => m.MapFrom(n => n.Username))
              .ForMember(x => x.Userlicenses, m => m.MapFrom(n => new List<Cherries.Models.dbo.UserLicence> { n.Licence }))
              .ForMember(x => x.Currency, m => m.MapFrom(n => n.Currency))
              .ForMember(x => x.UserID, m => m.MapFrom(n => n.UserID));

            Mapper.CreateMap<Cherries.Models.Lookup.StockMarket, TFI.Entities.Lookup.SelStockExchange>()
               .ForMember(x => x.id, m => m.MapFrom(n => n.id))
               .ForMember(x => x.strHebName, m => m.MapFrom(n => n.HebName))
               .ForMember(x => x.Currency, m => m.Ignore())
                //.ForMember(x => x.Currency, m => m.MapFrom(n => n.Currency))
                .ForMember(x => x.strName, m => m.MapFrom(n => n.Name));

            Mapper.CreateMap<Cherries.Models.Lookup.Currency, TFI.Entities.Lookup.SelCurrency>()
               .ForMember(x => x.Name, m => m.MapFrom(n => n.CurrecnyName))
               .ForMember(x => x.Sign, m => m.MapFrom(n => n.CurrencySign))
                .ForMember(x => x.CurrencyID, m => m.MapFrom(n => n.CurrencyId));

            Mapper.CreateMap<Cherries.Models.Lookup.Country, Entities.Lookup.Country>();
            Mapper.CreateMap<Cherries.Models.Lookup.District, Entities.Lookup.District>();
        }
    }
}
