using Cherries.Models.Queries;
using Cherries.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSciEngine;

namespace TFI.CalculationEngine
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            while (true)
            {
                var json = Console.ReadLine();
                if (json == null) break;
                CalculationQuery data = Newtonsoft.Json.JsonConvert.DeserializeObject<CalculationQuery>(json); 
                //cErrorHandler m_objErrors = new cErrorHandler();
                cPrepareSmallData m_objSmallEngine;
                m_objSmallEngine = new cPrepareSmallData();
                var colRisks = new List<double>();
                var colReturns = new List<double>();
                double[,] matWeights = new double[0, 0];
                string err = m_objSmallEngine.runOptimizationProcess(out colRisks, out colReturns, out matWeights, data.MatCovariance, data.ArrReturns);
                CalculationResponse resp = new CalculationResponse
                {
                    colReturns = colReturns,
                    colRisks = colRisks,
                    matWeights = matWeights,
                    errorString = err
                };
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
                //cErrorHandler m_objErrors = new cErrorHandler();

            }
        }
    }
}
