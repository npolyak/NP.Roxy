using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NP.Utilities
{
    public class StopWatch
    {
        public static StopWatch TheStopWatch { get; } =
            new StopWatch();

        DateTime _startDate;
        public StopWatch()
        {
            Reset();
        }

        public void Reset()
        {
            _startDate = DateTime.Now;
        }

        public double Diff()
        {
            DateTime now = DateTime.Now;

            return now.Subtract(_startDate).TotalMilliseconds;
        }

        public string GetDiffStr(string str = null)
        {
            str = "" + str.NullToEmpty() + " " + Diff();

            return str;
        }

        public void PrintDiff(string str = null)
        {
            str = GetDiffStr(str);

            Console.WriteLine(str);
        }

        public void PrintDiffToDebug(string str = null)
        {
            str = GetDiffStr(str);

            Debug.WriteLine(str);
        }

        public static void ResetStatic()
        {
            TheStopWatch.Reset();
        }

        public static void PrintDifference(string str = null)
        {
            TheStopWatch.PrintDiff(str);
        }

        public static void PrintDifferenceToDebug(string str = null)
        {
            TheStopWatch.PrintDiffToDebug(str);
        }
    }
}
