namespace Lox
{
    using System.Collections.Generic;

    internal class LoxInstance
    {
        internal LoxClass klass;
        private readonly IDictionary<string, object> fields = new Dictionary<string, object>(); 
        internal IDictionary<string, object> Fields => fields;

        internal LoxInstance(LoxClass klass = null)
        {
            this.klass = klass;
        }

        internal object Get(Token name)
        {
            if (fields.ContainsKey(name.Lexeme))
                return fields[name.Lexeme];
            var method = klass.FindMethod(this, name.Lexeme);
            if (method != null)
                return method;
            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }
        
        internal bool HasField(string name) => fields.ContainsKey(name);

        internal void Set(Token name, object value) => Set(name.Lexeme, value);

        internal void Set(string name, object value) => fields[name] = value;

        override public string ToString() => $"{klass.ToString()} instance";
    }
}