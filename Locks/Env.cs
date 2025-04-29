using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    class Env
    {
        public Env enclosing { get; private set; }
        private Env closure;
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public Env()
        {
            this.closure = null;
            this.enclosing = null;
        }
        public Env(Env enclosing)
        {
            this.enclosing = enclosing;
            this.closure = null;
        }
        public Env(Env enclosing, Env closure)
        {
            this.enclosing = enclosing;
            this.closure = closure;
        }

        public object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
                return values[name.lexeme];
            else if(enclosing != null)
                return enclosing.Get(name);
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).values[name];
        }

        public void Define(string name, object value)
        {
            values[name] = value;
        }
        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }
            if(enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).Assign(name, value);
        }
        public Env Ancestor(int distance)
        {
            Env env = this;
            for(int i = 0; i < distance; i++)
            {
                env = env.enclosing;
            }
            return env;
        }
    }
}
