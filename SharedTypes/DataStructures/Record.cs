using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [Serializable]
    public class Record : IComparable<Record>
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string Owner { get; set; }

        public Record(string type, string owner, string content, DateTime time)
        {
            Type = type;
            Owner = owner;
            Content = content;
            Timestamp = time;    
        }
        public override string ToString()
        {
            //return $"[{Timestamp}  <{Owner}>]: {Content}";
            return $"{Type.PadRight(8, ' ')} {Owner.PadRight(8, ' ')} {Content}";
        }
        public int CompareTo(Record other)
        {
            return Timestamp.CompareTo(other.Timestamp);
        }
    }
}
