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
    public delegate void BasicEvent();

    public partial class SudokuCell : UserControl
    {
        public event Action ValueChanged = delegate { };

        private void SignalValueChanged()
        {
            ValueChanged();
        }

        public SudokuCell()
        {
            InitializeComponent();

            cellTextBox.KeyDown += new KeyEventHandler(KeyPressed);
            cellTextBox.TextChanged += (s, e) => SignalValueChanged();

            SizeChanged += SudokuCell_SizeChanged;
        }

        public event BasicEvent MoveFocusLeft;
        public event BasicEvent MoveFocusRight;
        public event BasicEvent MoveFocusUp;
        public event BasicEvent MoveFocusDown;

        public enum CellState
        {
            VALID,
            INVALID,
            NEIGHBOR_INVALID
        }

        public CellState ValidityState
        {
            set
            {
                switch (value)
                {
                    case CellState.VALID:
                        cellTextBox.BackColor = Color.White;
                        break;
                    case CellState.INVALID:
                        cellTextBox.BackColor = Color.Firebrick;
                        break;
                    case CellState.NEIGHBOR_INVALID:
                        cellTextBox.BackColor = Color.IndianRed;
                        break;
                }
            }
        }

        public SudokuBoard.Symbol Value
        {
            get
            {
                return SudokuBoard.StringSymbol(cellTextBox.Text);
            }

            set
            {
                if (value != SudokuBoard.Symbol.UNKNOWN)
                {
                    cellTextBox.Text = SudokuBoard.SymbolString(value);
                }
                else
                {
                    cellTextBox.Text = "";
                }
            }
        }

        void SudokuCell_SizeChanged(object sender, EventArgs e)
        {
            cellTextBox.Font = new System.Drawing.Font(FontFamily.GenericSansSerif,
                                                    (float)(Math.Max(10, cellTextBox.Height - 15)),
                                                    FontStyle.Bold,
                                                    GraphicsUnit.Pixel);
        }

        private void KeyPressed(Object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.Up:
                    if (MoveFocusUp != null)
                    {
                        MoveFocusUp();
                    }
                    break;
                case Keys.Left:
                    if (MoveFocusLeft != null)
                    {
                        MoveFocusLeft();
                    }
                    break;
                case Keys.Down:
                    if (MoveFocusDown != null)
                    {
                        MoveFocusDown();
                    }
                    break;
                case Keys.Right:
                    if (MoveFocusRight != null)
                    {
                        MoveFocusRight();
                    }
                    break;
                case Keys.NumPad1:
                    cellTextBox.Text = "1";
                    break;
                case Keys.NumPad2:
                    cellTextBox.Text = "2";
                    break;
                case Keys.NumPad3:
                    cellTextBox.Text = "3";
                    break;
                case Keys.NumPad4:
                    cellTextBox.Text = "4";
                    break;
                case Keys.NumPad5:
                    cellTextBox.Text = "5";
                    break;
                case Keys.NumPad6:
                    cellTextBox.Text = "6";
                    break;
                case Keys.NumPad7:
                    cellTextBox.Text = "7";
                    break;
                case Keys.NumPad8:
                    cellTextBox.Text = "8";
                    break;
                case Keys.NumPad9:
                    cellTextBox.Text = "9";
                    break;
                case Keys.Delete:
                    cellTextBox.Text = "";
                    break;
                case Keys.Back:
                    cellTextBox.Text = "";
                    break;
            }

            if (Char.IsDigit((char)args.KeyValue)
                && args.KeyValue != '0')
            {
                cellTextBox.Text = new String((char)args.KeyValue, 1);
            }
            
            args.Handled = true;
            args.SuppressKeyPress = true;
            return;
        }
    }
}
