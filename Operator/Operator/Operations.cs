using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
                    return new IList<string>[] { x };
                } else
                {
                    return new IList<string>[0];
                    
                }
            });
        }

        public static ProcessDelegate GetCountOperation()
        {

            return new ProcessDelegate((x) =>
            {
                int count = -2;

                // thread-safe
                count = Interlocked.Increment(ref ReplicaInstance.totalSeenTuples);
                
                return new IList<string>[] { new List<string> { count.ToString() } };

            });
        }

        public static ProcessDelegate GetFilterOperation(int fieldNumber, string comparator, string value)
        {
            if (comparator == "=")
            {
                return new ProcessDelegate((x) =>
                {
                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(x[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(x[fieldNumber]) == Int32.Parse(value) ? new IList<string>[] { x } : new IList<string>[0];
                    } else
                    {
                        return x[fieldNumber].CompareTo(value) == 0 ? new IList<string>[] { x } : new IList<string>[0];
                    }
                    
                });
            
            } else if (comparator == ">")
            {
                return new ProcessDelegate((x) =>
                {
                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(x[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(x[fieldNumber]) > Int32.Parse(value) ? new IList<string>[] { x } : new IList<string>[0];
                    }
                    else
                    {
                        return x[fieldNumber].CompareTo(value) > 0 ? new IList<string>[] { x } : new IList<string>[0];
                    }

                });
            } else if (comparator == "<")
            {
                return new ProcessDelegate((x) =>
                {
                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(x[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(x[fieldNumber]) < Int32.Parse(value) ? new IList<string>[] { x } : new IList<string>[0];
                    }
                    else
                    {
                        return x[fieldNumber].CompareTo(value) < 0 ? new IList<string>[] { x } : new IList<string>[0];
                    }

                });
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
            
            var c = Activator.CreateInstance(theType);
            var m = theType.GetMethod(method, new[] { typeof(IList<string>) });

            return new ProcessDelegate((x) =>
            {
                try
                {
                    return (IList<IList<string>>)m.Invoke(c, new object[] { x });
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message + ";" + e.InnerException?.Message);
                    return null;
                }
                
            });

        }
    }
}
