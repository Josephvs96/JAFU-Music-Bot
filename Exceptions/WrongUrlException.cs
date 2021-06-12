using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Exceptions
{
    class WrongUrlException : Exception
    {
        public WrongUrlException()
        {

        }
        public WrongUrlException(string message) : base(message)
        {

        }
    }
}
