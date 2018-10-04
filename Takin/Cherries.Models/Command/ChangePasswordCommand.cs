using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.Queries
{
    public class ChangePasswordCommand
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
    }
}