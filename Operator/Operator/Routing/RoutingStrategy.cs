using SharedTypes;

namespace Operator
{
    public abstract class RoutingStrategy
    {
        public int countRep { get; set; }

        public RoutingStrategy(int countRep)
        {
            this.countRep = countRep;
        }
        abstract public int ChooseReplica(CTuple tuple);
        public virtual object GetState()
        {
            return null;
        }
        public virtual void LoadState(object state)
        {
            // do nothing
        }
    }
}


