using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sudoku;

namespace SudokuGUI
{
    public partial class SudokuGrid : UserControl
    {
        public event Action BoardChanged = delegate { };

        private void SignalBoardChanged()
        {
            BoardChanged();
        }

        public SudokuGrid()
        {
            InitializeComponent();

            _blockPanel = new TableLayoutPanel();
            _blockPanel.ColumnCount = SudokuBoard.BoxDimension;
            _blockPanel.RowCount = SudokuBoard.BoxDimension;
            _blockPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            _blockPanel.Dock = DockStyle.Fill;
            _blockPanel.Padding = new Padding(0);

            for (int i = 0; i < SudokuBoard.BoxDimension; i++)
            {
                _blockPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f / (float)SudokuBoard.BoxDimension));
                _blockPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f / (float)SudokuBoard.BoxDimension));
            }

            foreach (Coordinate2D box in Coordinate2D.AllCoordinates(SudokuBoard.BoxDimension))
            {
                TableLayoutPanel boxPanel = new TableLayoutPanel();
                boxPanel.ColumnCount = SudokuBoard.BoxDimension;
                boxPanel.RowCount = SudokuBoard.BoxDimension;
                boxPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                boxPanel.Dock = DockStyle.Fill;
                boxPanel.Margin = new Padding(0);

                for (int i = 0; i < SudokuBoard.BoxDimension; i++)
                {
                    boxPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f / (float)SudokuBoard.BoxDimension));
                    boxPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100.0f / (float)SudokuBoard.BoxDimension));
                }
                
                foreach (Coordinate2D cellPos in Coordinate2D.AllCoordinates(SudokuBoard.BoxDimension))
                {
                    SudokuCell cell = new SudokuCell();
                    cell.Dock = DockStyle.Fill;
                    cell.ValueChanged += SignalBoardChanged;
                    boxPanel.Controls.Add(cell);
                    boxPanel.SetColumn(cell, cellPos.Column);
                    boxPanel.SetRow(cell, cellPos.Row);
                }

                _blockPanel.Controls.Add(boxPanel);
                _blockPanel.SetColumn(boxPanel, box.Column);
                _blockPanel.SetRow(boxPanel, box.Row);
            }

            foreach (Coordinate2D cellCoord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
            {
                SudokuCell cell = Cell(cellCoord);

                if (cellCoord.Column > 0)
                {
                    SudokuCell left = Cell(new Coordinate2D(cellCoord.Row, cellCoord.Column - 1));
                    cell.MoveFocusLeft += () => left.Focus();
                }

                if (cellCoord.Column < 8)
                {
                    SudokuCell right = Cell(new Coordinate2D(cellCoord.Row, cellCoord.Column + 1));
                    cell.MoveFocusRight += () => right.Focus();
                }

                if (cellCoord.Row > 0)
                {
                    SudokuCell up = Cell(new Coordinate2D(cellCoord.Row - 1, cellCoord.Column));
                    cell.MoveFocusUp += () => up.Focus();
                }

                if (cellCoord.Row < 8)
                {
                    SudokuCell down = Cell(new Coordinate2D(cellCoord.Row + 1, cellCoord.Column));
                    cell.MoveFocusDown += () => down.Focus();
                }
            }

            Controls.Add(_blockPanel);
        }

        private TableLayoutPanel _blockPanel;

        public SudokuCell Cell(Coordinate2D coord)
        {
            TableLayoutPanel box = _blockPanel.GetControlFromPosition(coord.Column / SudokuBoard.BoxDimension, coord.Row / SudokuBoard.BoxDimension) as TableLayoutPanel;
            return box.GetControlFromPosition(coord.Column % SudokuBoard.BoxDimension, coord.Row % SudokuBoard.BoxDimension) as SudokuCell;
        }

        public SudokuBoard Board
        {
            get
            {
                SudokuBoard board = new SudokuBoard();
                foreach (Coordinate2D coord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
                {
                    board[coord] = Cell(coord).Value;
                }
                return board;
            }

            set
            {
                foreach (Coordinate2D coord in Coordinate2D.AllCoordinates(SudokuBoard.Dimension))
                {
                    SudokuCell cell = Cell(coord);
                    cell.ValueChanged -= SignalBoardChanged;
                    cell.Value = value[coord];
                    cell.ValueChanged += SignalBoardChanged;
                }
                SignalBoardChanged();
            }
        }

    }
}
