using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JavaCompiler.LexicalAnalyzer;
using static JavaCompiler.SymbolTable;

namespace JavaCompiler
{
    class Parser
    {
        #region Global Variables
        /// <summary>
        /// Global declaration of the Lexical Analyzer
        /// </summary>
        public LexicalAnalyzer myLex { get; set; }
        /// <summary>
        /// Global declaration of the Symbol Table
        /// </summary>
        public SymbolTable mySymTab { get; set; }
        /// <summary>
        /// Global declaration of the Depth value
        /// </summary>
        public int Depth { get; set; } = 1;
        /// <summary>
        /// Global variable that stores the variable names inside a class to be stored in the symbol table
        /// </summary>
        public List<string> VarNames { get; set; } = new List<string>();
        /// <summary>
        /// Global variable that stores the method names inside a class to be stored in the symbol table
        /// </summary>
        public List<string> MethodNames { get; set; } = new List<string>();
        /// <summary>
        /// Global variable that stores the size of the local variables either to the class or method being stored in the symbol table
        /// </summary>
        public int SizeOfLocal { get; set; } = 0;
        /// <summary>
        /// Global variable that stores the number of parameters that have been stored in the symbol table for that function
        /// </summary>
        public int NumberOfParameters { get; set; } = 0;
        /// <summary>
        /// Global variable that stores the parameter types for the function to be stored in the symbol table
        /// </summary>
        public List<Tokens> ParameterTypes { get; set; }
        /// <summary>
        /// Global variable that stores the offset for the variable to be stored in the symbol table
        /// </summary>
        public int Offset { get; set; } = 0;
        /// <summary>
        /// Global variable that stores the type of the variable to be stored in the symbol table
        /// </summary>
        public Tokens VariableType { get; set; }
        #endregion

        /// <summary>
        /// Name: Parser
        /// Input: LexicalAnalyzer
        /// Output: N/A
        /// Description: Constructor for the parser class. Takes in and initalizes the
        /// lexical analyzer and also primes the parser with the first token.
        /// </summary>
        /// <param name="lex"></param>
        public Parser(LexicalAnalyzer lex, SymbolTable symTab)
        {
            mySymTab = symTab;
            myLex = lex;
            myLex.GetNextToken(); //Prime the parser
        }

        #region Helper Methods
        /// <summary>
        /// Name: Match
        /// Input: Tokens
        /// Output: N/A
        /// Description: Takes in a desired token and compares it to the current
        /// token. If they are the same, then get next token, if not then throw
        /// and error.
        /// </summary>
        /// <param name="desired"></param>
        private void Match(Tokens desired)
        {
            if (myLex.Token == desired)
                myLex.GetNextToken();
            else
                ErrorHandler.ThrowError(desired, myLex.Token, myLex.LineNumber);
        }

        /// <summary>
        /// Helper method that increments the depth
        /// </summary>
        private void increaseDepth()
        {
            Depth++;
        }

        /// <summary>
        /// Helper method that decrements the depth and outputs everything to the user that is being removed from the symbol table
        /// </summary>
        private void decreaseDepth()
        {
            Console.WriteLine("Lexemes at Depth:" + this.Depth.ToString());
            mySymTab.WriteTable(this.Depth);
            Console.WriteLine();

            mySymTab.DeleteDepth(this.Depth);
            this.Depth--;
        }

        /// <summary>
        /// Private helper method that checks if the symbol exists prior to inserting the symbol
        /// </summary>
        private void checkForDuplicate()
        {
            TableEntry entry = new TableEntry();
            entry = mySymTab.LookUp(myLex.Lexeme);

            if(entry != null && entry.depth == Depth)
            {
                ErrorHandler.ThrowError("Duplicate Identifier", myLex.LineNumber);
            }
        }

        /// <summary>
        /// Helper method that returns the size based on the variable type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int GetSize(Tokens type)
        {
            if (type == Tokens.intT)
                return 2;
            if (type == Tokens.booleanT)
                return 1;

            return 0;
        }
        #endregion

        #region Grammar Methods
        /// <summary>
        /// Name: Prog
        /// Description: Is the beginning of the grammar that asks for classes and the main class
        /// </summary>
        public void Prog()
        {
            MoreClasses();
            MainClass();
            Match(Tokens.eofT);

            Console.WriteLine("Lexemes at Depth:" + this.Depth.ToString());
            mySymTab.WriteTable(this.Depth);
        }

        /// <summary>
        /// Name: MoreClasses
        /// Description: Implements the grammar for MoreClasses
        /// </summary>
        private void MoreClasses()
        {
            if (myLex.Token == Tokens.classT)
            {
                ClassDecl();
                MoreClasses();
            }
        }

