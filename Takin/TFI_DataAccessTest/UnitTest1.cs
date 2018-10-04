using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ness.DataAccess.Fluent;
using NHibernate.Linq;
using System.Linq;
using TFI.Entities.dbo;
using Ness.Utils;

namespace TFI_DataAccessTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var enc = EncryptionHelper.EncryptAES("dtdis646");
        }
    }
}
