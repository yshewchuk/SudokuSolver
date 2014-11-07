using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public struct SudokuCell
    {
        public SudokuBoard.Symbol Value
        {
            get;
            set;
        }

        public Coordinate2D Location
        {
            get;
            set;
        }
    }

    public class SudokuBoard : IEnumerable<SudokuCell>
    {
        public enum Symbol
        {
            [Description(".")]
            UNKNOWN = 0x0000,
            [Description("1")]
            ONE = 0x0001,
            [Description("2")]
            TWO = 0x0002,
            [Description("3")]
            THREE = 0x0004,
            [Description("4")]
            FOUR = 0x0008,
            [Description("5")]
            FIVE = 0x0010,
            [Description("6")]
            SIX = 0x0020,
            [Description("7")]
            SEVEN = 0x0040,
            [Description("8")]
            EIGHT = 0x0080,
            [Description("9")]
            NINE = 0x0100
        }

        public static int Dimension = 9;
        public static int BoxDimension = 3;

        public static String SymbolString(Symbol value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static Symbol StringSymbol(String value)
        {
            foreach (Symbol symbol in Enum.GetValues(typeof(Symbol)))
            {
                if (value.Equals(SymbolString(symbol)))
                {
                    return symbol;
                }
            }

            return Symbol.UNKNOWN;
        }

        public SudokuBoard()
        {
            _values = new Symbol[81];
        }

        public SudokuBoard(SudokuBoard other)
        {
            _values = new Symbol[81];
            for (int i = 0; i < 81; i++)
            {
                _values[i] = other._values[i];
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || (obj as SudokuBoard) == null)
            {
                return false;
            }

            SudokuBoard otherBoard = obj as SudokuBoard;
            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i] != otherBoard._values[i]
                    && _values[i] != Symbol.UNKNOWN
                    && otherBoard._values[i] != Symbol.UNKNOWN)
                {
                    return false;
                }
            }

            return true;
        }

        private Symbol[] _values;
        public Symbol this[Coordinate2D pos]
        {
            get
            {
                return _values[pos.FlattenedIndex(Dimension)];
            }

            set
            {
                _values[pos.FlattenedIndex(Dimension)] = value;
            }
        }

        public override string ToString()
        {
            char[] str = new char[Dimension * Dimension];
            for (int i = 0; i < _values.Length; i++)
            {
                str[i] = SymbolString(_values[i])[0];
            }
            return new String(str);
        }

        public static SudokuBoard FromString(String str)
        {
            SudokuBoard board = new SudokuBoard();
            for (int i = 0; i < Dimension * Dimension; i++)
            {
                board._values[i] = StringSymbol(str.Substring(i, 1));
            }
            return board;
        }

        public IEnumerator<SudokuCell> GetEnumerator()
        {
            return _values.Select((v, i) => new SudokuCell() { Value = v, Location = Coordinate2D.FromIndex(i, Dimension)}).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
