def generateAst(baseName, types):
    tabCount = 1
    result = (  "namespace Lox\n"
                "{\n" +
                addTabs(tabCount, "using System.Collections.Generic;\n") +
                addTabs(tabCount, "\n") +
                addTabs(tabCount, "internal abstract class {}\n".format(baseName)) + 
                addTabs(tabCount, "{\n"))
    tabCount += 1
    result += defineVisitor(baseName, types)
    result += '\n'
    result += addTabs(tabCount, "internal abstract T Accept<T>(Visitor<T> visitor);\n")
    result += addTabs(tabCount, '\n')
    for (name, fields) in types:
        result += defineType(baseName, name, fields)
        result += '\n'
    result = result[:-1]
    tabCount -= 1
    # Class end
    result += "\t}\n"
    # Namespace end
    result += "}\n"
    return result

def defineType(baseName, className, fields):
    tabCount = 2
    result = addTabs(tabCount, "internal class {}: {}\n".format(className, baseName))
    result += addTabs(tabCount, "{\n")
    tabCount += 1
    result += addTabs(tabCount, "override internal T Accept<T>(Visitor<T> visitor) => visitor.Visit{}{}(this);\n".format(className, baseName))
    result += addTabs(tabCount, '\n')
    for field in fields:
        result += addTabs(tabCount, "internal {} {{ get; set; }}\n".format(field))
    tabCount -= 1
    result += addTabs(tabCount, "}\n")
    return result

def defineVisitor(baseName, types):
    tabCount = 2
    result = (  addTabs(tabCount, "internal interface Visitor<T>\n") +
                addTabs(tabCount, "{\n"))
    tabCount += 1
    for (name, _) in types:
        result += addTabs(tabCount, 
            "T Visit{}{}({} {});\n".format(name, baseName, name, str.lower(baseName)))
    tabCount -= 1
    result += addTabs(tabCount, "}\n")
    return result

def addTabs(tabCount, text):
    result = ""
    for _ in range(tabCount):
        result += "\t"
    return result + text

# Expression classes
print(generateAst("Expr", 
            [("Binary", ["Expr Left", "Token Op", "Expr Right"]),
            ("Call", ["Expr Calee", "Token Paren", "List<Expr> Arguments"]),
            ("Get", ["Expr Object", "Token Name"]),
            ("Grouping", ["Expr Expression"]),
            ("Literal", ["object Value"]),
            ("Logical", ["Expr Left", "Token Op", "Expr Right"]),
            ("Set", ["Expr Object", "Token Name", "Expr Value"]),
            ("Super", ["Token Keyword", "Token Method"]),
            ("This", ["Token Keyword"]),
            ("Unary", ["Token Op", "Expr Right"]),
            ("Variable", ["Token Name"]),
            ("Ternary", ["Expr Condition", "Expr Left", "Expr Right"]),
            ("Assign", ["Token Name", "Expr Value"])]))

# Statements classes
""" print(generateAst("Stmt", 
    [("Block", ["List<Stmt> statements"]),
    ("Class", ["Token name", "Expr.Variable superClass", "List<Stmt.Function> methods", "List<Stmt.Function> statics"]),
    ("Expression", ["Expr expression"]),
    ("Function", ["Token name", "List<Token> parameters", "List<Stmt> body"]),
    ("If", ["Expr condition", "Stmt thenBranch", "Stmt elseBranch"]),
    ("Print", ["Expr expression"]),
    ("Return", ["Token keyword", "Expr value"]),
    ("Var", ["Token Name", "Expr Initializer"]),
    ("While", ["Expr condition", "Stmt body"])]
)) """