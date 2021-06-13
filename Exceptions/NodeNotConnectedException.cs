
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Exceptions
{
    class NodeNotConnectedException : Exception
    {
        public NodeNotConnectedException()
        {

        }

        public NodeNotConnectedException(string message) : base(message)
        {

        }
    }
}
