using System;

namespace Operator
{
    class NeighbourOperatorIsDeadException : ApplicationException
    {
        public NeighbourOperatorIsDeadException() {}
        public NeighbourOperatorIsDeadException(string message) : base(message) {}
        public NeighbourOperatorIsDeadException(string message, Exception inner): base(message, inner) {}
       
    }
}
