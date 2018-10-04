using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IPriceHandler : IBaseBL
    {
        //void setPortfolioPricesFromDB();
        List<Rate> GetPrices(string secId, string currency);
        Rate GetPrice(string secId, string currency, DateTime date);
    }
}
