using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku
{
    public class BruteForceSolver2 : Solver
    {
        public double ProcessingProgress
        {
            get;
            private set;
        }

        public string ProcessingStatus
        {
            get;
            private set;
        }

        private static IEnumerable<SudokuBoard.Symbol> Symbols = Enum.GetValues(typeof(SudokuBoard.Symbol)).Cast<SudokuBoard.Symbol>().Where(e => e != SudokuBoard.Symbol.UNKNOWN).ToArray();

        public event BasicEvent ProcessingStateChanged;

        public SudokuBoard Solve(SudokuBoard initialBoard)
        {
            ProcessingProgress = 0.0;
            ProcessingStatus = "Solving...";
            Progress();

            SudokuBoard solution = Solve(new SudokuBoard(initialBoard), new Coordinate2D(0, 0), 0.0, 1.0 / SudokuBoard.Dimension);

            ProcessingProgress = 1.0;
            if (solution != null)
            {
                ProcessingStatus = "Solved";
            }
            else
            {
                ProcessingStatus = "Could not solve";
            }
            Progress();

            return solution;
        }

        private SudokuBoard Solve(SudokuBoard board, Coordinate2D position, double baseProgess, double progressIncrement)
        {
            Coordinate2D nextPosition = new Coordinate2D(position.Row, position.Column);
            nextPosition.Column++;
            if (nextPosition.Column >= SudokuBoard.Dimension)
            {
                nextPosition.Column = 0;
                nextPosition.Row++;
            }
            bool terminal = nextPosition.Row >= SudokuBoard.Dimension;

            if (board[position] != SudokuBoard.Symbol.UNKNOWN)
            {
                if (terminal)
                {
                    return board;
                }
                else
                {
                    return Solve(board, nextPosition, baseProgess, progressIncrement);
                }
            }

            int nBoardFolded = 0;
            Coordinate2D iter = new Coordinate2D(position.Row, 0);
            for (iter.Column = 0; iter.Column < SudokuBoard.Dimension; iter.Column++)
            {
                nBoardFolded |= (int)board[iter];
            }
            iter.Column = position.Column;
            for (iter.Row = 0; iter.Row < SudokuBoard.Dimension; iter.Row++)
            {
                nBoardFolded |= (int)board[iter];
            }
            Coordinate2D start = new Coordinate2D((position.Row / SudokuBoard.BoxDimension) * SudokuBoard.BoxDimension, (position.Column / SudokuBoard.BoxDimension) * SudokuBoard.BoxDimension);
            for (iter.Row = start.Row; iter.Row < start.Row + SudokuBoard.BoxDimension; iter.Row++)
            {
                for (iter.Column = start.Column; iter.Column < start.Column + SudokuBoard.BoxDimension; iter.Column++)
                {
                    nBoardFolded |= (int)board[iter];
                }
            }


            double progress = baseProgess;
            for (SudokuBoard.Symbol symbol = SudokuBoard.Symbol.ONE; symbol <= SudokuBoard.Symbol.NINE; symbol = (SudokuBoard.Symbol)((int)symbol << 1))
            {
                board[position] = symbol;
                if (((int)symbol & nBoardFolded) == 0)
                {
                    if (terminal)
                    {
                        return board;
                    }
                    else
                    {
                        SudokuBoard solution = Solve(board, nextPosition, progress, progressIncrement / (double)SudokuBoard.Dimension);
                        if (solution != null)
                        {
                            return solution;
                        }
                    }
                }
                progress += progressIncrement;
                ProcessingProgress = progress;
                if (progressIncrement > 0.0001)
                {
                    Progress();
                }
            }

            board[position] = SudokuBoard.Symbol.UNKNOWN;
            return null;
        }

        public SudokuBoardValidity Validate(SudokuBoard initialBoard)
        {
            ProcessingProgress = 0.0;
            ProcessingStatus = "Validating...";
            Progress();

            int validatedCells = 0;
            int cells = SudokuBoard.Dimension * SudokuBoard.Dimension;

            List<Coordinate2D> invalidCells = new List<Coordinate2D>();
            HashSet<int> invalidRows = new HashSet<int>();
            HashSet<int> invalidColumns = new HashSet<int>();
            HashSet<int> invalidBoxes = new HashSet<int>();

            foreach (Coordinate2D coord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
            {
                bool bRowValid = RowIsValid(coord, initialBoard);
                bool bColumnValid = ColumnIsValid(coord, initialBoard);
                bool bBoxValid = BoxIsValid(coord, initialBoard);
                if (!(bRowValid && bColumnValid && bBoxValid))
                {
                    invalidCells.Add(coord);
                }

                if (!bRowValid)
                {
                    invalidRows.Add(coord.Row);
                }

                if (!bColumnValid)
                {
                    invalidColumns.Add(coord.Column);
                }

                if (!bBoxValid)
                {
                    Coordinate2D invalidBox = new Coordinate2D();
                    invalidBox.Row = coord.Row / SudokuBoard.BoxDimension;
                    invalidBox.Column = coord.Column / SudokuBoard.BoxDimension;
                    invalidBoxes.Add(invalidBox.FlattenedIndex(SudokuBoard.BoxDimension));
                }

                validatedCells++;
                ProcessingProgress = (double)validatedCells / (double)cells;
                Progress();
            }

            SudokuBoardValidity validity = new SudokuBoardValidity();
            validity.InvalidCells = invalidCells;
            validity.InvalidRows = invalidRows;
            validity.InvalidColumns = invalidColumns;
            validity.InvalidBoxes = invalidBoxes.Select(idx => Coordinate2D.FromIndex(idx, SudokuBoard.BoxDimension));

            ProcessingStatus = "Board Validated";
            Progress();

            return validity;
        }

        private void Progress()
        {
            if (ProcessingStateChanged != null)
            {
                ProcessingStateChanged();
            }
        }

        private static bool RowIsValid(Coordinate2D coord, SudokuBoard board)
        {
            SudokuBoard.Symbol current = board[coord];
            if (current == SudokuBoard.Symbol.UNKNOWN)
            {
                return true;
            }

            Coordinate2D iter = new Coordinate2D(coord.Row, 0);
            for (iter.Column = 0; iter.Column < SudokuBoard.Dimension; iter.Column++)
            {
                SudokuBoard.Symbol other = board[iter];
                if (iter != coord
                    && other != SudokuBoard.Symbol.UNKNOWN
                    && other == current)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ColumnIsValid(Coordinate2D coord, SudokuBoard board)
        {
            SudokuBoard.Symbol current = board[coord];
            if (current == SudokuBoard.Symbol.UNKNOWN)
            {
                return true;
            }

            Coordinate2D iter = new Coordinate2D(0, coord.Column);
            for (iter.Row = 0; iter.Row < SudokuBoard.Dimension; iter.Row++)
            {
                SudokuBoard.Symbol other = board[iter];
                if (iter != coord
                    && other != SudokuBoard.Symbol.UNKNOWN
                    && other == current)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool BoxIsValid(Coordinate2D coord, SudokuBoard board)
        {
            SudokuBoard.Symbol current = board[coord];
            if (current == SudokuBoard.Symbol.UNKNOWN)
            {
                return true;
            }

            Coordinate2D box = new Coordinate2D(coord.Row / SudokuBoard.BoxDimension, coord.Column / SudokuBoard.BoxDimension);

            Coordinate2D iter = new Coordinate2D();
            Coordinate2D start = new Coordinate2D(box.Row * SudokuBoard.BoxDimension, box.Column * SudokuBoard.BoxDimension);
            Coordinate2D end = new Coordinate2D(start.Row + SudokuBoard.BoxDimension, start.Column + SudokuBoard.BoxDimension);
            for (iter.Row = start.Row; iter.Row < end.Row; iter.Row++)
            {
                for (iter.Column = start.Column; iter.Column < end.Column; iter.Column++)
                {
                    SudokuBoard.Symbol other = board[iter];
                    if (iter != coord
                        && other != SudokuBoard.Symbol.UNKNOWN
                        && other == current)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
