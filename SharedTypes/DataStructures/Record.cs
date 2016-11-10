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
        public string Content { get; set; }
        public string Owner { get; set; }

        public Record(string owner, string content, DateTime time)
        {
            Owner = owner;
            Content = content;
            Timestamp = time;    
        }
        public override string ToString()
        {
            return $"[{Timestamp}] [OpID: {Owner}] {Content}";
        }
        public int CompareTo(Record other)
        {
            return Timestamp.CompareTo(other.Timestamp);
        }
    }
}
