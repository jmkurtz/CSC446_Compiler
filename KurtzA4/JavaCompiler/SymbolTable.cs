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
        /// A struct to define a variable entry type
        /// </summary>
        public struct variableType
        {
            VarType TypeOfVariable;
            int Offset;
            int Size;
        }
        /// <summary>
        /// A struct to define a constant entry type
        /// </summary>
        public struct constantType
        {
            VarType TypeOfConstant;
            int Offset;
            int Size;
            [StructLayout(LayoutKind.Explicit)]
            public class Number
            {
                [FieldOffset(0)]
                public float ValueR;

                [FieldOffset(0)]
                public int Value;
            }
        }
        /// <summary>
        /// A struct to define a function entry type
        /// </summary>
        public struct functionType
        {
            int SizeOfLocal;
            int NumberOfParameters;
            VarType ReturnType;
            List<VarType> ParameterTypes;
        }

        public struct classType
        {
            int SizeOfLocal;
            List<string> methodNames;
            List<string> variableNames;
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
        /// <summary>
        /// A enum for a type of variable
        /// </summary>
        public enum VarType { intT, booleanT, StringT, realT, voidT };

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
        /// An enum for the type of entry
        /// </summary>
        public enum EntryType { constEntry, variableEntry, functionEntry, classEntry };

        /// <summary>
        /// The private definition of the table
        /// </summary>
        private List<TableEntry>[] table;

        /// <summary>
        /// The contrustor of the symbol table
        /// </summary>
        public SymbolTable()
        {
            table = new List<TableEntry>[TableSize];
            for(int i = 0; i < table.Length; i++)
                table[i] = new List<TableEntry>();
        }

        /// <summary>
        /// Look up function that takes in a string and return null if that does exist
        /// </summary>
        /// <param name="lex"></param>
        /// <returns></returns>
        public TableEntry LookUp(string lex)
        {
            foreach(var t in table)
                foreach (var c in t)
                    if (c.Lexeme == lex)
                        return c; 

            return null;
        }

        /// <summary>
        /// Hashpjw takes in a string and returns a hashed value
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int hashpjw(string s)
        {
            uint h = 0, g;
            
            foreach(var c in s)
            {
                h = (h << 4) + (byte)c;
                if((g = h & 0xf0000000) != 0)
                {
                    h ^= (g >> 24);
                    h ^= g;
                }
            }

            return (int)(h % (uint)TableSize);
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
            foreach(var t in table)
                foreach(var c in t)
                    if(c.depth == depth)
                        Console.WriteLine("{0} {1} {2}", c.Token.ToString(), c.Lexeme, c.depth);
        }

        /// <summary>
        /// Deletes all entries that exist at the specified depth
        /// </summary>
        /// <param name="depth"></param>
        public void DeleteDepth(int depth)
        {
            for(int i = 0; i < table.Length; i++)
                for(int k = 0; k < table[i].Count; k++)
                    if(table[i][k].depth == depth)
                        table[i].Remove(table[i][k]);
        }

        public void SetFunction(TableEntry entry, EntryType entryType, int sizeOfLocal, int numberOfParameters, Tokens returnType, List<Tokens> varTypes)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = entryType;
                        table[i][k].TypeOfEntry.SizeOfLocal = sizeOfLocal;
                        table[i][k].TypeOfEntry.NumberOfParameters = numberOfParameters;
                        table[i][k].TypeOfEntry.ReturnType = TokenToVar(returnType);
                    }
        }

        public void SetVariable(TableEntry entry, EntryType entryType, Tokens varType, int offset, int size)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = entryType;
                        table[i][k].TypeOfEntry.TypeOfVariable = TokenToVar(varType);
                        table[i][k].TypeOfEntry.Offset = offset;
                        table[i][k].TypeOfEntry.Size = size;
                    }
        }

        public void SetConstant(TableEntry entry, EntryType entryType, Tokens varType, int offset, int size)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = entryType;
                        table[i][k].TypeOfEntry.TypeOfVariable = TokenToVar(varType);
                        table[i][k].TypeOfEntry.Offset = offset;
                        table[i][k].TypeOfEntry.Size = size;
                    }
        }

        public void SetClass(TableEntry entry, EntryType entryType, int SizeOfLocal, List<string> MethodNames, List<string> VariableNames)
        {
            for (int i = 0; i < table.Length; i++)
                for (int k = 0; k < table[i].Count; k++)
                    if (table[i][k] == entry)
                    {
                        table[i][k].TypeOfEntry = entryType;
                        table[i][k].TypeOfEntry.SizeOfLocal = SizeOfLocal;
                        table[i][k].TypeOfEntry.methodNames = MethodNames;
                        table[i][k].TypeOfEntry.variableNames = VariableNames;
                    }
        }
    }
}
