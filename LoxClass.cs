namespace Lox
{
    using System.Collections.Generic;
    
    internal class LoxClass: LoxInstance, LoxCallable
    {
        private readonly string name;
        private readonly IDictionary<string, LoxFunction> methods;
        readonly LoxClass superClass;

        public LoxClass(string name, LoxClass superClass, IDictionary<string, LoxFunction> methods, IEnumerable<LoxFunction> statics)
        {
            this.klass = this;
            this.name = name;
            this.methods = methods;
            this.superClass = superClass;
            foreach (var func in statics)
                Set(func.Name, func);
        }

        public int Arity
        {
            get 
            {
                LoxFunction function;
                if (methods.TryGetValue("init", out function))
                    return function.Arity;
                return 0;
            }
        }

        public string Name => name;

        internal LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (HasField(name)) 
            {
                var function = Fields[name] as LoxFunction;
                if (function != null)
                    return function;
            }
            if (methods.ContainsKey(name))
                return methods[name].Bind(instance);
            if (superClass != null)
                return superClass.FindMethod(instance, name);
            return null;
        }

        // Implemementation of constructor
        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var instance = new LoxInstance(this);
            LoxFunction init;
            if (methods.TryGetValue("init", out init))
                init.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        override public string ToString() => Name; 
    }
}