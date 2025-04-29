using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Instance
    {
        public Class cls { get; private set; }
        private Dictionary<string, object> fields;
        public Instance(Class cls)
        {
            this.cls = cls;
            this.fields = new Dictionary<string, object>();

        }
        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            FunctionCallable method = cls.FindMethod(name.lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
        }
        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }
        public override string ToString()
        {
            return $"{cls.name} instance";
        }
    }
}
