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
        public Tokens tmpVarType { get; set; }

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

        #region Assignment 2
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

        private void increaseDepth()
        {
            Depth++;
        }

        private void decreaseDepth()
        {
            mySymTab.WriteTable(this.Depth);
            mySymTab.DeleteDepth(this.Depth);
            this.Depth--;
        }

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
        /// Name: Prog
        /// Description: Is the beginning of the grammar that asks for classes and the main class
        /// </summary>
        public void Prog()
        {
            MoreClasses();
            MainClass();
            Match(Tokens.eofT);

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
            Match(Tokens.finalT);
            Match(Tokens.classT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
            //mySymTab.SetEntryType(tmpEntry, EntryType.classEntry);

            Match(Tokens.idT);
            Match(Tokens.lcurlyT);
            Match(Tokens.publicT);
            Match(Tokens.staticT);

            tmpVarType = myLex.Token;
            Match(Tokens.voidT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            tmpEntry = mySymTab.LookUp(myLex.Lexeme);
            //mySymTab.SetFunction(tmpEntry, EntryType.functionEntry, tmpVarType);

            Match(Tokens.mainT);

            increaseDepth();

            Match(Tokens.lparaT);

            tmpVarType = myLex.Token;
            Match(Tokens.StringT);
            Match(Tokens.lbracketT);
            Match(Tokens.rbracketT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            tmpEntry = mySymTab.LookUp(myLex.Lexeme);
            //mySymTab.SetEntryType(tmpEntry, EntryType.variableEntry);
            //mySymTab.SetVarType(tmpEntry, tmpVarType);

            Match(Tokens.idT);
            Match(Tokens.rparaT);
            Match(Tokens.lcurlyT);
            SeqOfStatements();

            decreaseDepth();

            Match(Tokens.rcurlyT);
            Match(Tokens.rcurlyT);
        }

        /// <summary>
        /// Name: ClassDecl
        /// Description: Implements the grammar for ClassDecl
        /// </summary>
        private void ClassDecl()
        {
            Match(Tokens.classT);

            checkForDuplicate();
            mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
            var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
            //mySymTab.SetEntryType(tmpEntry, EntryType.classEntry);

            Match(Tokens.idT);

            if (myLex.Token == Tokens.lcurlyT)
            {
                Match(Tokens.lcurlyT);
                VarDecl();
                MethodDecl();
                Match(Tokens.rcurlyT);
            }
            else if (myLex.Token == Tokens.extendsT)
            {
                Match(Tokens.extendsT);
                Match(Tokens.idT);
                Match(Tokens.lcurlyT);
                VarDecl();
                MethodDecl();
                Match(Tokens.rcurlyT);
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
            if (myLex.Token == Tokens.finalT)
            {
                Match(Tokens.finalT);
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.constEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);

                Match(Tokens.idT);
                Match(Tokens.assignopT);
                Match(Tokens.numT);
                Match(Tokens.semiT);
                VarDecl();
            }
            else if (Types.Contains(myLex.Token))
            {
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
            if (myLex.Token == Tokens.idT)
            {
                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.variableEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);

                Match(Tokens.idT);
                IdentifierList();
            }
            else if (myLex.Token == Tokens.commaT)
            {
                Match(Tokens.commaT);

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.variableEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);

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
            if(myLex.Token == Tokens.publicT)
            {
                Match(Tokens.publicT);
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.functionEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);

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
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.variableEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);
                //mySymTab.AddFormalListTypes(tmpEntry, myLex.Token);

                Match(Tokens.idT);
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
                Type();

                checkForDuplicate();
                mySymTab.Insert(myLex.Lexeme, myLex.Token, this.Depth);
                var tmpEntry = mySymTab.LookUp(myLex.Lexeme);
                //mySymTab.SetEntryType(tmpEntry, EntryType.variableEntry);
                //mySymTab.SetVarType(tmpEntry, tmpVarType);
                //mySymTab.AddFormalListTypes(tmpEntry, myLex.Token);

                Match(Tokens.idT);
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
