namespace Lox
{
	using System.Collections.Generic;
	
	internal abstract class Stmt
	{
		internal interface Visitor<T>
		{
			T VisitBlockStmt(Block stmt);
			T VisitClassStmt(Class stmt);
			T VisitExpressionStmt(Expression stmt);
			T VisitFunctionStmt(Function stmt);
			T VisitIfStmt(If stmt);
			T VisitPrintStmt(Print stmt);
			T VisitReturnStmt(Return stmt);
			T VisitVarStmt(Var stmt);
			T VisitWhileStmt(While stmt);
		}

		internal abstract T Accept<T>(Visitor<T> visitor);
		
		internal class Block: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitBlockStmt(this);
			
			internal List<Stmt> statements { get; set; }
		}

		internal class Class: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitClassStmt(this);
			
			internal Token name { get; set; }
			internal Expr.Variable superClass { get; set; }
			internal List<Stmt.Function> methods { get; set; }
			internal List<Stmt.Function> statics { get; set; }
		}

		internal class Expression: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitExpressionStmt(this);
			
			internal Expr expression { get; set; }
		}

		internal class Function: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitFunctionStmt(this);
			
			internal Token name { get; set; }
			internal List<Token> parameters { get; set; }
			internal List<Stmt> body { get; set; }
		}

		internal class If: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitIfStmt(this);
			
			internal Expr condition { get; set; }
			internal Stmt thenBranch { get; set; }
			internal Stmt elseBranch { get; set; }
		}

		internal class Print: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitPrintStmt(this);
			
			internal Expr expression { get; set; }
		}

		internal class Return: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitReturnStmt(this);
			
			internal Token keyword { get; set; }
			internal Expr value { get; set; }
		}

		internal class Var: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitVarStmt(this);
			
			internal Token Name { get; set; }
			internal Expr Initializer { get; set; }
		}

		internal class While: Stmt
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitWhileStmt(this);
			
			internal Expr condition { get; set; }
			internal Stmt body { get; set; }
		}
	}
}