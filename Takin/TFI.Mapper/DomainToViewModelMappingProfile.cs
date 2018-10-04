using AutoMapper;
using Cherries.TFI.BusinessLogic.Collections;
using Cherries.TFI.BusinessLogic.DataManagement.DataTypes;
using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Data;
using System.Linq;
using TFI.BusinessLogic.Enums;

namespace TFI.Mappers
{
   
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile() : base("DomainToViewModelMappingProfile")
        {
           
        }


        protected override void Configure()
        {
            //LR: I don't think we need this mapping
            Mapper.CreateMap<TFI.Entities.dbo.Price, Cherries.Models.dbo.Price>();

            Mapper.CreateMap<Cherries.Models.dbo.Price, Entities.dbo.Price>()
             .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.idSecurity))
             .ForMember(x => x.dDate, m => m.MapFrom(n => n.dDate))
             .ForMember(x => x.fOpen, m => m.MapFrom(n => n.fOpen))
             .ForMember(x => x.fClose, m => m.MapFrom(n => n.fClose))
             .ForMember(x => x.fNISOpen, m => m.MapFrom(n => n.fNISOpen))
             .ForMember(x => x.fNISClose, m => m.MapFrom(n => n.fNISClose))
             .ForMember(x => x.isHoliday, m => m.MapFrom(n => n.isHoliday));


            Mapper.CreateMap<TFI.Entities.dbo.BMPrice, Cherries.Models.dbo.BMPrice>();

            //Mapper.CreateMap<Entities.dbo.Price, Cherries.Models.dbo.BMPrice>()
            Mapper.CreateMap<Cherries.Models.dbo.BMPrice, Entities.dbo.Price>()
                 .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.idSecurity))
                 .ForMember(x => x.dDate, m => m.MapFrom(n => n.dDate))
                 .ForMember(x => x.fOpen, m => m.MapFrom(n => n.fOpen))
                 .ForMember(x => x.fClose, m => m.MapFrom(n => n.fClose))
                 .ForMember(x => x.fNISOpen, m => m.MapFrom(n => n.fNISOpen))
                 .ForMember(x => x.fNISClose, m => m.MapFrom(n => n.fNISClose))
                 .ForMember(x => x.isHoliday, m => m.MapFrom(n => n.isHoliday));
            //Mapper.CreateMap<TFI.Entities.dbo.BMsecurity, Cherries.Models.dbo.BMsecurity>();

            //Mapper.CreateMap<TFI.Entities.dbo.PriceReturn, Cherries.Models.dbo.Price>();
            //.ForMember(x => x.idSecurity, m => m.MapFrom(n => n.Securities.idSecurity));
            Mapper.CreateMap<TFI.Entities.dbo.PortfolioSecurities, Cherries.Models.dbo.Security>()
                .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.Securities.idSecurity))
                .ForMember(x => x.strSymbol, m => m.MapFrom(n => n.Securities.strSymbol))
                .ForMember(x => x.strName, m => m.MapFrom(n => n.Securities.strName))
                .ForMember(x => x.idMarket, m => m.MapFrom(n => n.Securities.idMarket))
                .ForMember(x => x.idSector, m => m.MapFrom(n => n.Securities.idSector))
                .ForMember(x => x.dtPriceEnd, m => m.MapFrom(n => n.Securities.dtPriceEnd))
                .ForMember(x => x.idSecurityType, m => m.MapFrom(n => n.Securities.idSecurityType))
                //.ForMember(x => x.strISIN, m => m.MapFrom(n => n.Securities.strISIN))
                //.ForMember(x => x.FAC, m => m.MapFrom(n => n.Securities.FAC))
                .ForMember(x => x.StdYield, m => m.MapFrom(n => n.Securities.StdYield))
                .ForMember(x => x.strHebName, m => m.MapFrom(n => n.Securities.strHebName))
                .ForMember(x => x.AvgYield, m => m.MapFrom(n => n.Securities.AvgYield))
                .ForMember(x => x.StdYieldNIS, m => m.MapFrom(n => n.Securities.StdYieldNIS))
                .ForMember(x => x.AvgYieldNIS, m => m.MapFrom(n => n.Securities.AvgYieldNIS))
                .ForMember(x => x.dValueUSA, m => m.MapFrom(n => n.Securities.MonetaryAvg))
                .ForMember(x => x.dValueNIS, m => m.MapFrom(n => n.Securities.MonetaryAvgNIS))
                .ForMember(x => x.WeightUSA, m => m.MapFrom(n => n.Securities.WeightUSA))
                .ForMember(x => x.WeightNIS, m => m.MapFrom(n => n.Securities.WeightNIS))
                .ForMember(x => x.Prices, m => m.MapFrom(n => n.Securities.Prices))
                .ForMember(x => x.isActiveSecurity, m => m.MapFrom(n => n.isActiveSecurity))
                .ForMember(x => x.dtStartDate, m => m.MapFrom(n => n.dtStartDate))
                .ForMember(x => x.dtEndDate, m => m.MapFrom(n => n.dtEndDate));

            Mapper.CreateMap<Cherries.TFI.BusinessLogic.Optimization.cOptPortSecurity, Cherries.Models.App.OptimalPortfolioSecurity>()
                .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.Security.Properties.PortSecurityId))
                .ForMember(x => x.dtPriceStart, m => m.MapFrom(n => n.Security.DateRange.StartDate))
                .ForMember(x => x.dtPriceEnd, m => m.MapFrom(n => n.Security.DateRange.EndDate))
                .ForMember(x => x.Name, m => m.MapFrom(n => n.Security.Properties.SecurityName))
                .ForMember(x => x.HebName, m => m.MapFrom(n => n.Security.Properties.HebName))
                 .ForMember(x => x.idMarket, m => m.MapFrom(n => n.Security.Properties.Market.ID))
                  .ForMember(x => x.idSector, m => m.MapFrom(n => n.Security.Properties.Sector.ID))
                   .ForMember(x => x.marketName, m => m.MapFrom(n => n.Security.Properties.MarketName))
                    .ForMember(x => x.sectorName, m => m.MapFrom(n => n.Security.Properties.Sector.ItemName))
                     .ForMember(x => x.idSecurityType, m => m.MapFrom(n => n.Security.Properties.SecurityType.ID))
                      .ForMember(x => x.securityTypeName, m => m.MapFrom(n => n.Security.Properties.SecurityType.ItemName))
                .ForMember(x => x.FinalRate, m => m.MapFrom(n => n.Security.RatesClass.FinalRate))
                .ForMember(x => x.LastPrice, m => m.MapFrom(n => n.Security.LastPrice))
                .ForMember(x => x.StandardDeviation, m => m.MapFrom(n => n.Security.CovarClass.StandardDeviation))
                .ForMember(x=> x.StdYield, m => m.MapFrom(n=>n.Security.StdYield))
                .ForMember(x=>x.IdCurrency, m=>m.MapFrom(n=>n.Security.IdCurrency))
                .ForMember(x => x.Symbol, m => m.MapFrom(n => n.Security.Properties.SecuritySymbol));

            Mapper.CreateMap<Cherries.TFI.BusinessLogic.Optimization.cOptimalPort, Cherries.Models.App.OptimalPortfolio>()
                .ForMember(x => x.Securities , m => m.MapFrom(n => n.Securities));

            Mapper.CreateMap<Cherries.TFI.BusinessLogic.Optimization.cOptimizationResults, Cherries.Models.ViewModel.OptimalPortoliosViewModel>()
                //.ForMember(x => x.SecuritiesTable, m => m.MapFrom(n => n.SecuritiesTable))
                .ForMember(x => x.Portfolios, m => m.MapFrom(n => n.Portfolios));
            Mapper.CreateMap<TFI.Entities.dbo.Security, Cherries.Models.dbo.Security>();

            Mapper.CreateMap<TFI.Entities.dbo.PortfolioSecurities, Cherries.Models.dbo.PortfolioSecurities>()
                .ForMember(x => x.Securities, m => m.MapFrom(n => n.Securities));

            Mapper.CreateMap<TFI.Entities.dbo.Portfolio, Cherries.Models.App.PortfolioDetails>()
                 .ForMember(x => x.ID, m => m.MapFrom(n => n.idPortfolio))
                 .ForMember(x => x.Code, m => m.MapFrom(n => n.strCode))
                 .ForMember(x => x.Name, m => m.MapFrom(n => n.strName))
                 .ForMember(x => x.ID, m => m.MapFrom(n => n.idPortfolio))
                 .ForMember(x => x.Equity, m => m.MapFrom(n => n.dEquity))
                 .ForMember(x => x.CurrEquity, m => m.MapFrom(n => n.dCurrEquity))
                 .ForMember(x => x.DateEdited, m => m.MapFrom(n => n.dtLastOpened))
                 .ForMember(x => x.DateCreated, m => m.MapFrom(n => n.dtCreated))
                 .ForMember(x => x.StartDate, m => m.MapFrom(n => n.dtStartDate))
                 .ForMember(x => x.EndDate, m => m.MapFrom(n => n.dtEndDate))
                 .ForMember(x => x.LastOptimization, m => m.MapFrom(n => n.dtLastOptimization))
                 .ForMember(x => x.SecsNum, m => m.MapFrom(n => n.iSecsNum))
                 .ForMember(x => x.CalcCurrency, m => m.MapFrom(n => n.CalcCurrency));
            //.ForMember(x => x.MaxSecs, m => m.MapFrom(n => n.iMaxSecs));

            Mapper.CreateMap<TFI.Entities.dbo.BacktestingPortfolio, Cherries.Models.App.PortfolioDetails>()
                 .ForMember(x => x.ID, m => m.MapFrom(n => n.idPortfolio))
                 .ForMember(x => x.Code, m => m.MapFrom(n => n.strCode))
                 .ForMember(x => x.Name, m => m.MapFrom(n => n.strName))
                 .ForMember(x => x.ID, m => m.MapFrom(n => n.idPortfolio))
                 .ForMember(x => x.Equity, m => m.MapFrom(n => n.dEquity))
                 .ForMember(x => x.CurrEquity, m => m.MapFrom(n => n.dCurrEquity))
                 .ForMember(x => x.DateEdited, m => m.MapFrom(n => n.dtLastOpened))
                 .ForMember(x => x.DateCreated, m => m.MapFrom(n => n.dtCreated))
                 .ForMember(x => x.StartDate, m => m.MapFrom(n => n.dtStartDate))
                 .ForMember(x => x.EndDate, m => m.MapFrom(n => n.dtEndDate))
                 .ForMember(x => x.LastOptimization, m => m.MapFrom(n => n.dtLastOptimization))
                 .ForMember(x => x.SecsNum, m => m.MapFrom(n => n.iSecsNum))
                 .ForMember(x => x.CalcCurrency, m => m.MapFrom(n => n.CalcCurrency));
            //.ForMember(x => x.MaxSecs, m => m.MapFrom(n => n.iMaxSecs));

            Mapper.CreateMap<TFI.Entities.Sp.UserPortfolio, Cherries.Models.App.PortfolioDetails>()
                 .ForMember(x => x.ID, m => m.MapFrom(n => n.idPortfolio))
                 .ForMember(x => x.Name, m => m.MapFrom(n => n.strName))
                 .ForMember(x => x.CalcCurrency, m => m.MapFrom(n => n.CalcCurrency))
                  .ForMember(x => x.CurrEquity, m => m.MapFrom(n => n.currentEquity))
                  .ForMember(x => x.DateCreated, m => m.MapFrom(n => n.dtCreated))
                  .ForMember(x => x.DateEdited, m => m.MapFrom(n => n.dtLastOptimization))
                  .ForMember(x => x.StartDate, m => m.MapFrom(n => n.dtStartDate))
                  .ForMember(x => x.EndDate, m => m.MapFrom(n => n.dtEndDate))
                  .ForMember(x => x.Equity, m => m.MapFrom(n => n.dEquity))
                  //.ForMember(x => x.MaxSecs, m => m.MapFrom(n => n.iMaxSecs))
                  .ForMember(x => x.SecsNum, m => m.MapFrom(n => n.iSecsNum))
                  .ForMember(x => x.CurrentStDev, m => m.MapFrom(n => n.dCurrRisk))
                  .ForMember(x => x.InitRisk, m => m.MapFrom(n => n.dInitRisk))
                  .ForMember(x => x.LastProfit, m => m.MapFrom(n => n.dLastProfit))
                  .ForMember(x => x.Profit, m => m.MapFrom(n => n.currentEquity - n.dEquity));
            //.ForMember(x => x.DefaultOptimizationType, m => m.MapFrom(n => (enumEfCalculationType)n.Portfolios.iCalcPreference))

            Mapper.CreateMap<TFI.Entities.Lookup.SelStockExchange, Cherries.Models.Lookup.StockMarket>()
                .ForMember(x => x.id, m => m.MapFrom(n => n.id))
                .ForMember(x => x.HebName, m => m.MapFrom(n => n.strHebName))
                 .ForMember(x => x.Currency, m => m.MapFrom(n => n.Currency.CurrencyID))
                  .ForMember(x => x.CurrencyRank, m => m.MapFrom(n => n.Currency.rank))
                 .ForMember(x => x.Name, m => m.MapFrom(n => n.strName));

            Mapper.CreateMap<TFI.Entities.Lookup.Exchangestock, Cherries.Models.Lookup.StockMarket>()
                .ForMember(x => x.id, m => m.MapFrom(n => n.idStockMarket))
                .ForMember(x=> x.HebName, m => m.MapFrom(n=>n.tblSel_StockMarkets.strHebName))
                 .ForMember(x => x.Name, m => m.MapFrom(n => n.tblSel_StockMarkets.strName));

            Mapper.CreateMap<TFI.Entities.Lookup.SelCurrency, Cherries.Models.Lookup.Currency>()
                .ForMember(x => x.CurrecnyName, m => m.MapFrom(n => n.Name))
                .ForMember(x => x.CurrencySign, m => m.MapFrom(n => n.Sign))
                 .ForMember(x => x.CurrencyId, m => m.MapFrom(n => n.CurrencyID));

            Mapper.CreateMap<TFI.Entities.dbo.Licservices, Cherries.Models.dbo.LicenceService>();
              // .ForMember(x => x.ExchangesPackage, m => m.MapFrom(n => n.tbl_SelExchanges.Markets.Select(s=>s.id).ToList()))
               //.ForMember(x => x.Stocks, m => m.MapFrom(n => n.));

            Mapper.CreateMap<TFI.Entities.dbo.Userlicenses, Cherries.Models.dbo.UserLicence>()
               .ForMember(x => x.ActivationDate, m => m.MapFrom(n => n.dtActivationDate))
               .ForMember(x => x.ExpiryDate, m => m.MapFrom(n => n.dtExpirationDate))
               .ForMember(x => x.isTrial, m => m.MapFrom(n => n.isTrial))
               .ForMember(x=> x.Stocks, m => m.MapFrom(n=>n.Licenseexchanges.Select(x=>x.Stockexchanges)))
               .ForMember(x => x.PurchaseDate, m => m.MapFrom(n => n.dtPurchaseDate))
               //.ForMember(x => x.Licensetype, m => m.MapFrom(n => n.Licensetypes.Idlicensetype))
               .ForMember(x => x.Transaction, m => m.MapFrom(n => n.Transaction))
               .ForMember(x => x.Service, m => m.MapFrom(n => n.tb_LicServices));

            Mapper.CreateMap<TFI.Entities.dbo.Userlicenses, Cherries.Models.dbo.User>()
              .ForMember(x => x.Email, m => m.MapFrom(n => n.User.Email))
              .ForMember(x => x.Name, m => m.MapFrom(n => n.User.Name))
              //.ForMember(x => x.LastName, m => m.MapFrom(n => n.User.LastName))
              .ForMember(x => x.isTemporary, m => m.MapFrom(n => n.User.isTemporary))
              .ForMember(x => x.Email, m => m.MapFrom(n => n.User.Email))
              .ForMember(x => x.Username, m => m.MapFrom(n => n.User.Username))
              .ForMember(x => x.CellPhone, m => m.MapFrom(n => n.User.CellPhone))
              .ForMember(x => x.Licence, m => m.MapFrom(n => n))
              .ForMember(x => x.Currency, m => m.MapFrom(n => n.User.Currency))
              .ForMember(x => x.UserID, m => m.MapFrom(n => n.User.UserID));

            Mapper.CreateMap<TFI.Entities.dbo.LicTransactions, Cherries.Models.dbo.Transactions>();

            Mapper.CreateMap<TFI.Entities.Sp.SecurityData, Cherries.Models.App.SecurityData>();

            //Mapper.CreateMap<Cherries.TFI.BusinessLogic.Optimization.Backtesting.cBacktestingCalculation, Cherries.Models.ViewModel.BackTestingViewModel>()
            //    .ForMember(x => x.benchMarkResult, m => m.MapFrom(n => n.ReturnsByDate))
            //    .ForMember(x => x.SecuritiesTable, m => m.MapFrom(n => n.PortfolioData));

            Mapper.CreateMap<TFI.Entities.dbo.TopSecurities, Cherries.Models.dbo.Security>()
                .ForMember(x => x.AvgYield, m => m.MapFrom(n => n.AvgYield))
                .ForMember(x => x.AvgYieldNIS, m => m.MapFrom(n => n.AvgYieldNIS))
                .ForMember(x => x.dtPriceStart, m => m.MapFrom(n => n.dtPriceStart))
                .ForMember(x => x.dtPriceEnd, m => m.MapFrom(n => n.dtPriceEnd))
                .ForMember(x => x.dValueNIS, m => m.MapFrom(n => n.MonetaryAvgNIS))
                .ForMember(x => x.dValueUSA, m => m.MapFrom(n => n.MonetaryAvg))
                //.ForMember(x => x.FAC, m => m.MapFrom(n => n.FAC))
                .ForMember(x => x.strHebName, m => m.MapFrom(n => n.strHebName))
                .ForMember(x => x.idCurrency, m => m.MapFrom(n => n.idCurrency))
                .ForMember(x => x.idMarket, m => m.MapFrom(n => n.idMarket))
                .ForMember(x => x.idSector, m => m.MapFrom(n => n.idSector))
                .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.idSecurity))
                .ForMember(x => x.idSecurityType, m => m.MapFrom(n => n.idSecurityType))
                .ForMember(x => x.sectorName, m => m.MapFrom(n => n.sectorName))
                .ForMember(x => x.marketName, m => m.MapFrom(n => n.marketName))
                .ForMember(x => x.securityTypeName, m => m.MapFrom(n => n.securityTypeName))
                //.ForMember(x => x.isActiveSecurity, m => m.MapFrom(n => n.is_Selected))
                .ForMember(x => x.StdYield, m => m.MapFrom(n => n.StdYield))
                .ForMember(x => x.StdYieldNIS, m => m.MapFrom(n => n.StdYieldNIS))
                //.ForMember(x => x.strISIN, m => m.MapFrom(n => n.strISIN))
                .ForMember(x => x.strName, m => m.MapFrom(n => n.strName))
                .ForMember(x => x.strSymbol, m => m.MapFrom(n => n.strSymbol))
                .ForMember(x => x.WeightNIS, m => m.MapFrom(n => n.WeightNIS))
                .ForMember(x => x.WeightUSA, m => m.MapFrom(n => n.WeightUSA));

            Mapper.CreateMap<TFI.Entities.dbo.BMsecurity, Cherries.Models.dbo.BMsecurity>()
                .ForMember(x => x.AvgYield, m => m.MapFrom(n => n.AvgYield))
                .ForMember(x => x.AvgYieldNIS, m => m.MapFrom(n => n.AvgYieldNIS))
                .ForMember(x => x.dtPriceStart, m => m.MapFrom(n => n.dtPriceStart))
                .ForMember(x => x.dtPriceEnd, m => m.MapFrom(n => n.dtPriceEnd))
                .ForMember(x => x.dValueNIS, m => m.MapFrom(n => n.MonetaryAvgNIS))
                .ForMember(x => x.dValueUSA, m => m.MapFrom(n => n.MonetaryAvg))
                .ForMember(x => x.strHebName, m => m.MapFrom(n => n.strHebName))
                .ForMember(x => x.idCurrency, m => m.MapFrom(n => n.idCurrency))
                .ForMember(x => x.idMarket, m => m.MapFrom(n => n.idMarket))
                .ForMember(x => x.idSector, m => m.MapFrom(n => n.idSector))
                .ForMember(x => x.idSecurity, m => m.MapFrom(n => n.idSecurity))
                .ForMember(x => x.idSecurityType, m => m.MapFrom(n => n.idSecurityType))
                .ForMember(x => x.sectorName, m => m.MapFrom(n => n.sectorName))
                .ForMember(x => x.marketName, m => m.MapFrom(n => n.marketName))
                .ForMember(x => x.securityTypeName, m => m.MapFrom(n => n.securityTypeName))
                //.ForMember(x => x.isActiveSecurity, m => m.MapFrom(n => n.is_Selected))
                .ForMember(x => x.StdYield, m => m.MapFrom(n => n.StdYield))
                .ForMember(x => x.StdYieldNIS, m => m.MapFrom(n => n.StdYieldNIS))
                .ForMember(x => x.strName, m => m.MapFrom(n => n.strName))
                .ForMember(x => x.strSymbol, m => m.MapFrom(n => n.strSymbol))
                .ForMember(x => x.WeightNIS, m => m.MapFrom(n => n.WeightNIS))
                .ForMember(x => x.WeightUSA, m => m.MapFrom(n => n.WeightUSA));

            Mapper.CreateMap<Entities.Lookup.Country, Cherries.Models.Lookup.Country>();
            Mapper.CreateMap<Entities.Lookup.District, Cherries.Models.Lookup.District>()
                .ForMember(x => x.idCountry, m => m.MapFrom(n => n.Country.CountryID));
            Mapper.CreateMap<Entities.Lookup.Gender, Cherries.Models.Lookup.Gender>();
            Mapper.CreateMap<Entities.dbo.PriceReturn, Cherries.Models.App.PriceReturn>();
            Mapper.CreateMap<BusinessLogic.Classes.Optimization.Backtesting.cBacktestingHandler, Cherries.Models.ViewModel.BackTestingViewModel>();
              
        }
            
    }
}
