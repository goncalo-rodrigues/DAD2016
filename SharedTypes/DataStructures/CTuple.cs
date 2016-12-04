using System;
using System.Collections.Generic;

namespace SharedTypes
{
    [Serializable]
    public class TupleID : IComparable
    {
        public int GlobalID { get; }
        public int SubID { get; }

        public TupleID()
        {
            GlobalID = -1;
            SubID = 0;
        }

        public TupleID(int global, int sub)
        {
            if (global < 0 || sub < 0)
                throw new ArgumentException("Tuple id must be positive");
            GlobalID = global;
            SubID = sub;
        }
        public int CompareTo(object obj)
        {
            var other = obj as TupleID;
            if (other == null) return 1;
            var result = GlobalID - other.GlobalID;
            if (result == 0)
                return SubID - other.SubID;
            else
                return result;
        }
        public override bool Equals(object obj)
        {
            var other = obj as TupleID;
            if (other == null) return false;
            return other.GlobalID == GlobalID && other.SubID == SubID;
        }
        public override int GetHashCode()
        {
            return GlobalID.GetHashCode();
        }
        public static bool operator ==(TupleID left, TupleID right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }
        public static bool operator !=(TupleID left, TupleID right)
        {
            return !(left == right);
        }
        public static bool operator <(TupleID left, TupleID right)
        {
            return (left.CompareTo(right) < 0);
        }
        public static bool operator >(TupleID left, TupleID right)
        {
            return (left.CompareTo(right) > 0);
        }
        public static bool operator >=(TupleID left, TupleID right)
        {
            return (left.CompareTo(right) >= 0);
        }
        public static bool operator <=(TupleID left, TupleID right)
        {
            return (left.CompareTo(right) <= 0);
        }
        public override string ToString()
        {
            return $"{GlobalID}.{SubID}";
        }
    }

    [Serializable]
    public class CTuple
    {
        private List<string> fields;
        public TupleID ID { get; }
        public string opName { get;  }
        public int repID { get; }
        public int destinationId { get; set; } //I'm sorry... i'm desperate

        public CTuple() { }
        public CTuple(List<string> fields, int globalID, int subID, string opName, int repID) {
            if (fields != null)
            {
                this.fields = new List<string>();
                foreach (string f in fields)
                    this.fields.Add(f);
            }

            this.ID = new TupleID(globalID, subID);
            this.opName = opName;
            this.repID = repID;
        }
        public IList<string> GetFields() {
            return fields;
        }
        public string GetFieldByIndex(int index)
        {
            if (index >= 0 && index < fields.Count)
                return fields[index];
            else
                throw new System.IndexOutOfRangeException();
        }
        public void AddField(string field)
        {
            this.fields.Add(field);
        }
        public override string ToString()
        {
            string repr = $"{ID} [";
            if (fields != null)
            {
                repr += string.Join(",", fields);
            }
            return repr + "]";
        }
    }

   
}