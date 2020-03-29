using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static JavaCompiler.LexicalAnalyzer;

namespace JavaCompiler
{
    /// <summary>
    /// Name: JEFF KURTZ
    /// Assignment: #4
    /// Description: Symbol Table
    /// </summary>
    public class SymbolTable
    {
        /// <summary>
        /// A const variable to hold the table size
        /// </summary>
        public const int TableSize = 211;

        /// <summary>
        /// Public enum that defines the variable types
        /// </summary>
        public enum VarType { intT, booleanT, StringT, realT, voidT };

        /// <summary>
        /// The private definition of the table
        /// </summary>
        private List<TableEntry>[] table;

        #region Entry Type Structs
        /// <summary>
        /// A struct to define a variable entry type
        /// </summary>
        private struct variableType
        {
            public VarType TypeOfVariable;
            public int Offset;
            public int Size;
        }
        /// <summary>
        /// A struct to define a constant entry type
        /// </summary>
        private struct constantType
        {
            public VarType TypeOfConstant;
            public int Offset;
            public int Size;
            public int Value;
            public double ValueR;
        }
        /// <summary>
        /// A struct to define a function entry type
        /// </summary>
        private struct functionType
        {
            public int SizeOfLocal;
            public int NumberOfParameters;
            public VarType ReturnType;
            public List<VarType> ParameterTypes;
        }

        /// <summary>
        /// A stuct to define a function entry type
        /// </summary>
        private struct classType
        {
            public int SizeOfLocal;
            public List<string> methodNames;
            public List<string> variableNames;
        }
        /// <summary>
        /// A class defining a table entry
        /// </summary>
        public class TableEntry
        {
            public Tokens Token;
            public string Lexeme;
            public int depth;
            public dynamic TypeOfEntry;
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// A function that allows a Token to be passed in and a Variable Type to be passed back out
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private VarType TokenToVar(Tokens token)
        {
            if (token == Tokens.intT)
                return VarType.intT;
            if (token == Tokens.booleanT)
                return VarType.booleanT;
            if (token == Tokens.StringT)
                return VarType.StringT;
            if (token == Tokens.realT)
                return VarType.realT;
            else
                return VarType.voidT;
        }

        /// <summary>
        /// A function that allows a list of Tokens to be passed in and a list of Variable Types to be passed back out
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private List<VarType> TokenToVar(List<Tokens> tokens)
        {
            var tmpList = new List<VarType>();

            foreach(var token in tokens)
            {
                tmpList.Add(TokenToVar(token));
            }

            return tmpList;
        }

        /// <summary>
        /// Hashpjw takes in a string and returns a hashed value
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int hashpjw(string s)
        {
            uint h = 0, g;

            foreach (var c in s)
            {
                h = (h << 4) + (byte)c;
                if ((g = h & 0xf0000000) != 0)
                {
                    h ^= (g >> 24);
                    h ^= g;
                }
            }

            return (int)(h % (uint)TableSize);
        }
        #endregion

        #region Public Helper Methods
        /// <summary>
        /// A function that takes in the entry, and all the data needed to be stored in that entry and is set in the table for type function
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="sizeOfLocal"></param>
        /// <param name="numberOfParameters"></param>
        /// <param name="returnType"></param>
        /// <param name="paraTypes"></param>
        public void SetFunction(TableEntry entry, int sizeOfLocal, int numberOfParameters, Tokens returnType, List<Tokens> paraTypes)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = new functionType();
                        table[i][k].TypeOfEntry.SizeOfLocal = sizeOfLocal;
                        table[i][k].TypeOfEntry.NumberOfParameters = numberOfParameters;
                        table[i][k].TypeOfEntry.ReturnType = TokenToVar(returnType);
                        table[i][k].TypeOfEntry.ParameterTypes = TokenToVar(paraTypes);
                    }
        }

        /// <summary>
        /// A function that takes in the entry, and all the data needed to be stored in that entry and is set in the table for type variable
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="varType"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SetVariable(TableEntry entry, Tokens varType, int offset, int size)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = new variableType();
                        table[i][k].TypeOfEntry.TypeOfVariable = TokenToVar(varType);
                        table[i][k].TypeOfEntry.Offset = offset;
                        table[i][k].TypeOfEntry.Size = size;
                    }
        }

        /// <summary>
        /// A function that takes in the entry, and all the data needed to be stored in that entry and is set in the table for type constant
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="varType"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        public void SetConstant(TableEntry entry, Tokens varType, int offset, int size, string value)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = new constantType();
                        table[i][k].TypeOfEntry.TypeOfConstant = TokenToVar(varType);
                        table[i][k].TypeOfEntry.Offset = offset;
                        table[i][k].TypeOfEntry.Size = size;

                        if (!value.Contains('.'))
                            table[i][k].TypeOfEntry.Value = Convert.ToInt32(value);
                        else
                            table[i][k].TypeOfEntry.ValueR = Convert.ToDouble(value);
                    }
        }

        /// <summary>
        /// A function that takes in the entry, and all the data needed to be stored in that entry and is set in the table for type class
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="SizeOfLocal"></param>
        /// <param name="MethodNames"></param>
        /// <param name="VariableNames"></param>
        public void SetClass(TableEntry entry, int SizeOfLocal, List<string> MethodNames, List<string> VariableNames)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = new classType();
                        table[i][k].TypeOfEntry.SizeOfLocal = SizeOfLocal;
                        table[i][k].TypeOfEntry.methodNames = MethodNames;
                        table[i][k].TypeOfEntry.variableNames = VariableNames;
                    }
        }
        #endregion

        #region Main Functionality Methods
        /// <summary>
        /// The contrustor of the symbol table
        /// </summary>
        public SymbolTable()
        {
            table = new List<TableEntry>[TableSize];
            for (int i = 0; i < table.Length; i++)
                table[i] = new List<TableEntry>();
        }

        /// <summary>
        /// Look up function that takes in a string and return null if that does exist
        /// </summary>
        /// <param name="lex"></param>
        /// <returns></returns>
        public TableEntry LookUp(string lex)
        {
            foreach (var t in table)
                foreach (var c in t)
                    if (c.Lexeme == lex)
                        return c;

            return null;
        }

        /// <summary>
        /// Insert function takes in a string, token, and int to insert into the symbol table
        /// </summary>
        /// <param name="lex"></param>
        /// <param name="token"></param>
        /// <param name="depth"></param>
        public void Insert(string lex, Tokens token, int depth)
        {
            int index = hashpjw(lex);
            var tmp = new TableEntry
            {
                Token = token,
                Lexeme = lex,
                depth = depth
            };

            table[index] = table[index].Prepend(tmp).ToList();
        }

        /// <summary>
        /// Outputs the table to the user
        /// </summary>
        /// <param name="depth"></param>
        public void WriteTable(int depth)
        {
            foreach (var t in table)
                foreach (var c in t)
                    if (c.depth == depth)
                        Console.WriteLine("{0} {1}", c.Lexeme, c.TypeOfEntry.ToString().Split('+')[1]);
        }

        /// <summary>
        /// Deletes all entries that exist at the specified depth
        /// </summary>
        /// <param name="depth"></param>
        public void DeleteDepth(int depth)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k].depth == depth)
                        table[i].Remove(table[i][k]);
        }
        #endregion

    }
}
