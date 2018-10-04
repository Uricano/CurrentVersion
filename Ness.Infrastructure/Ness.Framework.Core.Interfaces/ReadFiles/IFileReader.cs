using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Interfaces.ReadFiles
{
    public interface IFileReader
    {
        Stream GetFile(string location, string name);
    }
}
