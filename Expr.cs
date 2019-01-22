namespace Lox
{
	using System.Collections.Generic;
	
	internal abstract class Expr
	{
		internal interface Visitor<T>
		{
			T VisitBinaryExpr(Binary expr);
			T VisitCallExpr(Call expr);
			T VisitGetExpr(Get expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			T VisitSetExpr(Set expr);
			T VisitSuperExpr(Super expr);
			T VisitThisExpr(This expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
			T VisitTernaryExpr(Ternary expr);
			T VisitAssignExpr(Assign expr);
		}

		internal abstract T Accept<T>(Visitor<T> visitor);
		
		internal class Binary: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
			
			internal Expr Left { get; set; }
			internal Token Op { get; set; }
			internal Expr Right { get; set; }
		}

		internal class Call: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitCallExpr(this);
			
			internal Expr Calee { get; set; }
			internal Token Paren { get; set; }
			internal List<Expr> Arguments { get; set; }
		}

		internal class Get: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitGetExpr(this);
			
			internal Expr Object { get; set; }
			internal Token Name { get; set; }
		}

		internal class Grouping: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
			
			internal Expr Expression { get; set; }
		}

		internal class Literal: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
			
			internal object Value { get; set; }
		}

		internal class Logical: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitLogicalExpr(this);
			
			internal Expr Left { get; set; }
			internal Token Op { get; set; }
			internal Expr Right { get; set; }
		}

		internal class Set: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitSetExpr(this);
			
			internal Expr Object { get; set; }
			internal Token Name { get; set; }
			internal Expr Value { get; set; }
		}

		internal class Super: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitSuperExpr(this);
			
			internal Token Keyword { get; set; }
			internal Token Method { get; set; }
		}

		internal class This: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitThisExpr(this);
			
			internal Token Keyword { get; set; }
		}

		internal class Unary: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
			
			internal Token Op { get; set; }
			internal Expr Right { get; set; }
		}

		internal class Variable: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitVariableExpr(this);
			
			internal Token Name { get; set; }
		}

		internal class Ternary: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitTernaryExpr(this);
			
			internal Expr Condition { get; set; }
			internal Expr Left { get; set; }
			internal Expr Right { get; set; }
		}

		internal class Assign: Expr
		{
			override internal T Accept<T>(Visitor<T> visitor) => visitor.VisitAssignExpr(this);
			
			internal Token Name { get; set; }
			internal Expr Value { get; set; }
		}
	}
}