using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Class : Callable
    {
        public string name { get; private set; }
        public Dictionary<string, FunctionCallable> methods { get; private set; }
        public Class superclass { get; private set; }
        public Class(string name, Class superclass, Dictionary<string, FunctionCallable> methods)
        {
            this.name = name;
            this.superclass = superclass;
            this.methods = methods;
        }
        public FunctionCallable FindMethod(string name)
        {
            if(methods.ContainsKey(name))
                return methods[name];
            if(superclass != null)
                return superclass.FindMethod(name);

            return null;
        }

        public int Arity()
        {
            Callable init = FindMethod("init");
            if (init != null)
                return init.Arity();
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Instance instance = new Instance(this);
            FunctionCallable init = FindMethod("init");
            if (init != null)
            {
                init.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public override string ToString()
        {
            return $"<cls {name}>";
        }
    }
}
