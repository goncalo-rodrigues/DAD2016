using System;

namespace SharedTypes

{
    [Serializable]
    public class NeighbourOperatorIsDeadException : ApplicationException
    {
        public NeighbourOperatorIsDeadException() {}
        public NeighbourOperatorIsDeadException(string message) : base(message) {}
        public NeighbourOperatorIsDeadException(string message, Exception inner): base(message, inner) {}

        public NeighbourOperatorIsDeadException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
		: base(info, context) {}

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }



    }
}
