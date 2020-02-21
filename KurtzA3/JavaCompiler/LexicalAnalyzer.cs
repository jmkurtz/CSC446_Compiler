using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JavaCompiler
{
    public class LexicalAnalyzer
    {
        /// <summary>
        /// Public global variable of type Token that is defaulted to an Unknown token
        /// </summary>
        public Tokens Token { get; set; } = Tokens.emptyT;

        /// <summary>
        /// Public global string that stores the contents of the token
        /// </summary>
        public string Lexeme { get; set; } = null;

        /// <summary>
        /// Public global integer that stores the integer value of an integer value
        /// </summary>
        public int Value { get; set; } = 0;

        /// <summary>
        /// Public global double that stores the real value of a real number
        /// </summary>
        public double ValueR { get; set; } = 0;

        /// <summary>
        /// Public global variable stores the literal value
        /// </summary>
        public string Literal { get; set; } = "";

        /// <summary>
        /// Public global variable stores the current line number
        /// </summary>
        public int LineNumber { get; set; } = 1;

        /// <summary>
        /// Private global variable to store the file string
        /// </summary>
        private string file { get; set; }

        /// <summary>
        /// Private global variable to refer to the current location in the file string
        /// </summary>
        private int location { get; set; } = 0;

        /// <summary>
        /// Public enum that stores all the possible tokens needed
        /// </summary>
        public enum Tokens
        {
            classT, //Class
            publicT, //Public
            staticT, //Static
            voidT, //Void
            mainT, //Main
            StringT, //String
            noteqT, //Not Equals
            extendsT, //Extends
            returnT, //Return
            intT, //Integer
            booleanT, //Boolean
            realT, //Real
            ifT, //If
            elseT, //Else
            idT, //Identifier
            whileT, //While
            printT, //Print.out.println
            lengthT, //Length
            trueT, //True
            falseT, //False
            thisT, //This
            newT, //New
            addopT, //Add Operator
            mulopT, //Multiply Operator
            commaT, //Comma
            rbracketT, //Right Bracket
            lbracketT, //Left Bracket
            strlitT, //String Literal
            rparaT, //Right Parenthesis
            lparaT, //Left Parenthesis
            rcurlyT, //Right Curly Bracket
            lcurlyT, //Left Curly Bracket
            semiT, //Semi Colon
            assignopT, //Equals
            eofT, //End of File
            relopT, //Relative Operatore
            numT, //Number
            unknownT, //Unknown
            periodT, //Period
            quoteT, //Quotations
            emptyT, //Inital Token
            finalT //Final Token
        }
        #region Assignment 1
        /// <summary>
        /// Name: LexicalAnalyzer
        /// Input: string
        /// Output: N/A
        /// Description: Constructor of the class. Takes in the file string 
        /// and sets it to private global variable.
        /// </summary>
        /// <param name="lines"></param>
        public LexicalAnalyzer(string lines)
        {
            file = lines;
        }

        /// <summary>
        /// Name: ResetToken
        /// Input: N/A
        /// Output: N/A
        /// Description: Helper function for the driver to clear the global 
        /// variable prior to moving on to the next token.
        /// </summary>
        public void ResetToken()
        {
            if(Token != Tokens.eofT)
                Token = Tokens.emptyT;
            Lexeme = "";
            Value = 0;
            ValueR = 0;
            Literal = "";
        }

        /// <summary>
        /// Name: GetNextToken
        /// Input: N/A
        /// Output: N/A
        /// Description: This function gets the next token in the file string and 
        /// sets the Token, Lexeme, Value, ValueR global variables.
        /// </summary>
        public void GetNextToken()
        {
            ResetToken();
            if(NextNotWhiteSpace())
                ProcessToken();
        }

        /// <summary>
        /// Name: NextNotWhiteSpace
        /// Input: N/A
        /// Output: bool
        /// Description: Helper function that runs prior to process token, and it
        /// goes to the next space, handles a new line, and handles eof token
        /// </summary>
        /// <returns></returns>
        public bool NextNotWhiteSpace()
        {
            while (this.location < file.Length && Char.IsWhiteSpace(file[this.location]))
            {
                if(file[this.location] == '\n')
                {
                    this.LineNumber++;
                }
                this.location++;
            }

            if(this.location >= file.Length)
            {
                Token = Tokens.eofT;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Name: GetNextCh
        /// Input: bool
        /// Output: N/A
        /// Desciption: A private helper method to increase the Location variable, 
        /// while also checking for various senarios
        /// </summary>
        /// <param name="newLine"></param>
        private void GetNextCh(int inc = 1)
        {
            this.location += inc;
        }

        /// <summary>
        /// Name: ProcessToken
        /// Input: N/A
        /// Output: N/A
        /// Desciption: Defines the type of token that is about to be found,
        /// and uses various methods for specific token types.
        /// </summary>
        private void ProcessToken()
        {
            if (Char.IsLetter(file[this.location]))
                ProcessWordToken();
            else if (Char.IsDigit(file[this.location]))
                ProcessNumToken();
            else if (!Char.IsLetterOrDigit(file[this.location]))
                ProcessSymbolToken();
        }

        /// <summary>
        /// Name: ProcessMultiComment
        /// Input: N/A
        /// Output: N/A
        /// Description: Specifically processes multiple line comments
        /// </summary>
        private void ProcessMultiComment()
        {
            while (this.location < file.Length)
            {
                if (file[this.location] == '*' && file[this.location + 1] == '/')
                {
                    GetNextCh(2);
                    break;
                }
                else
                    GetNextCh();
            }

            GetNextToken();
        }

        /// <summary>
        /// Name: ProcessSingleComment()
        /// Input: N/A
        /// Output: N/A
        /// Description: Specifically processes single line comments
        /// </summary>
        private void ProcessSingleComment()
        {
            while (this.location < file.Length && file[this.location] != '\n')
                GetNextCh();

            GetNextToken();
        }

        /// <summary>
        /// Name: ProcessSymbolToken
        /// Input: N/A
        /// Output: N/A
        /// Description: Specifically processes tokens that begin with a symbol
        /// </summary>
        private void ProcessSymbolToken()
        {
            Lexeme += file[this.location];

            switch (Lexeme)
            {
                case "/":
                    if (CheckNext('/'))
                        ProcessSingleComment();
                    else if (CheckNext('*'))
                        ProcessMultiComment();
                    else
                    {
                        Token = Tokens.mulopT;
                        GetNextCh();
                        Lexeme = "/";
                    }
                    break;
                case "=":
                    if (CheckNext('='))
                    {
                        Token = Tokens.relopT;
                        GetNextCh(2);
                        Lexeme = "==";
                    }
                    else
                    {
                        Token = Tokens.assignopT;
                        GetNextCh();
                        Lexeme = "=";
                    }
                    break;
                case ">":
                    if (CheckNext('='))
                    {
                        Token = Tokens.relopT;
                        GetNextCh(2);
                        Lexeme = ">=";
                    }
                    else
                    {
                        Token = Tokens.relopT;
                        GetNextCh();
                        Lexeme = ">";
                    }
                    break;
                case "<":
                    if (CheckNext('='))
                    {
                        Token = Tokens.relopT;
                        GetNextCh(2);
                        Lexeme = "<=";
                    }
                    else
                    {
                        Token = Tokens.relopT;
                        GetNextCh();
                        Lexeme = "<";
                    }
                    break;
                case "!":
                    if (CheckNext('='))
                    {
                        Token = Tokens.relopT;
                        GetNextCh(2);
                        Lexeme = "!=";
                    }
                    else
                    {
                        Token = Tokens.unknownT;
                        GetNextCh();
                        Lexeme = "!";
                    }
                    break;
                case "&":
                    if (CheckNext('&'))
                    {
                        Token = Tokens.mulopT;
                        GetNextCh(2);
                        Lexeme = "&&";
                    }
                    else
                    {
                        Token = Tokens.unknownT;
                        GetNextCh();
                        Lexeme = "&";
                    }
                    break;
                case "|":
                    if (CheckNext('|'))
                    {
                        Token = Tokens.addopT;
                        GetNextCh(2);
                        Lexeme = "||";
                    }
                    else
                    {
                        Token = Tokens.unknownT;
                        GetNextCh();
                        Lexeme = "|";
                    }
                    break;
                case "(":
                    Token = Tokens.lparaT;
                    GetNextCh();
                    break;
                case ")":
                    Token = Tokens.rparaT;
                    GetNextCh();
                    break;
                case "[":
                    Token = Tokens.lbracketT;
                    GetNextCh();
                    break;
                case "]":
                    Token = Tokens.rbracketT;
                    GetNextCh();
                    break;
                case "{":
                    Token = Tokens.lcurlyT;
                    GetNextCh();
                    break;
                case "}":
                    Token = Tokens.rcurlyT;
                    GetNextCh();
                    break;
                case ";":
                    Token = Tokens.semiT;
                    GetNextCh();
                    break;
                case ",":
                    Token = Tokens.commaT;
                    GetNextCh();
                    break;
                case ".":
                    Token = Tokens.periodT;
                    GetNextCh();
                    break;
                case "+":
                    Token = Tokens.addopT;
                    GetNextCh();
                    break;
                case "-":
                    Token = Tokens.addopT;
                    GetNextCh();
                    break;
                case "*":
                    Token = Tokens.mulopT;
                    GetNextCh();
                    break;
                case "\"":
                    ProcessLiteralToken();
                    GetNextCh();
                    break;
                default:
                    Token = Tokens.unknownT;
                    GetNextCh();
                    break;
            }
        }

        /// <summary>
        /// Name: CheckNextToken
        /// Input: char v
        /// Output: bool
        /// Descprition: Returns true if the next element equals the passed in character.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool CheckNext(char v)
        {
            if (this.location + 1 < file.Length)
                if (file[this.location + 1] == v)
                    return true;

            return false;
        }


        /// <summary>
        /// Name: ProcessNumToken
        /// Input: N/A
        /// Output: N/A
        /// Description: Specifically processes tokens that begin with digits
        /// </summary>
        private void ProcessNumToken()
        {
            while (Char.IsDigit(file[this.location]))
            {
                Lexeme += file[this.location];
                GetNextCh();
            }

            if (file[this.location] == '.')
            {
                Lexeme += file[this.location];
                GetNextCh();

                if (!Char.IsDigit(file[this.location]))
                {
                    Token = Tokens.unknownT;
                    return;
                }

                while (Char.IsDigit(file[this.location]))
                {
                    Lexeme += file[this.location];
                    GetNextCh();
                }

                ValueR = Convert.ToDouble(Lexeme);
            }
            else
                Value = Convert.ToInt32(Lexeme);

            Token = Tokens.numT;
        }

        /// <summary>
        /// Name: ProcessLiteralToken
        /// Input: N/A
        /// Output: N/A
        /// Description: Handles a literal token
        /// </summary>
        private void ProcessLiteralToken()
        {
            GetNextCh();

            while (file[this.location] != '"' && file[this.location] != '\n' && Token != Tokens.eofT)
            {
                Lexeme += file[this.location];
                GetNextCh();
            }

            if (Token != Tokens.eofT)
            {
                if (file[this.location] == '"')
                {
                    Lexeme += file[this.location];
                    Token = Tokens.strlitT;
                    Literal = Lexeme;
                }
                else
                {
                    Token = Tokens.unknownT;
                }
            }
                
            return;
        }

        /// <summary>
        /// Name: ProcessWordToken
        /// Input: N/A
        /// Output: N/A
        /// Desciption: Specifically processes word tokens
        /// </summary>
        private void ProcessWordToken()
        {
            while (Char.IsLetterOrDigit(file[this.location]) || file[this.location] == '_')
            {
                Lexeme += file[this.location];
                GetNextCh();
            }

            if(Lexeme == "System")
                while(Char.IsLetterOrDigit(file[this.location]) || file[this.location] == '_' || file[this.location] == '.')
                {
                    Lexeme += file[this.location];
                    GetNextCh();
                }

            switch (Lexeme)
            {
                case "class":
                    Token = Tokens.classT;
                    break;
                case "public":
                    Token = Tokens.publicT;
                    break;
                case "static":
                    Token = Tokens.staticT;
                    break;
                case "void":
                    Token = Tokens.voidT;
                    break;
                case "main":
                    Token = Tokens.mainT;
                    break;
                case "String":
                    Token = Tokens.StringT;
                    break;
                case "extends":
                    Token = Tokens.extendsT;
                    break;
                case "return":
                    Token = Tokens.returnT;
                    break;
                case "int":
                    Token = Tokens.intT;
                    break;
                case "boolean":
                    Token = Tokens.booleanT;
                    break;
                case "if":
                    Token = Tokens.ifT;
                    break;
                case "else":
                    Token = Tokens.elseT;
                    break;
                case "while":
                    Token = Tokens.whileT;
                    break;
                case "System.out.println":
                    Token = Tokens.printT;
                    break;
                case "length":
                    Token = Tokens.lengthT;
                    break;
                case "true":
                    Token = Tokens.trueT;
                    break;
                case "false":
                    Token = Tokens.falseT;
                    break;
                case "this":
                    Token = Tokens.thisT;
                    break;
                case "new":
                    Token = Tokens.newT;
                    break;
                case "real":
                    Token = Tokens.realT;
                    break;
                case "final":
                    Token = Tokens.finalT;
                    break;
                default:
                    Token = Lexeme.Length > 31 ? Tokens.unknownT : Tokens.idT;
                    break;
            }
        }
        #endregion

       
    }
}
