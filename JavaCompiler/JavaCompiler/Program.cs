﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavaCompiler.LexicalAnalyzer;

namespace JavaCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepath = ArgsHandler(args);
            if (filepath == null)
                return;

            string file = File.ReadAllText(filepath);

            var lexAnalyzer = new LexicalAnalyzer(file);

            while (lexAnalyzer.Token != Tokens.eofT)
            {
                lexAnalyzer.GetNextToken();
                ViewTokens.Add(lexAnalyzer);
                lexAnalyzer.ResetToken();
            }

            ViewTokens.Add(lexAnalyzer);
            ViewTokens.View();

            Console.ReadKey();
        }

        /// <summary>
        /// Name: ArgsHandler
        /// Input: string[]
        /// Output: string
        /// Description: Helper function that takes in the command line arguments and 
        /// verifies the correct path, and returns the file path.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static string ArgsHandler(string[] args)
        {
            string filename = null;

            if (args.Length == 0)
            {
                Console.WriteLine("Please enter filename: ");
                filename = Console.ReadLine();
            }
            else
            {
                filename = args[0];
            }

            while (true)
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("Filename '{0}' does not exists...", filename);
                    Console.WriteLine("Please enter a correct filename (or Type 'x' to exit --> ");
                    filename = Console.ReadLine();
                    if (filename == "x")
                        return null;
                    Console.WriteLine();
                }
                else if (!filename.EndsWith(".java"))
                {
                    Console.WriteLine("Filename '{0}' is not a valid file...", filename);
                    Console.WriteLine("Please enter a filename ending in '.java' (or Type 'x' to exit --> ");
                    filename = Console.ReadLine();
                    if (filename == "x")
                        return null;
                    Console.WriteLine();
                }
                else    
                    break;
            }


            return filename;
        }
    }

}
