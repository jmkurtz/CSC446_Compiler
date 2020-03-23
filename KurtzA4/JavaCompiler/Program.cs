using System;
using System.Collections;
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
        /// <summary>
        /// 
        /// PROJECT: Java Compiler 
        /// Assignment: #2
        /// Name: Jeff Kurtz
        /// Description: Takes in java code following the implemented grammer
        /// and returns an error message if the code does not follow the grammer
        /// rules.
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var filepath = ArgsHandler(args);
            if (filepath == null)
                return;

            var file = File.ReadAllText(filepath);

            var parser = new Parser(new LexicalAnalyzer(file), new SymbolTable());
            parser.Prog();

            //var symTab = new SymbolTable();
            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("INSERT:");
            //Console.WriteLine("----------------------------------");
            //symTab.Insert("id1", Tokens.idT, 1);
            //symTab.Insert("id2", Tokens.idT, 1);
            //symTab.Insert("id3", Tokens.idT, 2);
            //symTab.Insert("id4", Tokens.idT, 2);
            //symTab.Insert("id5", Tokens.idT, 3);
            //symTab.Insert("id6", Tokens.idT, 3);
            //symTab.Insert("id7", Tokens.idT, 1);
            //symTab.Insert("id7", Tokens.idT, 1);
            //symTab.Insert("id7", Tokens.idT, 1);
            //symTab.Insert("id7", Tokens.idT, 1);
            //symTab.Insert("id7", Tokens.idT, 1);
            //symTab.Insert("id8", Tokens.idT, 2);
            //symTab.Insert("id9", Tokens.idT, 3);
            //symTab.Insert("id10", Tokens.idT, 2);
            //symTab.Insert("id10", Tokens.idT, 3);

            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("LOOK UP: ");
            //Console.WriteLine("----------------------------------");
            //var id2 = symTab.LookUp("id2");
            //Console.WriteLine("Look up id2 -> {0}", id2.Lexeme);

            //var id9 = symTab.LookUp("id9");
            //Console.WriteLine("Look up id9 -> {0}", id9.Lexeme);

            //var id15 = symTab.LookUp("id15");
            //Console.WriteLine("Look up id2 -> {0}", id15 != null ? id15.Lexeme : "DNE");

            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("WRITE TABLE");
            //Console.WriteLine("----------------------------------");
            //symTab.WriteTable(2);

            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("DELETE TABLE");
            //Console.WriteLine("----------------------------------");
            //symTab.DeleteDepth(2);

            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("WRITE DELETED TABLE");
            //Console.WriteLine("----------------------------------");
            //symTab.WriteTable(2);

            //Console.WriteLine("----------------------------------");
            //Console.WriteLine("WRITE TABLE");
            //Console.WriteLine("----------------------------------");
            //symTab.WriteTable(1);


            //Console.ReadKey();
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
