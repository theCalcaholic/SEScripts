using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib
{

    class DimensionMismatchException : Exception
    {
        public DimensionMismatchException() : base("Dimensions not matching!")
        { }

        public DimensionMismatchException(string reason) : base("Dimensions not matching: " + reason)
        { }
    }
}
