using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cherries.Models.Queries
{
    public class LoginQuery
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string IP { get; set; }
    }
}
