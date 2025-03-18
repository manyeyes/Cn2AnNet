using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cn2AnNet
{
    public class Performance
    {
        private static An2Cn ac = new An2Cn();
        private static Cn2An ca = new Cn2An();
        private static double an = 9876543298765432;
        private static string cn = "九千八百七十六万五千四百三十二亿九千八百七十六万五千四百三十二";

        public static void RunCn2AnTenThousandTimes()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                long result = (long)ca.Cn2AnConvert(cn);
                if (result != an)
                {
                    throw new Exception("转换结果不符合预期");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"run_cn2an_ten_thousand_times 运行时间: {stopwatch.ElapsedMilliseconds} 毫秒");
        }

        public static void RunAn2CnTenThousandTimes()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10000; i++)
            {
                string result = ac.An2CnConvert(an);
                if (result != cn)
                {
                    throw new Exception("转换结果不符合预期");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"run_an2cn_ten_thousand_times 运行时间: {stopwatch.ElapsedMilliseconds} 毫秒");
        }
    }
}