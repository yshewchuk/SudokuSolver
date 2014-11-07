using Sudoku;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SudokuGUI
{
    public partial class SudokuSolverWindow : Form
    {
        public SudokuSolverWindow()
        {
            bAllowPaint = true;
            _grid = new SudokuGrid();
            _grid.BoardChanged += UpdateMinimizeState;
            Controls.Add(_grid);

            SizeChanged += SudokuSolverWindow_SizeChanged;

            InitializeComponent();
            UpdateAlgorithm();

            Timer = new Stopwatch();
            bruteForceRadio.CheckedChanged += UpdateAlgorithm;
            inductiveRadio.CheckedChanged += UpdateAlgorithm;
            mixedRadio.CheckedChanged += UpdateAlgorithm;

            // start off with test board
            SavedBoard = new SudokuBoard();
            SavedBoard[new Coordinate2D(1, 5)] = SudokuBoard.Symbol.THREE;
            SavedBoard[new Coordinate2D(1, 7)] = SudokuBoard.Symbol.EIGHT;
            SavedBoard[new Coordinate2D(1, 8)] = SudokuBoard.Symbol.FIVE;
            SavedBoard[new Coordinate2D(2, 2)] = SudokuBoard.Symbol.ONE;
            SavedBoard[new Coordinate2D(2, 4)] = SudokuBoard.Symbol.TWO;
            SavedBoard[new Coordinate2D(3, 3)] = SudokuBoard.Symbol.FIVE;
            SavedBoard[new Coordinate2D(3, 5)] = SudokuBoard.Symbol.SEVEN;
            SavedBoard[new Coordinate2D(4, 2)] = SudokuBoard.Symbol.FOUR;
            SavedBoard[new Coordinate2D(4, 6)] = SudokuBoard.Symbol.ONE;
            SavedBoard[new Coordinate2D(5, 1)] = SudokuBoard.Symbol.NINE;
            SavedBoard[new Coordinate2D(6, 0)] = SudokuBoard.Symbol.FIVE;
            SavedBoard[new Coordinate2D(6, 7)] = SudokuBoard.Symbol.SEVEN;
            SavedBoard[new Coordinate2D(6, 8)] = SudokuBoard.Symbol.THREE;
            SavedBoard[new Coordinate2D(7, 2)] = SudokuBoard.Symbol.TWO;
            SavedBoard[new Coordinate2D(7, 4)] = SudokuBoard.Symbol.ONE;
            SavedBoard[new Coordinate2D(8, 4)] = SudokuBoard.Symbol.FOUR;
            SavedBoard[new Coordinate2D(8, 8)] = SudokuBoard.Symbol.NINE;
            _grid.Board = SavedBoard;
        }

        private Solver Algorithm
        {
            get;
            set;
        }

        private Stopwatch Timer
        {
            get;
            set;
        }

        private String Status
        {
            get;
            set;
        }

        private String TimeResult
        {
            get;
            set;
        }

        private SudokuBoard SavedBoard
        {
            get;
            set;
        }

        void UpdateAlgorithm(Object sender = null, EventArgs args = null)
        {
            if (bruteForceRadio.Checked)
            {
                Algorithm = new BruteForceSolver2();
                Algorithm.ProcessingStateChanged += InvokeUpdateState;
            }
            else if (inductiveRadio.Checked)
            {
                Algorithm = new InductiveSolver();
                Algorithm.ProcessingStateChanged += InvokeUpdateState;
            }
            else if (mixedRadio.Checked)
            {
                Algorithm = null;
            }
        }

        void InvokeUpdateState()
        {
            Status = Algorithm.ProcessingStatus;
            TimeResult = "";

            if (InvokeRequired)
            {
                Invoke(new Action(UpdateState));
            }
            else
            {
                Timer.Stop();
                UpdateState();
                Application.DoEvents();
                Timer.Start();
            }
        }

        void UpdateState()
        {
            if (Algorithm != null)
            {
                statusLabel.Text = Status;
                timingLabel.Text = TimeResult;
                progressBar.Value = (int)(Algorithm.ProcessingProgress * progressBar.Maximum);
            }
        }

        void SudokuSolverWindow_SizeChanged(object sender, EventArgs e)
        {
            int nSize = Math.Min(Height - 65, Width - 160);

            _grid.Size = new Size(nSize, nSize);
        }

        private SudokuGrid _grid;

        private void clearButton_Click(object sender, EventArgs e)
        {
            _grid.Board = new SudokuBoard();
            ClearValidity();
        }
        
        private void validateButton_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            timingLabel.Text = "";
            if (Algorithm == null)
            {
                statusLabel.Text = "Algorithm not implemented";
                return;
            }

            statusLabel.Text = "";
            ClearValidity();
            BeginProcessing();

            ValidationProcessor = new ProcessingInstance<SudokuBoardValidity>(Validate);
            ValidationProcessor.Completed += ValidateComplete;
            ValidationProcessor.Failed += ValidateFailed;

            ValidationProcessor.Start();
        }

        private ProcessingInstance<SudokuBoardValidity> ValidationProcessor
        {
            get;
            set;
        }

        private void UpdateMinimizeState()
        {
            minimizeButton.Enabled = !_grid.Board.Any(cell => cell.Value == SudokuBoard.Symbol.UNKNOWN);
        }

        private new SudokuBoardValidity Validate()
        {
            SudokuBoard board = _grid.Board;
            Timer.Reset();
            Timer.Start();
            SudokuBoardValidity validity = Algorithm.Validate(board);
            Timer.Stop();
            return validity;
        }

        private void ClearValidity()
        {
            foreach (Coordinate2D coord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
            {
                _grid.Cell(coord).ValidityState = SudokuCell.CellState.VALID;
            }
        }

        private void UpdateGridWithValidity()
        {
            if (ValidationProcessor == null || ValidationProcessor.ReturnValue == null)
            {
                ClearValidity();
                return;
            }

            foreach (Coordinate2D coord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
            {
                if (ValidationProcessor.ReturnValue.InvalidCells.Contains(coord))
                {
                    _grid.Cell(coord).ValidityState = SudokuCell.CellState.INVALID;
                }
                else if (ValidationProcessor.ReturnValue.InvalidColumns.Contains(coord.Column)
                    || ValidationProcessor.ReturnValue.InvalidRows.Contains(coord.Row)
                    || ValidationProcessor.ReturnValue.InvalidBoxes.Contains(new Coordinate2D(coord.Row / SudokuBoard.BoxDimension, coord.Column / SudokuBoard.BoxDimension)))
                {
                    _grid.Cell(coord).ValidityState = SudokuCell.CellState.NEIGHBOR_INVALID;
                }
                else
                {
                    _grid.Cell(coord).ValidityState = SudokuCell.CellState.VALID;
                }
            }
        }

        private void ValidateComplete()
        {
            Status = Algorithm.ProcessingStatus;
            TimeResult = "Completed in " + (1000.0 * (double)Timer.ElapsedTicks / (double)Stopwatch.Frequency).ToString("0.0") + "ms";
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(UpdateGridWithValidity));
            Invoke(new Action(CloseValidityProcessor));
        }

        private void ValidateFailed()
        {
            Status = Algorithm.ProcessingStatus;
            TimeResult = ValidationProcessor.ErrorMessage;
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(CloseValidityProcessor));
        }

        private void CloseValidityProcessor()
        {
            ValidationProcessor = null;
        }

        private static void SetInteractiveControlsEnabled(Control parent, bool enabled)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button || ctrl is RadioButton || ctrl is TextBox)
                {
                    ctrl.Enabled = enabled;
                }
                SetInteractiveControlsEnabled(ctrl, enabled);
            }
        }

        void BeginProcessing()
        {
            SetInteractiveControlsEnabled(this, false);
            cancelButton.Enabled = true;
        }

        void EndProcessing()
        {
            SetInteractiveControlsEnabled(this, true);
            cancelButton.Enabled = false;
        }

        private void Cancel(object sender = null, EventArgs e = null)
        {
            if (ValidationProcessor != null)
            {
                ValidationProcessor.Cancel();
            }

            if (SolutionProcessor != null)
            {
                SolutionProcessor.Cancel();
            }
        }

        private void solveButton_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            timingLabel.Text = "";
            if (Algorithm == null)
            {
                statusLabel.Text = "Algorithm not implemented";
                return;
            }

            statusLabel.Text = "";
            ClearValidity();
            BeginProcessing();

            SolutionProcessor = new ProcessingInstance<SudokuBoard>(Solve);
            SolutionProcessor.Completed += SolveComplete;
            SolutionProcessor.Failed += SolveFailed;

            SolutionProcessor.Start();
        }

        private ProcessingInstance<SudokuBoard> SolutionProcessor
        {
            get;
            set;
        }

        private SudokuBoard Solve()
        {
            SudokuBoard board = _grid.Board;
            Timer.Reset();
            Timer.Start();
            SudokuBoard solution = Algorithm.Solve(board);
            Timer.Stop();
            return solution;
        }

        private void UpdateGridWithSolution()
        {
            bAllowPaint = false;
            ClearValidity();
            if (SolutionProcessor == null || SolutionProcessor.ReturnValue == null)
            {
                bAllowPaint = true;
                return;
            }

            _grid.Board = SolutionProcessor.ReturnValue;
            bAllowPaint = true;
        }

        private void SolveComplete()
        {
            Status = Algorithm.ProcessingStatus;
            TimeResult = "Completed in " + (1000.0 * (double)Timer.ElapsedTicks / (double)Stopwatch.Frequency).ToString("0.0") + "ms";
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(UpdateGridWithSolution));
            Invoke(new Action(CloseSolutionProcessor));
        }

        private void SolveFailed()
        {
            Status = Algorithm.ProcessingStatus;
            TimeResult = SolutionProcessor.ErrorMessage;
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(CloseSolutionProcessor));
        }

        private void CloseSolutionProcessor()
        {
            SolutionProcessor = null;
        }

        private bool bAllowPaint;
        private const int WM_PAINT = 0x000F;

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg != WM_PAINT) ||
                (bAllowPaint && m.Msg == WM_PAINT))
            {
                base.WndProc(ref m);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sudoku files (*.sdk)|*.sdk|All files (*.*)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                SavedBoard = _grid.Board;
                File.WriteAllText(dialog.FileName, SavedBoard.ToString());
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Sudoku files (*.sdk)|*.sdk|All files (*.*)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                String str = File.ReadAllText(dialog.FileName);
                SavedBoard = SudokuBoard.FromString(str);
                _grid.Board = SavedBoard;
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            _grid.Board = SavedBoard;
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            timingLabel.Text = "";
            statusLabel.Text = "Generating Board...";
            ClearValidity();
            BeginProcessing();

            BoardMinimizer = new ProcessingInstance<SudokuBoard>(Minimize);
            BoardMinimizer.Completed += MinimizeComplete;
            BoardMinimizer.Failed += MinimizeFailed;

            BoardMinimizer.Start();
        }

        private ProcessingInstance<SudokuBoard> BoardMinimizer
        {
            get;
            set;
        }

        private new SudokuBoard Minimize()
        {
            SudokuBoard board = _grid.Board;
            Timer.Reset();
            Timer.Start();
            SudokuBoard result = BoardGenerator.Minimize(board);
            Timer.Stop();
            return result;
        }

        private void MinimizeComplete()
        {
            Status = "Board Generated";
            TimeResult = "Completed in " + (1000.0 * (double)Timer.ElapsedTicks / (double)Stopwatch.Frequency).ToString("0.0") + "ms";
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(UpdateGridWithMinimizedBoard));
            Invoke(new Action(CloseValidityProcessor));
        }

        private void MinimizeFailed()
        {
            Status = "Board Generation Failed";
            Invoke(new Action(UpdateState));
            Invoke(new Action(EndProcessing));
            Invoke(new Action(CloseMinimizeProcessor));
        }

        private void CloseMinimizeProcessor()
        {
            BoardMinimizer = null;
        }

        private void UpdateGridWithMinimizedBoard()
        {
            bAllowPaint = false;
            ClearValidity();
            if (BoardMinimizer == null || BoardMinimizer.ReturnValue == null)
            {
                bAllowPaint = true;
                return;
            }

            _grid.Board = BoardMinimizer.ReturnValue;
            bAllowPaint = true;
        }
    }
}
