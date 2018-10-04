using Cherries.TFI.BusinessLogic.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IConstraints : IBaseBL
    {
        void refreshConstList();
        void setConstraintValues(cSecurities cSecsCol);

    }
}
