using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

public static class Runner
{
    public static int Main(string[] args)
    {
        var asm = Assembly.LoadFrom(args[0]);
        string filter = args.Length > 1 ? args[1] : null;
        int pass = 0, fail = 0;
        foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            if (filter != null && type.Name != filter) continue;
            foreach (var m in type.GetMethods())
            {
                var cases = m.GetCustomAttributes<TestCaseAttribute>().Select(c => c.Arguments).ToList();
                if (m.GetCustomAttribute<TestAttribute>() != null) cases.Add(new object[0]);
                foreach (var a in cases)
                {
                    var name = type.Name + "." + m.Name + (a.Length > 0 ? "(" + string.Join(",", a) + ")" : "");
                    try
                    {
                        var inst = Activator.CreateInstance(type);
                        var ps = m.GetParameters();
                        var conv = a.Select((x, i) => x == null ? null : Convert.ChangeType(x, ps[i].ParameterType)).ToArray();
                        m.Invoke(inst, conv);
                        pass++;
                    }
                    catch (Exception e)
                    {
                        var inner = e is TargetInvocationException t ? t.InnerException : e;
                        fail++;
                        Console.WriteLine("FAIL " + name + ": " + inner.GetType().Name + ": " + inner.Message.Trim());
                    }
                }
            }
        }
        Console.WriteLine($"passed {pass}, failed {fail}");
        return fail == 0 ? 0 : 1;
    }
}
