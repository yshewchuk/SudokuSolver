using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class SudokuBoardValidity
    {
        public IEnumerable<Coordinate2D> InvalidCells
        {
            get;
            set;
        }

        public IEnumerable<int> InvalidRows
        {
            get;
            set;
        }

        public IEnumerable<int> InvalidColumns
        {
            get;
            set;
        }

        public IEnumerable<Coordinate2D> InvalidBoxes
        {
            get;
            set;
        }
    }
}
