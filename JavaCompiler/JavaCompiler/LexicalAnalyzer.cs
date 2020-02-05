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
        public Tokens Token { get; set; } = Tokens.unknownT;

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
        /// Private global bool that is flipped when in a string literal
        /// </summary>
        private bool literalBool { get; set; } = false;

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
        }

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
                Token = Tokens.unknownT;
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

            if (this.location >= file.Length)
            {
                Token = Tokens.eofT;
            }
            else
            {
                while (true)
                {
                    if (this.location >= file.Length)
                        return;
                    if (file[this.location] == '\n')
                    {
                        this.location++;
                        this.LineNumber++;
                    }
                    else if (Char.IsWhiteSpace(file[this.location]))
                        this.location++;
                    else
                        break;
                }

                ProcessToken();
            }
        }

        /// <summary>
        /// Name: GetNextCh
        /// Input: bool
        /// Output: N/A
        /// Desciption: A private helper method to increase the Location variable, 
        /// while also checking for various senarios
        /// </summary>
        /// <param name="newLine"></param>
        private void GetNextCh(bool newLine = false)
        {
            if(this.location+1 > file.Length)
                Token = Tokens.eofT;
            else
                this.location++;

            if(!newLine)
            {
                if (this.location >= file.Length)
                    return;
                else if (file[this.location] == '\n')
                    this.LineNumber++;
                else
                    return;
            }
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
            char ch = file[this.location];
            char nextCh = this.location+1 >= file.Length ? '\0' : file[this.location+1];

            if (Char.IsLetter(ch) && literalBool == true)
                ProcessLiteralToken();
            else if (Char.IsLetter(ch) && literalBool != true)
                ProcessWordToken();
            else if (Char.IsDigit(ch) || (ch == '.' && Char.IsDigit(nextCh)))
                ProcessNumToken();
            else if (!Char.IsLetterOrDigit(ch))
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
            while (true)
            {
                if (file[this.location] == '*' && file[this.location + 1] == '/')
                    break;

                GetNextCh();
                if (this.location >= file.Length)
                    Token = Tokens.eofT;
            }
                
            this.location += 2;
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
            while (file[this.location] != '\n')
            {
                GetNextCh(true);
                if (this.location+1 >= file.Length)
                {
                    Token = Tokens.eofT;
                    return;
                }
                    
            }
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
            char nextCh = (this.location + 1) < file.Length ? file[this.location + 1] : '\0';
            Lexeme += file[this.location];

            if (nextCh == '/' || nextCh == '*' || nextCh == '=' || nextCh == '&' || nextCh == '|')
                Lexeme += file[this.location + 1];

            switch (Lexeme)
            {
                case "//":
                    ProcessSingleComment();
                    break;
                case "/*":
                    ProcessMultiComment();
                    break;
                case "==":
                    Token = Tokens.relopT;
                    Lexeme = "==";
                    this.location += 2;
                    break;
                case ">=":
                    Token = Tokens.relopT;
                    Lexeme = ">=";
                    this.location += 2;
                    break;
                case "<=":
                    Token = Tokens.relopT;
                    Lexeme = "<=";
                    this.location += 2;
                    break;
                case "!=":
                    Token = Tokens.noteqT;
                    Lexeme = "!=";
                    this.location += 2;
                    break;
                case "&&":
                    Token = Tokens.mulopT;
                    Lexeme = "&&";
                    this.location += 2;
                    break;
                case "||":
                    Token = Tokens.addopT;
                    Lexeme = "!=";
                    this.location += 2;
                    break;
                case "=":
                    Token = Tokens.assignopT;
                    GetNextCh();
                    break;
                case "<":
                    Token = Tokens.relopT;
                    GetNextCh();
                    break;
                case ">":
                    Token = Tokens.relopT;
                    GetNextCh();
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
                    Lexeme = "*";
                    break;
                case "/":
                    Token = Tokens.mulopT;
                    GetNextCh();
                    break;
                case "\"":
                    Token = Tokens.quoteT;
                    GetNextCh();
                    literalBool = literalBool ? false : true;
                    break;
                default:
                    Token = Tokens.unknownT;
                    GetNextCh();
                    break;
            }
        }

        /// <summary>
        /// Name: ProcessNumToken
        /// Input: N/A
        /// Output: N/A
        /// Description: Specifically processes tokens that begin with digits
        /// </summary>
        private void ProcessNumToken()
        {
            try
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
            catch
            {
                Token = Tokens.unknownT;
            }
            
        }

        private void ProcessLiteralToken()
        {
            while (file[this.location] != '"' && file[this.location] != '\n' && Token != Tokens.eofT)
            {
                Lexeme += file[this.location];
                GetNextCh();
            }

            Lexeme = Lexeme.TrimEnd();

            if (file[this.location] == '\n')
                literalBool = false;
            if (Token != Tokens.eofT)
            {
                Token = Tokens.strlitT;
                Literal = Lexeme;
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
            {
                while(Char.IsLetterOrDigit(file[this.location]) || file[this.location] == '_' || file[this.location] == '.')
                {
                    Lexeme += file[this.location];
                    GetNextCh();
                }
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
                default:
                    Token = Lexeme.Length > 31 || literalBool == true ? Tokens.unknownT : Tokens.idT;
                    break;

            }
        }
    }
}
