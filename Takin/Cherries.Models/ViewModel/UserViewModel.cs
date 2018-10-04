using Cherries.Models.dbo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class UserViewModel : BaseViewModel
    {
        public User User { get; set; }
    }
}
