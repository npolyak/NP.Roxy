using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests
{
    public class XUnitTestBase
    {
        #region Redirecting Console
        private class MyTextWriter : TextWriter
        {
            ITestOutputHelper _output;
            public override Encoding Encoding => Encoding.ASCII;


            public MyTextWriter(ITestOutputHelper output)
            {
                _output = output;
            }

            public override void WriteLine(string str)
            {
                _output.WriteLine(str);
            }
        }
        MyTextWriter _myTextWriter;

        public XUnitTestBase(ITestOutputHelper testOutputHelper)
        {
            _myTextWriter = new MyTextWriter(testOutputHelper);

            Console.SetOut(_myTextWriter);
        }

        #endregion Redirecting Console

    }
}
