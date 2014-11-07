using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class BoardGenerator
    {
        public static SudokuBoard Minimize(SudokuBoard board)
        {
            InductiveSolver solver = new InductiveSolver();

            SudokuBoard min = new SudokuBoard(board);
            int nMinCount = SudokuBoard.Dimension * SudokuBoard.Dimension;

            for (int i = 0; i < 100; i++)
            {
                int nCount = 0;
                SudokuBoard result = new SudokuBoard(board);
                IEnumerable<Coordinate2D> coords = Coordinate2D.AllCoordinates(SudokuBoard.Dimension).Shuffle();

                foreach (Coordinate2D coord in coords)
                {
                    result[coord] = SudokuBoard.Symbol.UNKNOWN;
                    SudokuBoard solution = solver.Solve(result);
                    if (!board.Equals(solution)
                        || coords.Any(c => solution[c] == SudokuBoard.Symbol.UNKNOWN))
                    {
                        result[coord] = board[coord];
                        nCount++;
                    }
                }

                if (nCount < nMinCount)
                {
                    min = result;
                    nMinCount = nCount;
                }
            }

            return min;
        }
    }
}
