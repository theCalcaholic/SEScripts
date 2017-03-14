using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib.Build
{
    class BasicVector : System.Collections.Generic.IEnumerable<double>
    {
        Double[] Values;
        public int Size
        {
            get { return Values.Length; }
        }

        public double Length
        {
            get
            {
                double length = 0;
                for(int i = 0; i < Size; i++)
                {
                    length += Values[i];
                }
                return length;
            }
        }

        public Double this[int i]
        {
            get { return Values[i]; }
            set { Values[i] = value; }
        }

        public BasicVector() : this(0)
        { }

        public BasicVector(int length)
        {
            Values = new Double[length];
        }

        public void Add(double value)
        {
            Double[] newValues = new Double[Size + 1];
            Values.CopyTo(newValues, 0);
            newValues[Size] = value;
            Values = newValues;
        }
        
        private IEnumerable<double> GetValuesAsync()
        {
            for (int i = 0; i < Size; i++)
            {
                yield return Values[i];
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

        public BasicVector Clone()
        {
            BasicVector result = new BasicVector(Size);
            this.CopyTo(result, 0);
            return result;
        }

        public void CopyTo(BasicVector vector, int index)
        {
            for(int i = 0; i < Size; i++)
            {
                vector[index + i] = this[i];
            }
        }


        // Math functions

        public static BasicVector Add(BasicVector vector1, BasicVector vector2)
        {
            if(vector1.Size != vector2.Size)
            {
                throw new DimensionMismatchException("Vectors need to have the same length!");
            }
            BasicVector result = new BasicVector(vector1.Size);
            for(int i = 0; i < vector1.Size; i++)
            {
                result[i] = vector1[i] + vector2[i];
            }
            return result;
        }

        public static BasicVector ScalarProduct(double v, BasicVector vector)
        {
            BasicVector result = new BasicVector(vector.Size);
            for(int i = 0; i < vector.Size; i++)
            {
                result[i] = v * vector[i];
            }
            return result;
        }

        public static double Dot(BasicVector vector1, BasicVector vector2)
        {
            if (vector1.Size != vector2.Size)
            {
                throw new DimensionMismatchException("Vectors need to have the same length!");
            }
            double result = 0;
            for (int i = 0; i < vector1.Size; i++)
            {
                result += vector1[i] * vector2[i];
            }
            return result;
        }

        public static double ScalarProduct(BasicVector v1, BasicVector v2)
        {
            return Dot(v1, v2);
        }

        public static BasicVector Multiply(BasicVector v1, BasicVector v2)
        {
            if (v1.Size != v2.Size)
            {
                throw new DimensionMismatchException("Vectors need to have the same length!");
            }
            BasicVector result = new BasicVector(v1.Size);
            for(int i = 0; i < v1.Size; i++)
            {
                result[i] = v1[i] * v2[i];
            }

            return result;
        }


    }
}
