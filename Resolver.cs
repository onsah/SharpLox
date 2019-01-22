namespace Lox
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    internal class Resolver : Stmt.Visitor<Void>, Expr.Visitor<Void>
    {
        private readonly Interpreter interpreter;
        // 'false' means value has not initalized
        private readonly Stack<IDictionary<string, ScopeVariable>> scopes = new Stack<IDictionary<string, ScopeVariable>>();
        private FuncType currentFunction = FuncType.NONE; 
        private ClassType currentClass = ClassType.NONE; 

        private enum FuncType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }

        class ScopeVariable
        {
            public Token defineToken;
            public bool initialized;
            public bool used;

            public ScopeVariable(Token token)
            {
                defineToken = token;
                initialized = false;
                used = false;
            }
        }
        
        internal Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public Void VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public Void VisitClassStmt(Stmt.Class stmt)
        {
            var enclosing = currentClass;
            currentClass = ClassType.CLASS;
            Declare(stmt.name);
            if (stmt.superClass != null)
                Resolve(stmt.superClass);
            Define(stmt.name);
            if (stmt.superClass != null)
            {
                BeginScope();
                scopes.Peek().Add("super", new ScopeVariable(stmt.superClass.Name) { initialized = true, used = true } );
            }
            BeginScope();
            foreach (var method in stmt.statics)
                ResolveFunction(method, FuncType.FUNCTION);
            scopes.Peek().Add("this", new ScopeVariable(stmt.name) { initialized = true, used =  true });
            foreach (var method in stmt.methods)
            {
                var declaration = FuncType.METHOD;
                if (method.name.Lexeme == "init")
                    declaration = FuncType.INITIALIZER;
                ResolveFunction(method, declaration);
            }
            EndScope();
            if (stmt.superClass != null)
                EndScope();
            currentClass = enclosing;
            return null;
        }

        public Void VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);
            ResolveFunction(stmt, FuncType.FUNCTION);
            return null;
        }

        public Void VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null)
                Resolve(stmt.elseBranch);
            return null;
        }

        public Void VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public Void VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FuncType.NONE)
                Program.Error(stmt.keyword, "Cannot return from top-level code.");
            if (stmt.value != null)
            {
                if (currentFunction == FuncType.INITIALIZER)
                    Program.Error(stmt.keyword, "Cannot return a value from an initializer.");
                Resolve(stmt.value);
            }
            return null;
        }

        public Void VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
                Resolve(stmt.Initializer);
            Define(stmt.Name);
            return null;
        }

        public Void VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0)
                return;
            var scope = scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
                Program.Error(name, 
                    "Variable with this name already declared in this scope.");
            scope.Add(name.Lexeme, new ScopeVariable (name) { initialized = false });
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0)
                return;
            var scope = scopes.Peek();
            scope[name.Lexeme].initialized = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            foreach (var (scope, depth) in 
                scopes.Zip(Enumerable.Range(0, scopes.Count).Reverse(), (s, d) => (s, d))) 
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - depth - 1);
                    return;
                }
            }
            // TODO
        }

        private void BeginScope() => scopes.Push(new Dictionary<string, ScopeVariable>());

        private void EndScope() 
        {
            var scope = scopes.Peek();
            foreach (var scoperVar in scope.Values)
                if (!scoperVar.used)
                    Program.Error(scoperVar.defineToken, "Variable defined but not used");   
            scopes.Pop();
        }

        internal void Resolve(IEnumerable<Stmt> stmts)
        {
            foreach (var stmt in stmts)
                Resolve(stmt);
        }

        private void Resolve(Stmt stmt) => stmt.Accept(this);

        private void Resolve(Expr expr) => expr.Accept(this);

        private void ResolveFunction(Stmt.Function function, FuncType type)
        {
            var enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (var param in function.parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        public Void VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public Void VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Calee);
            foreach (var arg in expr.Arguments)
                Resolve(arg);
            return null;
        }

        public Void VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Object);
            return null;
        }

        public Void VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public Void VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public Void VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public Void VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public Void VisitSuperExpr(Expr.Super expr)
        {
            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public Void VisitThisExpr(Expr.This expr)
        {
            if (!InClass())
                Program.Error(expr.Keyword, "Can't use 'this' outside of a class definition");
            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public Void VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public Void VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count != 0 && 
                scopes.Peek().ContainsKey(expr.Name.Lexeme))
            {
                var scoperVar = scopes.Peek()[expr.Name.Lexeme]; 
                if (!scoperVar.initialized)
                {
                    Program.Error(expr.Name, 
                        "Cannot read local variable in its own initializer");
                }
                scoperVar.used = true;
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public Void VisitTernaryExpr(Expr.Ternary expr)
        {
            Resolve(expr.Condition);
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public Void VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        private bool InClass() => currentClass == ClassType.CLASS;
    }
}