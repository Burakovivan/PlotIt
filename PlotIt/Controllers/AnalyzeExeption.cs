using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlottingGraphsSystem
{
    class InvalidBraketsExeption : ArgumentException
    {
        public override string Message
        {
            get
            {
                return "String expression is not valid expression. Number of closing and opening brackets are different or brackets are confused.";
            }
        }
    }

}
