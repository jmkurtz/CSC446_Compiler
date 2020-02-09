using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavaCompiler.LexicalAnalyzer;

namespace JavaCompiler
{
    static class ViewTokens
    {
        public static List<TokenObject> AllTokens = new List<TokenObject>();

        /// <summary>
        /// Name: Add
        /// Input: LexicalAnalyzer
        /// Output: N/A
        /// Description: Adds the token to a list of all tokens, for the soul purpose of being
        /// displayed for Assignment 1
        /// </summary>
        /// <param name="lex"></param>
        public static void Add(LexicalAnalyzer lex)
        {
            if(lex.Token == Tokens.emptyT) 
                return;

            AllTokens.Add(new TokenObject
            {
                token = lex.Token,
                lexeme = lex.Lexeme,
                value = lex.Value,
                valueR = lex.ValueR,
                lineNumber = lex.LineNumber,
                literal = lex.Literal
            });
        }

        /// <summary>
        /// Name: View
        /// Input: N/A
        /// Output: N/A
        /// Description: A view function to display the contents for Assignment 1
        /// </summary>
        public static void View()
        {
            int i = 0;
            Console.WriteLine("{0, -10} | {1,-20} | {2,-5} | {3,-7} | {4,-7} | {5,-5}", "TOKEN", "LEXEME", "VALUE", "VALUER", "LITERAL", "LINE NUMBER");
            Console.WriteLine(new string('_', 70));
            Console.WriteLine();
            foreach(var t in AllTokens)
            {
                string v = t.value == 0 ? "-" : t.value.ToString();
                string vR = t.valueR == 0 ? "-" : t.valueR.ToString();
                string l = t.literal == "" ? "-" : t.literal;

                Console.WriteLine("{0,-10} | {1,-20} | {2,-5} | {3,-7} | {4,-7} | {5,-5}", t.token.ToString(), t.lexeme.ToString(), v, vR, l, t.lineNumber);

                if (i > 15)
                {
                    Console.ReadKey();
                    i = 0;
                }
                else
                    i++;
            }

            Console.WriteLine();
            Console.WriteLine("Finished");
        }

    }

    class TokenObject
    {
        public Tokens token;
        public string lexeme;
        public int value;
        public double valueR;
        public int lineNumber;
        public string literal;
    }
}
