using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class InductiveSolver : Solver
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

        public event BasicEvent ProcessingStateChanged;

        private void Progress()
        {
            if (ProcessingStateChanged != null)
            {
                ProcessingStateChanged();
            }
        }

        private static SparseBinaryMatrix _rowAdjacency;
        private static SparseBinaryMatrix _columnAdjacency;
        private static SparseBinaryMatrix _boxAdjacency;
        private static SparseBinaryMatrix _adjacency;
        private static Coordinate2D[] _boardCoordinates;
        private static int _allSymbolsCombined;

        public InductiveSolver()
        {
            if (_adjacency == null)
            {
                _allSymbolsCombined = 0;
                for (SudokuBoard.Symbol symbol = SudokuBoard.Symbol.ONE; symbol <= SudokuBoard.Symbol.NINE; symbol = (SudokuBoard.Symbol)((int)symbol << 1))
                {
                    _allSymbolsCombined |= (int)symbol;
                }

                Coordinate2D[] cells = Coordinate2D.AllCoordinates(SudokuBoard.Dimension).ToArray();
                _boardCoordinates = cells;
                _adjacency = new SparseBinaryMatrix(_boardCoordinates.Length);
                _rowAdjacency = new SparseBinaryMatrix(_boardCoordinates.Length);
                _columnAdjacency = new SparseBinaryMatrix(_boardCoordinates.Length);
                _boxAdjacency = new SparseBinaryMatrix(_boardCoordinates.Length);
                for (int i = 0; i < cells.Length; i++)
                {
                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j] != cells[i])
                        {
                            if (cells[j].Row == cells[i].Row)
                            {
                                _rowAdjacency[new Coordinate2D(i, j)] = -1;
                                _rowAdjacency[new Coordinate2D(j, i)] = -1;
                                _adjacency[new Coordinate2D(i, j)] = -1;
                                _adjacency[new Coordinate2D(j, i)] = -1;
                            }
                            if (cells[j].Column == cells[i].Column)
                            {
                                _columnAdjacency[new Coordinate2D(i, j)] = -1;
                                _columnAdjacency[new Coordinate2D(j, i)] = -1;
                                _adjacency[new Coordinate2D(i, j)] = -1;
                                _adjacency[new Coordinate2D(j, i)] = -1;
                            }
                            if (((cells[j].Column / 3) == (cells[i].Column / 3))
                                    && ((cells[j].Row / 3) == (cells[i].Row / 3)))
                            {
                                _boxAdjacency[new Coordinate2D(i, j)] = -1;
                                _boxAdjacency[new Coordinate2D(j, i)] = -1;
                                _adjacency[new Coordinate2D(i, j)] = -1;
                                _adjacency[new Coordinate2D(j, i)] = -1;
                            }
                        }
                    }
                }
            }
        }

        public SudokuBoard Solve(SudokuBoard initialBoard)
        {
            ProcessingProgress = 0.0;
            ProcessingStatus = "Solving...";
            Progress();

            int[] initialBoardVector = new int[_boardCoordinates.Length];
            for (int i = 0; i < _boardCoordinates.Length; i++)
            {
                initialBoardVector[i] = (int)initialBoard[_boardCoordinates[i]];
            }

            int[] adjacentBoardVector = new int[_boardCoordinates.Length];
            if (!IsValid(initialBoardVector, adjacentBoardVector))
            {
                ProcessingStatus = "Could not solve";
                ProcessingProgress = 1.0;
                Progress();
                return null;
            }

            int[] workingBoard = new int[_boardCoordinates.Length];
            int[] resultantBoard = new int[_boardCoordinates.Length];

            int[] adjacentRowAllowedVector = new int[_boardCoordinates.Length];
            int[] adjacentColumnAllowedVector = new int[_boardCoordinates.Length];
            int[] adjacentBoxAllowedVector = new int[_boardCoordinates.Length];

            bool bBoardChanged = true;
            while (bBoardChanged)
            {
                bBoardChanged = CheckCells(_adjacency, initialBoardVector, initialBoardVector, workingBoard);
                if (bBoardChanged = bBoardChanged | CheckCells(_rowAdjacency, initialBoardVector, workingBoard, resultantBoard))
                {
                    continue;
                }
                if (bBoardChanged = bBoardChanged | CheckCells(_columnAdjacency, initialBoardVector, workingBoard, resultantBoard))
                {
                    continue;
                }
                if (bBoardChanged = bBoardChanged | CheckCells(_boxAdjacency, initialBoardVector, workingBoard, resultantBoard))
                {
                    continue;
                }
            }

            bool bPartial = false;
            bool bAltered = false;
            SudokuBoard solution = new SudokuBoard();
            for (int i = 0; i < _boardCoordinates.Length; i++)
            {
                solution[_boardCoordinates[i]] = (SudokuBoard.Symbol)initialBoardVector[i];
                if (solution[_boardCoordinates[i]] == SudokuBoard.Symbol.UNKNOWN)
                {
                    bPartial = true;
                }
                else if (solution[_boardCoordinates[i]] != initialBoard[_boardCoordinates[i]])
                {
                    bAltered = true;
                }
            }

            ProcessingProgress = 1.0;
            if (!bPartial)
            {
                ProcessingStatus = "Solved";
            }
            else if (bPartial && bAltered)
            {
                ProcessingStatus = "Partial Solution";
            }
            else
            {
                ProcessingStatus = "Could not solve";
            }
            Progress();

            return solution;
        }

        private bool CheckCells(SparseBinaryMatrix matrix, int[] board, int[] inputBoard, int[] resultingBoard)
        {
            bool bBoardChanged = false;
            matrix.CrossProduct(inputBoard, resultingBoard);
            for (int i = 0; i < resultingBoard.Length; i++)
            {
                int nInitial = board[i];
                if (nInitial == 0)
                {
                    int nResult = resultingBoard[i];
                    nResult = ~nResult & _allSymbolsCombined;
                    resultingBoard[i] = nResult;
                    if (UniqueBit(nResult))
                    {
                        for (int symbol = (int)SudokuBoard.Symbol.ONE; symbol <= (int)SudokuBoard.Symbol.NINE; symbol = symbol << 1)
                        {
                            if (nResult == (int)symbol)
                            {
                                board[i] = symbol;
                                bBoardChanged = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    resultingBoard[i] = nInitial;
                }
            }
            return bBoardChanged;
        }

        private bool UniqueBit(int i)
        {
            return (((i & 0xf00f) == 0) != ((i & 0x0ff0) == 0)
                && ((i & 0x5555) == 0) != ((i & 0xaaaa) == 0)
                && ((i & 0x3333) == 0) != ((i & 0xcccc) == 0));
        }

        private int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        private bool IsValid(int[] originalBoard, int[] adjacentCombinationBoard)
        {
            for (int i = 0; i < originalBoard.Length; i++)
            {
                if ((originalBoard[i] & adjacentCombinationBoard[i]) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public SudokuBoardValidity Validate(SudokuBoard initialBoard)
        {
            ProcessingProgress = 0.0f;
            ProcessingStatus = "Validating...";
            Progress();

            int[] initialBoardVector = new int[_boardCoordinates.Length];
            for (int i = 0; i < _boardCoordinates.Length; i++)
            {
                initialBoardVector[i] = (int)initialBoard[_boardCoordinates[i]];
            }

            List<Coordinate2D> invalidCells = new List<Coordinate2D>();
            HashSet<int> invalidRows = new HashSet<int>();
            HashSet<int> invalidColumns = new HashSet<int>();
            HashSet<int> invalidBoxes = new HashSet<int>();

            int[] rowResults = new int[_boardCoordinates.Length];
            int[] columnResults = new int[_boardCoordinates.Length];
            int[] boxResults = new int[_boardCoordinates.Length];

            _rowAdjacency.CrossProduct(initialBoardVector, rowResults);
            ProcessingProgress = 0.25;
            Progress();
            _columnAdjacency.CrossProduct(initialBoardVector, columnResults);
            ProcessingProgress = 0.5;
            Progress();
            _boxAdjacency.CrossProduct(initialBoardVector, boxResults);
            ProcessingProgress = 0.75;
            Progress();

            for (int i = 0; i < _boardCoordinates.Length; i++)
            {
                rowResults[i] = rowResults[i] & initialBoardVector[i];
                columnResults[i] = columnResults[i] & initialBoardVector[i];
                boxResults[i] = boxResults[i] & initialBoardVector[i];
                if (rowResults[i] != 0 || columnResults[i] != 0 || boxResults[i] != 0)
                {
                    invalidCells.Add(_boardCoordinates[i]);
                    if (rowResults[i] != 0)
                    {
                        invalidRows.Add(_boardCoordinates[i].Row);
                    }
                    if (columnResults[i] != 0)
                    {
                        invalidColumns.Add(_boardCoordinates[i].Column);
                    }
                    if (boxResults[i] != 0)
                    {
                        Coordinate2D box = new Coordinate2D();
                        box.Row = _boardCoordinates[i].Row / SudokuBoard.BoxDimension;
                        box.Column = _boardCoordinates[i].Column / SudokuBoard.BoxDimension;
                        invalidBoxes.Add(box.FlattenedIndex(SudokuBoard.BoxDimension));
                    }
                }
                ProcessingProgress = 0.75 + 0.25 * ((double)i / (double)_boardCoordinates.Length);
                Progress();
            }

            SudokuBoardValidity validity = new SudokuBoardValidity();
            validity.InvalidCells = invalidCells;
            validity.InvalidRows = invalidRows;
            validity.InvalidColumns = invalidColumns;
            validity.InvalidBoxes = invalidBoxes.Select(idx => Coordinate2D.FromIndex(idx, SudokuBoard.BoxDimension));

            ProcessingProgress = 1.0;
            ProcessingStatus = "Board Validated";
            Progress();

            return validity;
        }
    }
}
