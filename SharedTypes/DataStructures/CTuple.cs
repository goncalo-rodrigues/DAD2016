using System;
using System.Collections.Generic;

namespace SharedTypes
{
    [Serializable]
    public class CTuple
    {
        private List<string> fields = new List<string>();
        public int ID { get; }

        public CTuple() {
        }
        public CTuple(List<string> fields, int ID) {
            foreach (string f in fields)
                this.fields.Add(f);
            this.ID = ID;
        }
        public void AddField(string field) {
            this.fields.Add(field);
        }
        public IList<string> GetFields() { return fields; }

        public string GetFieldByIndex(int index)
        {
            if (index >= 0 && index < fields.Count)
                return fields[index];
            else
                throw new System.IndexOutOfRangeException();
        }
        public override string ToString()
        {
            string repr = $"{ID} [";
            if (fields != null)
            {
                int i = 0;
                for( ; i < fields.Count - 1; i++)
                    repr += fields[i] + ',';

                repr += fields[i];
            }
            return repr + "]";
        }
    }
}