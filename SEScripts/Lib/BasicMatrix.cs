using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib
{

    class TestBasicMatrix
    {
        BasicMatrix mat = new BasicMatrix
        {
            { 0d, 0d, 0d },
            { 0d, 0d, 0d },
            { 0d, 0d, 0d }
        };
    }
    class BasicMatrix : System.Collections.Generic.IEnumerable<double>
    {
        public int Width
        {
            get { return Values.Length; }
        }
        public int Height
        {
            get { return Values[0].Length; }
        }
        Double[][] Values;

        public Double this[int x, int y]
        {
            get { return Values[x][y]; }
            set { Values[x][y] = value; }
        }

        public BasicMatrix() : this(0, 0) { }

        public BasicMatrix(int cols, int rows)
        {
            Values = new Double[cols][];
            for(int x = 0; x < Values.Length; x++)
            {
                Values[x] = new Double[rows];
            }
        }

        public void Add(params Double[] column)
        {
            if(Height != 0 && Height != column.Length)
            {
                throw new Exception("All columns need to have the same size!");
            }
            Double[][] newValues = new Double[Width + 1][];
            for(int x = 0; x < Width; x++)
            {
                newValues[x] = Values[x];
            }
            newValues[Width] = column;
            Values = newValues;
        }

        private IEnumerable<double> GetValuesAsync()
        {
            for(int x = 0; x < Width; x++)
            {
                for(int y = 0; y < Height; y++)
                {
                    yield return Values[x][y];
                }
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            return GetValuesAsync().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Double[][] ToArray()
        {
            return Values;
        }
        public BasicMatrix Clone()
        {
            return BasicMatrix.Add(new BasicMatrix(Width, Height),
                this);
        }


        // Math functions
        public static BasicMatrix Add(BasicMatrix matrix1, BasicMatrix matrix2)
        {
            if(matrix1.Width != matrix2.Width || matrix1.Height != matrix2.Height)
            {
                throw new DimensionMismatchException("Matrices must have the same dimensions!");
            }
            BasicMatrix result = new BasicMatrix(matrix1.Height, matrix1.Width);
            for (int x = 0; x < matrix1.Width; x++)
            {
                for(int y = 0; y < matrix1.Height; y++)
                {

                    result[x,y] = matrix1[x,y] + matrix2[x,y];
                }
            }
            return result;
        }

        public static BasicMatrix Multiply(double v, BasicMatrix matrix)
        {
            BasicMatrix result = new BasicMatrix(matrix.Width, matrix.Height);
            
            for(int x = 0; x < matrix.Width; x++)
            {
                for(int y = 0; y < matrix.Height; y++)
                {
                    result[x,y] = v * matrix[x,y];
                }
            }
            return result;
        }

        public static BasicMatrix Multiply(BasicMatrix matrix1, BasicMatrix matrix2)
        {
            if(matrix1.Height != matrix2.Width)
            {
                throw new DimensionMismatchException("Matrix1.Height must equal Matrix2.Width!");
            }
            BasicMatrix result = new BasicMatrix(matrix1.Width, matrix2.Height);
            for(int x = 0; x < matrix1.Width; x++)
            {
                for (int y = 0; y < matrix2.Height; y++)
                {
                    for (int k = 0; k < matrix1.Height; k++)
                    {
                        result[x,y] += matrix1[x,k] * matrix2[k,y];
                    }
                }
            }
            return result;
        }

        public static BasicMatrix Transpose(BasicMatrix matrix)
        {
            BasicMatrix result = new BasicMatrix(matrix.Height, matrix.Width);
            for (int x = 0; x < matrix.Width; x++)
            {
                for(int y = 0; y < matrix.Height; y++)
                {
                    result[y, x] = matrix[x, y];
                }
            }
            return result;
        }

        public static BasicMatrix operator +(BasicMatrix matrix1, BasicMatrix matrix2)
        {
            return BasicMatrix.Add(matrix1, matrix2);
        }

        public static BasicMatrix operator *(BasicMatrix matrix1, BasicMatrix matrix2)
        {
            return BasicMatrix.Multiply(matrix1, matrix2);
        }

        public static BasicMatrix operator *(double v, BasicMatrix matrix)
        {
            return BasicMatrix.Multiply(v, matrix);
        }

        public static BasicMatrix operator *(BasicMatrix matrix, double v)
        {
            return BasicMatrix.Multiply(v, matrix);
        }

        
    }
}
