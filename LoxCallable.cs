namespace Lox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal interface LoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, IList<object> arguments);
    }

    internal class LoxFunction : LoxCallable
    {
        private int ArgsCount { get; set; }
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        private readonly bool IsInitializer;

        internal string Name => declaration.name.Lexeme;

        public LoxFunction(Stmt.Function declaration, Environment closure, bool IsInitializer = false)
        {
            this.declaration = declaration;
            this.closure = closure;
            this.IsInitializer = IsInitializer;
            ArgsCount = declaration.parameters.Count;
        }

        internal LoxFunction Bind(LoxInstance instance)
        {
            var env = new Environment(closure);
            env.Define("this", instance);
            return new LoxFunction(declaration, env, IsInitializer);
        }

        public int Arity => ArgsCount;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var env = new Environment(closure);
            var argsWithNames = declaration.parameters.Zip(arguments, 
                (name, arg) => (name.Lexeme, arg));
            foreach (var (name, arg) in argsWithNames)
                env.Define(name, arg);
            /* foreach (var (name, arg) in declaration.parameters.EnumerableZip(arguments))
                env.Define(name.Lexeme, arg); */
            try
            {
                interpreter.ExecuteBlock(declaration.body, env);
            }
            catch (Return returnVal)
            {
                if (IsInitializer) return closure.GetAt(0, "init");
                return returnVal.value;
            }
            
            return null;
        }

    }

    abstract class NativeFunction: LoxCallable
    {
        public abstract int Arity { get; }

        public abstract object Call(Interpreter interpreter, IList<object> arguments);

        override public string ToString() => "<native fn>";
    }

    sealed internal class Clock: NativeFunction
    {
        public override int Arity => 0;

        public override object Call(Interpreter interpreter, IList<object> arguments) => 
            (double) System.DateTime.Now.Millisecond / 1000.0;
    }

    sealed internal class Print: NativeFunction
    {
        public override int Arity => 1;

        public override object Call(Interpreter interpreter, IList<object> arguments) 
        {
            Console.WriteLine(arguments[0]);
            return null;
        }
    }
}