using Cherries.Models.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.ViewModel
{
    public class BaseViewModel
    {
        public BaseViewModel()
        {
            Messages = new List<Message>();
        }

        public List<Message> Messages { get; set; }
    }
}
