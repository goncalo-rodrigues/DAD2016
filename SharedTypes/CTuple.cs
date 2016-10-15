using System.Collections.Generic;

namespace SharedTypes
{

    public class CTuple
    {
        private List<string> fields;
        public CTuple() {
        }
        public CTuple(List<string> fields) {
            this.fields = fields;
        }
        public void AddField(string field) {
            this.fields.Add(field);
        }
        public string GetField(int index)
        {
            if (index > 0 && index < fields.Count)
                return fields[index];
            else
                throw new System.IndexOutOfRangeException();
        }
        public override string ToString()
        {
            string repr = "[";
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