        /// <summary>
        /// Name: MainClass
        /// Description: Implements the grammar for MainClass
        /// </summary>
        private void MainClass()
        {
            this.SizeOfLocal = 0;
            this.Offset = 0;
            this.MethodNames = new List<string>();
            this.VarNames = new List<string>();

            Match(Tokens.finalT);
            Match(Tokens.classT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            var className = myLex.Lexeme;

            Match(Tokens.idT);

            increaseDepth();

            Match(Tokens.lcurlyT);

            var classSize = this.SizeOfLocal;
            this.SizeOfLocal = 0;
            this.Offset = 0;

            Match(Tokens.publicT);
            Match(Tokens.staticT);

            var methodReturnType = myLex.Token;
            Match(Tokens.voidT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            var methodName = myLex.Lexeme;
            this.MethodNames.Add(myLex.Lexeme);

            Match(Tokens.mainT);

            increaseDepth();

            Match(Tokens.lparaT);

            Match(Tokens.StringT);
            Match(Tokens.lbracketT);
            Match(Tokens.rbracketT);
            Match(Tokens.idT);
            Match(Tokens.rparaT);
            Match(Tokens.lcurlyT);
            SeqOfStatements();

            var methodSize = this.SizeOfLocal;
            this.SizeOfLocal = 0;

            mySymTab.SetFunction(mySymTab.LookUp(methodName), methodSize, this.NumberOfParameters, methodReturnType, this.ParameterTypes);
            mySymTab.SetClass(mySymTab.LookUp(className), classSize, this.MethodNames, this.VarNames);

            decreaseDepth();

            Match(Tokens.rcurlyT);

            decreaseDepth();

            Match(Tokens.rcurlyT);

        }

        /// <summary>
        /// Name: ClassDecl
        /// Description: Implements the grammar for ClassDecl
        /// </summary>
        private void ClassDecl()
        {
            this.SizeOfLocal = 0;
            this.Offset = 0;
            this.MethodNames = new List<string>();
            this.VarNames = new List<string>();

            Match(Tokens.classT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            var className = myLex.Lexeme;

            Match(Tokens.idT);

            if (myLex.Token == Tokens.lcurlyT)
            {
                increaseDepth();

                Match(Tokens.lcurlyT);
                VarDecl();

                var classSize = this.SizeOfLocal;
                this.SizeOfLocal = 0;

                MethodDecl();

                decreaseDepth();

                Match(Tokens.rcurlyT);

                mySymTab.SetClass(mySymTab.LookUp(className), classSize, this.MethodNames, this.VarNames);
            }
            else if (myLex.Token == Tokens.extendsT)
            {
                Match(Tokens.extendsT);
                Match(Tokens.idT);

                increaseDepth();

                Match(Tokens.lcurlyT);
                VarDecl();

                var classSize = this.SizeOfLocal;
                this.SizeOfLocal = 0;

                MethodDecl();

                decreaseDepth();

                Match(Tokens.rcurlyT);

                mySymTab.SetClass(mySymTab.LookUp(className), classSize, this.MethodNames, this.VarNames);
            }
            else
            {
                ErrorHandler.ThrowError("Missing class declaration", myLex.LineNumber);
            }
        }

        /// <summary>
        /// Name: VarDecl
        /// Description: Implements the grammar for VarDecl
        /// </summary>
        private void VarDecl()
        {
            //this.SizeOfLocal = 0;

            if (myLex.Token == Tokens.finalT)
            {
                Match(Tokens.finalT);

                var varType = myLex.Token;
                var size = this.GetSize(varType);
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var varName = myLex.Lexeme;
                this.VarNames.Add(varName);

                Match(Tokens.idT);
                Match(Tokens.assignopT);

                var value = myLex.Lexeme;
                Match(Tokens.numT);
                Match(Tokens.semiT);

                this.SizeOfLocal += size;
                this.VarNames.Add(varName);
                mySymTab.SetConstant(mySymTab.LookUp(varName), varType, this.Offset, size, value);
                this.Offset += size;

                VarDecl();
            }
            else if (Types.Contains(myLex.Token))
            {
                this.VariableType = myLex.Token;
                Type();

                if (myLex.Token == Tokens.idT)
                    IdentifierList();
                else
                    ErrorHandler.ThrowError(Tokens.idT, myLex.Token, myLex.LineNumber);
                Match(Tokens.semiT);

                VarDecl();
            }
        }

        /// <summary>
        /// Name: IdentifierList
        /// Description: Implements the grammar for IdentifierList
        /// </summary>
        private void IdentifierList()
        {
            var size = this.GetSize(this.VariableType);

            if (myLex.Token == Tokens.idT)
            {
                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);

                this.SizeOfLocal += size;
                this.VarNames.Add(myLex.Lexeme);
                mySymTab.SetVariable(mySymTab.LookUp(myLex.Lexeme), this.VariableType, this.Offset, size);
                this.Offset += size;

                Match(Tokens.idT);
                IdentifierList();
            }
            else if (myLex.Token == Tokens.commaT)
            {
                Match(Tokens.commaT);

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);

                this.SizeOfLocal += size;
                this.VarNames.Add(myLex.Lexeme);
                mySymTab.SetVariable(mySymTab.LookUp(myLex.Lexeme), this.VariableType, this.Offset, size);
                this.Offset += size;

                Match(Tokens.idT);
                IdentifierList();
            }
        }

        /// <summary>
        /// A private list of names of data types
        /// </summary>
        private List<Tokens> Types = new List<Tokens>() { Tokens.intT, Tokens.booleanT, Tokens.StringT, Tokens.realT, Tokens.voidT };

        /// <summary>
        /// Name: Type
        /// Description: Implements the grammar for Type
        /// </summary>
        private void Type()
        {
            Tokens varType = myLex.Token;

            if (myLex.Token == Tokens.intT)
            {
                Match(Tokens.intT);
            }
            else if (myLex.Token == Tokens.booleanT)
            {
                Match(Tokens.booleanT);
            }
            else if (myLex.Token == Tokens.realT)
            {
                Match(Tokens.realT);
            }
            else if (myLex.Token == Tokens.voidT)
            {
                Match(Tokens.voidT);
            }
            else if (myLex.Token == Tokens.StringT)
            {
                Match(Tokens.StringT);
            }
            else
            {
                ErrorHandler.ThrowError("Type declaration expected", myLex.LineNumber);
            }
        }

        /// <summary>
        /// Name: MethodDecl
        /// Description: Implements the grammar for MethodDecl
        /// </summary>
        private void MethodDecl()
        {
            this.SizeOfLocal = 0;
            this.NumberOfParameters = 0;
            this.Offset = 0;
            this.ParameterTypes = new List<Tokens>();

            if(myLex.Token == Tokens.publicT)
            {
                Match(Tokens.publicT);

                var returnType = myLex.Token;
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var methodName = myLex.Lexeme;
                this.MethodNames.Add(methodName);

                Match(Tokens.idT);

                increaseDepth();

                Match(Tokens.lparaT);
                FormalList();
                Match(Tokens.rparaT);
                Match(Tokens.lcurlyT);
                VarDecl();
                SeqOfStatements();
                Match(Tokens.returnT);
                Expr();
                Match(Tokens.semiT);

                decreaseDepth();

                Match(Tokens.rcurlyT);

                mySymTab.SetFunction(mySymTab.LookUp(methodName), this.SizeOfLocal, this.NumberOfParameters, returnType, this.ParameterTypes);

                MethodDecl();
            }
        }

        /// <summary>
        /// Name: FormalList
        /// Description: Implements the grammar for FormalList
        /// </summary>
        private void FormalList()
        {
            if (Types.Contains(myLex.Token))
            {
                this.NumberOfParameters++;
                this.VariableType = myLex.Token;
                var size = this.GetSize(this.VariableType);
                this.ParameterTypes.Add(this.VariableType);
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var paraName = myLex.Lexeme;
                this.VarNames.Add(paraName);

                Match(Tokens.idT);

                this.SizeOfLocal += size;
                mySymTab.SetVariable(mySymTab.LookUp(paraName), this.VariableType, this.Offset, size);
                this.Offset += size;

                FormalRest();
            }
        }

        /// <summary>
        /// Name: FormalRest
        /// Description: Implements the grammar for FormalRest
        /// </summary>
        private void FormalRest()
        {
            if (myLex.Token == Tokens.commaT)
            {
                Match(Tokens.commaT);
                this.NumberOfParameters++;
                this.VariableType = myLex.Token;
                var size = this.GetSize(this.VariableType);
                this.ParameterTypes.Add(this.VariableType);
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var paraName = myLex.Lexeme;
                this.VarNames.Add(paraName);

                Match(Tokens.idT);

                this.SizeOfLocal += size;
                mySymTab.SetVariable(mySymTab.LookUp(paraName), this.VariableType, this.Offset, size);
                this.Offset += size;

                FormalRest();
            }
        }

        /// <summary>
        /// Name: SeqOfStatements
        /// Description: Implements the grammar for SequenceOfStatements
        /// </summary>
        private void SeqOfStatements()
        {
            return;
        }

        /// <summary>
        /// Name: Expr
        /// Description: Implements the grammar for Expression
        /// </summary>
        private void Expr()
        {
            return;
        }
        #endregion
    }
}
