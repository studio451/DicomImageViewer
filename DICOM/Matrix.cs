//Copyright © 2018 studio451.ru
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//E-mail: info@studio451.ru
//URL: https://studio451.ru/dicomimageviewer

using System;
using System.Text;
using ClearCanvas.Common;


namespace DicomImageViewer.Dicom
{
	internal class Matrix
	{
		private readonly int _rows;
		private readonly int _columns;

		private readonly float[,] _matrix;

		public Matrix(int rows, int columns)
		{
			Platform.CheckPositive(rows, "rows");
			Platform.CheckPositive(columns, "columns");

			_rows = rows;
			_columns = columns;

			_matrix = new float[rows, columns];
		}

		private Matrix(int rows, int columns, float[,] matrix)
		{
			_matrix = matrix;
			_rows = rows;
			_columns = columns;
		}

		public int Rows
		{
			get { return _rows; }	
		}

		public int Columns
		{
			get { return _columns; }
		}



		public float this[int row, int column]
		{
			get
			{
				Platform.CheckArgumentRange(row, 0, _rows - 1, "row");
				Platform.CheckArgumentRange(column, 0, _columns - 1, "column");

				return _matrix[row, column];
			}
			set
			{
				Platform.CheckArgumentRange(row, 0, _rows - 1, "row");
				Platform.CheckArgumentRange(column, 0, _columns - 1, "column");

				_matrix[row, column] = value;
			}
		}
		
		private void Scale(float scale)
		{
			for (int row = 0; row < _rows; ++row)
			{
				for (int column = 0; column < _columns; ++column)
					this[row, column] = this[row, column] * scale;
			}
		}
	    
		public void SetColumn(int column, params float[] values)
		{
			Platform.CheckArgumentRange(column, 0, _columns - 1, "column");
			Platform.CheckTrue(values.Length == _rows, "number of parameters == _rows");

			for (int row = 0; row < _rows; ++row)
				_matrix[row, column] = values[row];
		}
		
		public Matrix Clone()
		{
			float[,] matrix = (float[,])_matrix.Clone();
			for (int row = 0; row < _rows; ++row)
			{
				for (int column = 0; column < _columns; ++column)
					matrix[row, column] = _matrix[row, column];
			}
			
			return new Matrix(_rows, _columns, matrix);
		}


		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			for(int row = 0; row < _rows; ++row)
			{
				builder.Append("( ");
				for(int column = 0; column < _columns; ++column)
				{
					builder.Append(_matrix[row, column].ToString("F4"));
					if (column < (_columns - 1))
						builder.Append("  ");
				}

				builder.Append(")");
				if (row < (_rows - 1))
					builder.Append(", ");
			}

			return builder.ToString();
		}

		public static Matrix operator *(Matrix left, Matrix right)
		{
			if (left.Columns != right.Rows)
				throw new ArgumentException("Cannot multiply the two matrices together; their sizes are incompatible.");

			Matrix result = new Matrix(left.Rows, right.Columns);
			int mutualDimension = right.Rows;

			for (int row = 0; row < result.Rows; ++row)
			{
				for (int column = 0; column < result.Columns; ++column)
				{
					float value = 0F;

					for (int k = 0; k < mutualDimension; ++k)
						value = value + left[row, k] * right[k, column];

					result[row, column] = value;
				}
			}

			return result;
		}
		
	}
}
