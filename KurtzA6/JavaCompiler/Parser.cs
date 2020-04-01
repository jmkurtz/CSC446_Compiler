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

            if (entry != null && entry.depth == Depth)
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

            if (myLex.Token == Tokens.publicT)
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
        /// Description: Implements grammer rules - SeqOfStatments -> Statement ; StatTail | empty
        /// </summary>
        private void SeqOfStatements()
        {
            if (myLex.Token == Tokens.idT)
            {
                Statement();
                Match(Tokens.semiT);
                StatTail();
            }
            return;
        }

        /// <summary>
        /// Name: StatTail
        /// Description: Implements grammer rules - StatTail -> Statement ; StatTail | empty
        /// </summary>
        private void StatTail()
        {
            if (myLex.Token == Tokens.idT)
            {
                Statement();
                Match(Tokens.semiT);
                StatTail();
            }
            return;
        }

        /// <summary>
        /// Name: Statement
        /// Description: Implements grammer rules - Statement -> AssignStat | IOStat
        /// </summary>
        private void Statement()
        {
            if (myLex.Token == Tokens.idT)
            {
                AssignStat();
            }
            else
            {
                IOStat();
            }
            return;
        }

        /// <summary>
        /// Name: AssignStat
        /// Description: Implements grammer rules - AssignStat -> idt = Expr
        /// </summary>
        private void AssignStat()
        {
            Match(Tokens.idT);
            Match(Tokens.assignopT);
            Expr();
            return;
        }

        /// <summary>
        /// Name: IOStat
        /// Description: Implements grammer rules - IOStat -> empty
        /// </summary>
        private void IOStat()
        {
            return;
        }

        /// <summary>
        /// Name: Expr
        /// Description: Implements the grammar rules - Relation | empty
        /// </summary>
        private void Expr()
        {
            if (myLex.Token == Tokens.idT || myLex.Token == Tokens.numT || myLex.Token == Tokens.lparaT || myLex.Token == Tokens.notT || myLex.Token == Tokens.addopT || myLex.Token == Tokens.trueT || myLex.Token == Tokens.falseT)
            {
                Relation();
            }
            return;
        }

        /// <summary>
        /// Name: Relation
        /// Description: Implements grammer rules - Relation -> SimpleExpr
        /// </summary>
        private void Relation()
        {
            SimpleExpr();
            return;
        }

        /// <summary>
        /// Name: SimpleExpr
        /// Description: Implements grammer rules - SimpleExpr -> Term MoreTerm
        /// </summary>
        private void SimpleExpr()
        {
            Term();
            MoreTerm();
            return;
        }

        /// <summary>
        /// Name: Term
        /// Description: Implements grammer rules - Term -> Factor MoreFactor
        /// </summary>
        private void Term()
        {
            Factor();
            MoreFactor();
            return;
        }

        /// <summary>
        /// Name: MoreTerm
        /// Description: Implements grammer rules - MoreTerm -> Addop Term MoreTerm | empty
        /// </summary>
        private void MoreTerm()
        {
            if (myLex.Token == Tokens.addopT)
            {
                Addop();
                Term();
                MoreTerm();
            }
            return;
        }

        /// <summary>
        /// Name: Factor
        /// Description: Implements grammer rules - Factor -> id | num | ( Expr ) | ! Factor | true | false
        /// </summary>
        private void Factor()
        {
            if (myLex.Token == Tokens.idT)
                Match(Tokens.idT);
            else if (myLex.Token == Tokens.numT)
                Match(Tokens.numT);
            else if (myLex.Token == Tokens.lparaT)
            {
                Match(Tokens.lparaT);
                Expr();
                Match(Tokens.rparaT);
            }
            else if (myLex.Token == Tokens.notT)
            {
                Match(Tokens.notT);
                Factor();
            }
            else if (myLex.Token == Tokens.addopT)
            {
                SignOp();
                Factor();
            }
            else if (myLex.Token == Tokens.trueT)
                Match(Tokens.trueT);
            else if (myLex.Token == Tokens.falseT)
                Match(Tokens.falseT);

            return;
        }

        /// <summary>
        /// Name: MoreFactor
        /// Description: Implements grammer rules - MoreFactor -> Mulop Factor MoreFactor | empty
        /// </summary>
        private void MoreFactor()
        {
            if (myLex.Token == Tokens.mulopT)
            {
                Mulop();
                Factor();
                MoreFactor();
            }
            return;
        }

        /// <summary>
        /// Name: Addop
        /// Description: Implements grammer rules - Addop -> + | - | ||
        /// </summary>
        private void Addop()
        {
            if(myLex.Token == Tokens.addopT)
            {
                if(myLex.Lexeme == "+")
                    Match(Tokens.addopT);
                else if (myLex.Lexeme == "-")
                    Match(Tokens.addopT);
                else if (myLex.Lexeme == "||")
                    Match(Tokens.addopT);
            }
            return;
        }

        /// <summary>
        /// Name: Mulop
        /// Description: Implements grammer rules - Mulop -> * | / | &&
        /// </summary>
        private void Mulop()
        {
            if (myLex.Token == Tokens.mulopT)
            {
                if (myLex.Lexeme == "*")
                    Match(Tokens.mulopT);
                else if (myLex.Lexeme == "/")
                    Match(Tokens.mulopT);
                else if (myLex.Lexeme == "&&")
                    Match(Tokens.mulopT);
            }
            return;
        }

        /// <summary>
        /// Name: SignOp 
        /// Description: Implements grammer rules - SignOp -> -
        /// </summary>
        private void SignOp()
        {
            Match(Tokens.addopT);
            return;
        }
        #endregion
    }
}
