namespace Lox
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using static TokenType;
    internal class Interpreter : Expr.Visitor<object>, Stmt.Visitor<Void>
    {
        internal readonly Environment Globals = new Environment 
        { 
            {"clock", new Clock()},
            {"printNative", new Print()}
        };
        internal Environment Environment { get; set; }
        private readonly IDictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            Environment = Globals;
        }

        internal void Interpret(IEnumerable<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                    Execute(stmt);
            }
            catch (RuntimeError error)
            {
                Lox.Program.RuntimeError(error);
            }
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type) 
            {
                case GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left > (double) right;
                case GREATER_EQUAL:                    
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left >= (double) right;
                case LESS:                             
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left < (double) right; 
                case LESS_EQUAL:                       
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left <= (double) right;
                case MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left - (double) right;
                case PLUS:
                    if (left is double && right is double)
                        return (double) left + (double) right;
                    if (left is string && right is string)
                        return (string) left + (string) right;
                    break;
                case SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left / (double) right;
                case STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double) left * (double) right;
                case BANG_EQUAL: 
                    return !IsEqual(left, right);
                case EQUAL_EQUAL: 
                    return IsEqual(left, right);
            }
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var calee = Evaluate(expr.Calee);
            var arguments = expr.Arguments.Select(arg => Evaluate(arg)).ToList();
            var callable = calee as LoxCallable;
            if (callable == null)
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
            if (callable.Arity != arguments.Count())
                throw new RuntimeError(expr.Paren, $"Expected {callable.Arity} arguments but got {arguments.Count()}");
            return callable.Call(this, arguments); 
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.Object);
            var instance = obj as LoxInstance;
            if (instance != null)
                return instance.Get(expr.Name);
            throw new RuntimeError(expr.Name, "Only instances have properties.");
        }

        public object VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expression);

        public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Op.Type == OR)
            {    
                if (ToBool(left))
                    return left;
            }
            else // AND
            {
                if (!ToBool(left))
                    return left;
            }
            return Evaluate(expr.Right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var instance = Evaluate(expr.Object) as LoxInstance;
            if (instance == null)
                throw new RuntimeError(expr.Name, "Only instances have fields");
            var value = Evaluate(expr.Value);
            instance.Set(expr.Name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            var distance = locals[expr];
            var superClass = (LoxClass) Environment.GetAt(distance, "super");
            var self = (LoxInstance) Environment.GetAt(distance - 1, "this");
            var method = superClass.FindMethod(self, expr.Method.Lexeme);
            return method;
        }

        public object VisitThisExpr(Expr.This expr) => LookUpVariable(expr.Keyword, expr);        

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type) 
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return - (double) right;
                case TokenType.BANG:
                    return !ToBool(right);                
            }

            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr) => LookUpVariable(expr.Name, expr);

        private object LookUpVariable(Token name, Expr expr)
        {
            int distance;
            if (locals.TryGetValue(expr, out distance))
            {
                // Console.WriteLine($"distance: {distance}");
                return Environment.GetAt(distance, name.Lexeme);
            }
            else 
                return Globals.Get(name);
        }

        public object VisitTernaryExpr(Expr.Ternary expr)
        {
            if (ToBool(Evaluate(expr.Condition)))
                return Evaluate(expr.Left);
            else
                return Evaluate(expr.Right);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);
            int distance;
            if (locals.TryGetValue(expr, out distance))
                Environment.AssignAt(distance, expr.Name, value);
            else
                Globals.Assign(expr.Name, value);
            return value;
        }

        private object Evaluate(Expr expr) => expr.Accept(this);

        private void Execute(Stmt stmt) => stmt.Accept(this);

        internal void Resolve(Expr expr, int depth) => locals.Add(expr, depth);

        internal void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var prev = Environment;
            try
            {
                Environment = environment;
                foreach (var stmt in statements)
                    Execute(stmt);
            }
            finally
            {
                Environment = prev;
            }
        }

        private bool ToBool(object obj)
        {
            if (obj == null) 
                return false;
            if (obj is bool) 
                return (bool) obj;
            return true;
        }

        private bool IsEqual(object a, object b) => (a == null) ? (b == null) : a.Equals(b);

        private string Stringfy(object obj) => obj?.ToString() ?? "null";

        private void CheckNumberOperand(Token op, object operand)
        {
            if (!(operand is double))
                throw new RuntimeError(op, "Operand must be a number");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (!(left is double) || !(right is double))
                throw new RuntimeError(op, "Operands must be numbers");
        }

        public Void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public Void VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, Environment);
            Environment.Define(stmt.name.Lexeme, function);
            return null;
        }

        public Void VisitIfStmt(Stmt.If stmt)
        {
            if (ToBool(Evaluate(stmt.condition)))
                Execute(stmt.thenBranch);
            else if (stmt.elseBranch != null)
                Execute(stmt.elseBranch);
            return null;
        }

        public Void VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.expression);
            Console.WriteLine(Stringfy(value));
            return null;
        }

        public Void VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null)
                value = Evaluate(stmt.value);
            throw new Return(value);
        }

        public Void VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
                value = Evaluate(stmt.Initializer);
            Environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public Void VisitWhileStmt(Stmt.While stmt)
        {
            while (ToBool(Evaluate(stmt.condition)))
                Execute(stmt.body);
            return null;
        }

        public Void VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(Environment));
            return null;
        }

        public Void VisitClassStmt(Stmt.Class stmt)
        {
            LoxClass superClass = null;
            if (stmt.superClass != null)
            {
                superClass = Evaluate(stmt.superClass) as LoxClass;
                if (superClass == null)
                    throw new RuntimeError(stmt.superClass.Name,
                        "Superclass must be a class.");
            }
            Environment.Define(stmt.name.Lexeme, null);
            if (superClass != null)
            {
                Environment = new Environment(Environment);
                Environment.Define("super", superClass);
            }
            var methods = stmt.methods
                .Select(m => new LoxFunction(m, Environment, m.name.Lexeme == "init"))
                .ToDictionary(func => func.Name);
            var statics = stmt.statics
                .Select(s => new LoxFunction(s, Environment, false));
            var loxClass = new LoxClass(stmt.name.Lexeme, superClass, methods, statics);
            if (superClass != null)
                Environment = Environment.Enclosing;
            Environment.Assign(stmt.name, loxClass);
            return null;
        }
    }

    internal class Void
    {
        private Void() { }
    }
}