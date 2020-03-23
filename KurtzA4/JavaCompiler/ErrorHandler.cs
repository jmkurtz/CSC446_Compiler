using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavaCompiler.LexicalAnalyzer;

namespace JavaCompiler
{
    public static class ErrorHandler
    {
        public static string Error_Start = "ERROR: ";
        public static string Error_Expected = "Expected = ";
        public static string Error_Actual = ", Actual = ";
        public static string Error_End = ", on Line ";

        public static void ThrowError(Tokens expected, Tokens actual, int lineNo)
        {
            switch (expected)
            {
                case Tokens.semiT:
                    Console.WriteLine(Error_Start + "Missing semi colon" + Error_End + lineNo);
                    break;
                case Tokens.finalT:
                    Console.WriteLine(Error_Start + "Missing main class definition");
                    break;
                case Tokens.publicT:
                    Console.WriteLine(Error_Start + "Expected main class function");
                    break;
                default:
                    switch (actual)
                    {
                        case Tokens.eofT:
                            Console.WriteLine(Error_Start + "Unexpected End of File token" + Error_End + lineNo);
                            break;
                        default:
                            Console.WriteLine(Error_Start + Error_Expected + expected.ToString()
                                + Error_Actual + actual.ToString() + Error_End + lineNo);
                            break;
                    }
                    break;
            }

            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void ThrowError(string message, int lineNo)
        {
            Console.WriteLine(Error_Start + message + Error_End + lineNo);
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
