using Sample.Examples;
using System;
using System.Text;

namespace Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ClassBuildExample.Run();
        }
    }
}
