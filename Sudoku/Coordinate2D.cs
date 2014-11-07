using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public struct Coordinate2D
    {
        public Coordinate2D(int row, int column)
            : this()
        {
            Row = row;
            Column = column;
        }

        public int Column
        {
            get;
            set;
        }

        public int Row
        {
            get;
            set;
        }

        public int FlattenedIndex(int dimension)
        {
            return Row * dimension + Column;
        }

        public static Coordinate2D FromIndex(int index, int dimension)
        {
            return new Coordinate2D(index / dimension, index % dimension);
        }

        public static IEnumerable<Coordinate2D> AllCoordinates(int dimension)
        {
            Coordinate2D[] coordinates = new Coordinate2D[dimension * dimension];
            Coordinate2D iter = new Coordinate2D();
            for (iter.Row = 0; iter.Row < dimension; iter.Row++)
            {
                for (iter.Column = 0; iter.Column < dimension; iter.Column++)
                {
                    coordinates[iter.FlattenedIndex(dimension)] = new Coordinate2D(iter.Row, iter.Column);
                }
            }
            return coordinates;
        }

        public override bool Equals(object obj)
        {
            return obj != null && (Coordinate2D)obj == this;
        }

        public static bool operator ==(Coordinate2D lhs, Coordinate2D rhs)
        {
            return lhs.Row == rhs.Row && lhs.Column == rhs.Column;
        }

        public static bool operator !=(Coordinate2D lhs, Coordinate2D rhs)
        {
            return lhs.Row != rhs.Row || lhs.Column != rhs.Column;
        }
    }
}
