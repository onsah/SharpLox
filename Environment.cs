namespace Lox
{
    using System.Collections;
    using System.Collections.Generic;
    internal class Environment: IEnumerable
    {
        private readonly Environment enclosing;
        private readonly IDictionary<string, object> values = new Dictionary<string, object>();

        internal Environment Enclosing => enclosing;

        private IDictionary<string, object> Values => values;

        internal Environment()
        {
            enclosing = null;
        }

        internal Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        internal void Define(string name, object value) => Values.Add(name, value);

        internal object GetAt(int distance, string name) => Ancestor(distance).Values[name];

        internal void AssignAt(int distance, Token name, object value) => Ancestor(distance).Values[name.Lexeme] = value;

        internal Environment Ancestor(int distance)
        {
            var env = this;
            for (int i = 0; i < distance; ++i)
                env = env.Enclosing;
            return env;
        }

        internal void Add(string name, object value) => Define(name, value);

        internal object Get(Token name)
        {
            var key = name.Lexeme;
            return Get(key) ??
            throw new RuntimeError(name, 
                string.Format("Undefined variable '{0}'.", key));
        }

        internal object Get(string name)
        {
            if (Values.ContainsKey(name))
                return Values[name];
            if (enclosing != null)
                return enclosing.Get(name);
            return null;
        }

        internal void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
                Values[name.Lexeme] = value;
            else if (enclosing != null)
                enclosing.Assign(name, value);
            else
                throw new RuntimeError(name, string.Format("Undefined variable '{0}'.", name.Lexeme));
        }

        private IEnumerator GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
    }
}