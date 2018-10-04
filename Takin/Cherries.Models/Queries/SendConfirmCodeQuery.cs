using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class SendConfirmCodeQuery
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
