using System;
using System.Collections.Concurrent;
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
    public class Operation
    {
        public ProcessDelegate Delegate;
        public object InternalState;
        public IList<object> args;
        public IEnumerable<IList<string>> Process(IList<string> arg)
        {
            return Delegate(arg, ref InternalState, args);
        }
    }

    class Operations
    {
        public static Replica ReplicaInstance { get; set; }

        public static Operation GetOperation(string operationName, IList<string> args = null)
        {
            var result = new Operation();
            
            if (operationName == "DUP")
            {
                result.Delegate = new ProcessDelegate(Dup);
            }
            else if (operationName == "UNIQ")
            {
                result.args = new List<object>();
                result.args.Add(Int32.Parse(args[0]));
                result.InternalState = new ConcurrentDictionary<string, bool>();
                result.Delegate = new ProcessDelegate(Uniq);
            }
            else if (operationName == "COUNT")
            {
                result.InternalState = 0;
                result.Delegate = new ProcessDelegate(Count);
            }
            else if (operationName == "FILTER")
            {
                result.args = new List<object>();
                result.args.Add(Int32.Parse(args[0]));
                result.args.Add(args[1]);
                result.args.Add(args[2]);
                result.Delegate = new ProcessDelegate(Filter);
            }
            else if (operationName == "CUSTOM")
            {
                var theType = LoadType(args[0], args[1]);
                var c = Activator.CreateInstance(theType);
                var m = theType.GetMethod(args[2], new[] { typeof(IList<string>) });
                result.InternalState = c;
                result.args = new List<object> { m };
                result.Delegate = new ProcessDelegate(Custom);
            }
            else
            {
                return null;
            }
            return result;
        }


        public static Type LoadType(string dll, string className)
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
            }
            else
            {
                theType = DLL.GetType(className);
            }
            return theType;
        }

        public static IEnumerable<IList<string>> Dup(IList<string> arg, ref object state, IList<object> args)
        {
            return new IList<string>[] { arg };
        }


        public static IEnumerable<IList<string>> Count(IList<string> arg, ref object state, IList<object> args)
        {        // not thread-safe!!
            state = ((int)state) + 1;

            return new IList<string>[] { new List<string> { state.ToString() } };
        }

        public static IEnumerable<IList<string>> Custom(IList<string> tuple, ref object state, IList<object> args)
        {
            var m = (MethodInfo)args[0];
            try
            {
                return (IList<IList<string>>)m.Invoke(state, new object[] { tuple });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ";" + e.InnerException?.Message);
                return new List<IList<string>>();
            }
        }

        public static IEnumerable<IList<string>> Uniq(IList<string> arg, ref object state, IList<object> args)
        {
            var seenTuples = (ConcurrentDictionary<string, bool>)state;

            var isUnique = seenTuples.TryAdd(arg[(int)args[0]], true);
            if (isUnique)
            {
                return new IList<string>[] { arg };
            }
            else
            {
                return new IList<string>[0];
            }
        }
        public static IEnumerable<IList<string>> Filter(IList<string> tuple, ref object state, IList<object> args)
        {
            var comparator = (string) args[1];
            var fieldNumber = (int) args[0];
            var value = (string) args[2];
            if (comparator == "=")
            {

                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(tuple[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(tuple[fieldNumber]) == Int32.Parse(value) ? new IList<string>[] { tuple } : new IList<string>[0];
                    }
                    else
                    {
                        return tuple[fieldNumber].CompareTo(value) == 0 ? new IList<string>[] { tuple } : new IList<string>[0];
                    }

                

            }
            else if (comparator == ">")
            {
                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(tuple[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(tuple[fieldNumber]) > Int32.Parse(value) ? new IList<string>[] { tuple } : new IList<string>[0];
                    }
                    else
                    {
                        return tuple[fieldNumber].CompareTo(value) > 0 ? new IList<string>[] { tuple } : new IList<string>[0];
                    }
            }
            else if (comparator == "<")
            {
                    if (Regex.Match(value, @"^\d+$").Success && Regex.Match(tuple[fieldNumber], @"^\d+$").Success)
                    {
                        return Int32.Parse(tuple[fieldNumber]) < Int32.Parse(value) ? new IList<string>[] { tuple } : new IList<string>[0];
                    }
                    else
                    {
                        return tuple[fieldNumber].CompareTo(value) < 0 ? new IList<string>[] { tuple } : new IList<string>[0];
                    }
            }
            else
            {
                throw new Exception("Wrong comparator on operator FILTER");
            }
        }
    }
}
