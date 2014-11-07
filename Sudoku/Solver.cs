using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public delegate void BasicEvent();

    public interface Solver
    {
        double ProcessingProgress
        {
            get;
        }

        String ProcessingStatus
        {
            get;
        }

        event BasicEvent ProcessingStateChanged;

        SudokuBoard Solve(SudokuBoard initialBoard);

        SudokuBoardValidity Validate(SudokuBoard initialBoard);
    }
}
