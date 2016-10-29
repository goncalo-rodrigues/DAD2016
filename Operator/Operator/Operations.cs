using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Operator
{
    class Operations
    {
        public static Replica ReplicaInstance { get; set; }
        public static ProcessDelegate GetOperation(string operationName, IList<string> args = null)
        {
            if (operationName == "DUP")
            {
                return GetDupOperation();
            } else if (operationName == "UNIQ")
            {
                return GetUniqOperation(Int32.Parse(args[0]));
            } else if (operationName == "COUNT")
            {
                return GetCountOperation();
            } else if (operationName == "FILTER")
            {
                return GetFilterOperation(Int32.Parse(args[0]), args[1], args[2]);
            } else if (operationName == "CUSTOM")
            {
                return GetCustomOperation(args[0], args[1], args[2]);
            } else
            {
                return null;
            }
        }
        public static ProcessDelegate GetDupOperation()
        {
            return new ProcessDelegate((x) => new IList<string>[] { x });
        }

        public static ProcessDelegate GetUniqOperation(int fieldNumber)
        {
            return new ProcessDelegate((x) =>
            {
                var seenTuples = ReplicaInstance.SeenTupleFieldValues;
                
                var isUnique = seenTuples.TryAdd(x[fieldNumber], true);
                if (isUnique)
                {
                    return new IList<string>[0];
                } else
                {
                    return new IList<string>[] { x };
                }
            });
        }

        public static ProcessDelegate GetCountOperation()
        {

            return new ProcessDelegate((x) =>
            {
                // thread-safe
                int count = Interlocked.Increment(ref ReplicaInstance.totalSeenTuples);
                return new IList<string>[] { new List<string> { count.ToString() } };
            });
        }

        public static ProcessDelegate GetFilterOperation(int fieldNumber, string comparator, string value)
        {
            if (comparator == "=")
            {
                return new ProcessDelegate((x) => x[fieldNumber] == value ? new IList<string>[] { x } : new IList<string>[0]);
            } else if (comparator == ">")
            {
                return new ProcessDelegate((x) => x[fieldNumber].CompareTo(value) > 0 ? new IList<string>[] { x } : new IList<string>[0]);
            } else if (comparator == "<")
            {
                return new ProcessDelegate((x) => x[fieldNumber].CompareTo(value) > 0 ? new IList<string>[] { x } : new IList<string>[0]);
            } else
            {
                return GetDupOperation();
            }
        }

        public static ProcessDelegate GetCustomOperation(string dll, string className, string method)
        {
            var dllFile = new FileInfo(dll);
            dll = dllFile.FullName;
            Assembly DLL = null;
            try
            {
                DLL = Assembly.LoadFile(dll);
            }
            catch (Exception e)
            {
                // could not load dll. maybe it is in the same folder?
            }

            Type theType;
            if (DLL == null)
            {
                theType = Type.GetType($"{className},{dll.Split('.')[0]}");
            } else
            {
                theType = DLL.GetType(className);
            }
            
            //var c = Activator.CreateInstance(theType);
            var m = theType.GetMethod(method, new[] { typeof(IList<string>) });

            return new ProcessDelegate((x) =>
            {

                return (IList<string>[]) m.Invoke(null, new object[] {x});
            });

        }
    }
}
