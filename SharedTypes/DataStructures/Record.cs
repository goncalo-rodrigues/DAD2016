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
        public Record(string content, DateTime time)
        {
            Content = content;
            Timestamp = time;    
        }
        public override string ToString()
        {
            return $"[{Timestamp}] {Content}";
        }
        public int CompareTo(Record other)
        {
            return Timestamp.CompareTo(other.Timestamp);
        }
    }
}
