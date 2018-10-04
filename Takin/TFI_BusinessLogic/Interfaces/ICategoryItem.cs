using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface ICategoryItem : IBaseBL
    {
        void calculateItemWeight();
        int ID { get; set; }
        String ItemName { get; }
        ISecurities Securities { get; }

    }
}
