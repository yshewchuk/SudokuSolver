using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class SparseBinaryMatrix
    {
        private KeyValuePair<int, int>[,] _nonZeroValues;
        private int[] _columnCounts;

        public int Dimension
        {
            get;
            private set;
        }

        public SparseBinaryMatrix(int dimension)
        {
            _nonZeroValues = new KeyValuePair<int, int>[dimension, dimension];
            _columnCounts = new int[dimension];
            Dimension = dimension;
        }

        public void CrossProduct(int[] vector, int[] resultant)
        {
            if (vector.Length != Dimension)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < Dimension; i++)
            {
                int result = 0;
                for (int j = 0; j < _columnCounts[i]; j++)
                {
                    KeyValuePair<int, int> pair = _nonZeroValues[i, j];
                    result = result | (vector[pair.Key] & pair.Value);
                }
                resultant[i] = result;
            }
        }

        public int this[Coordinate2D pos]
        {
            get
            {
                if (_columnCounts[pos.Row] == 0)
                {
                    return 0;
                }
                int nMinIndex = 0;
                int nMaxIndex = _columnCounts[pos.Row] - 1;
                while (nMinIndex < nMaxIndex)
                {
                    int nMidIndex = (nMaxIndex - nMinIndex) / 2 + nMinIndex;
                    if (_nonZeroValues[pos.Row, nMidIndex].Key == pos.Column)
                    {
                        return _nonZeroValues[pos.Row, nMidIndex].Value;
                    }
                    else if (_nonZeroValues[pos.Row, nMidIndex].Key < pos.Column)
                    {
                        nMinIndex = nMidIndex;
                    }
                    else
                    {
                        nMaxIndex = nMidIndex;
                    }
                }
                if (_nonZeroValues[pos.Row, nMinIndex].Key == pos.Column)
                {
                    return _nonZeroValues[pos.Row, nMinIndex].Value;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (value == 0)
                {
                    bool bFound = false;
                    for (int i = 0; i < _columnCounts[pos.Row]; i++)
                    {
                        if (_nonZeroValues[pos.Row, i].Key == pos.Column)
                        {
                            bFound = true;
                        }
                        else if (bFound)
                        {
                            _nonZeroValues[pos.Row, i - 1] = _nonZeroValues[pos.Row, i];
                        }
                        else if (_nonZeroValues[pos.Row, i].Key > pos.Column) // already zero
                        {
                            return;
                        }
                    }
                    if (bFound)
                    {
                        _columnCounts[pos.Row]--;
                    }
                }
                else
                {
                    bool bInserted = false;
                    KeyValuePair<int, int> insertValue = new KeyValuePair<int,int>(pos.Column, value);
                    KeyValuePair<int, int> tempValue;
                    for (int i = 0; i < _columnCounts[pos.Row]; i++)
                    {
                        if (_nonZeroValues[pos.Row, i].Key == pos.Column)
                        {
                            _nonZeroValues[pos.Row, i] = insertValue;
                            return;
                        }
                        else if (_nonZeroValues[pos.Row, i].Key > pos.Column)
                        {
                            tempValue = _nonZeroValues[pos.Row, i];
                            _nonZeroValues[pos.Row, i] = insertValue;
                            insertValue = tempValue;
                            if (!bInserted)
                            {
                                _columnCounts[pos.Row]++;
                            }
                            bInserted = true;
                        }
                    }
                    if (!bInserted)
                    {
                        _nonZeroValues[pos.Row, _columnCounts[pos.Row]] = insertValue;
                        _columnCounts[pos.Row]++;
                    }
                }
            }
        }


    }
}
