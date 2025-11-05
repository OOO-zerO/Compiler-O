using System;
using System.Collections.Generic;

public abstract class AstNode
{
    public int Line { get; }
    public int Column { get; }

    protected AstNode(int line, int column)
    {
        Line = line;
        Column = column;
    }
}

public sealed class ProgramNode : AstNode
{
    public System.Collections.Generic.List<ClassDeclNode> Classes { get; }
    public ProgramNode(System.Collections.Generic.List<ClassDeclNode> classes) : base(0, 0)
    {
        Classes = classes;
    }
}

public sealed class ClassDeclNode : AstNode
{
    public string Name { get; }
    public string? BaseClassName { get; }
    public System.Collections.Generic.List<StatementNode> ThisStatements { get; }
    public System.Collections.Generic.List<MemberDeclNode> Members { get; }

    public ClassDeclNode(string name, string? baseClassName, System.Collections.Generic.List<StatementNode> thisStatements, System.Collections.Generic.List<MemberDeclNode> members, int line, int column)
        : base(line, column)
    {
        Name = name;
        BaseClassName = baseClassName;
        ThisStatements = thisStatements;
        Members = members;
    }
}

public abstract class MemberDeclNode : AstNode
{
    protected MemberDeclNode(int line, int column) : base(line, column) { }
}

public sealed class VarDeclNode : MemberDeclNode
{
    public string Name { get; }
    public ExprNode Initializer { get; }
    public VarDeclNode(string name, ExprNode initializer, int line, int column) : base(line, column)
    {
        Name = name;
        Initializer = initializer;
    }
}

public sealed class MethodDeclNode : MemberDeclNode
{
    public string Name { get; }
    public System.Collections.Generic.List<ParamNode> Parameters { get; }
    public TypeRefNode? ReturnType { get; }
    public System.Collections.Generic.List<StatementNode> Body { get; }

    public MethodDeclNode(string name, System.Collections.Generic.List<ParamNode> parameters, TypeRefNode? returnType, System.Collections.Generic.List<StatementNode> body, int line, int column)
        : base(line, column)
    {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }
}

public sealed class ParamNode : AstNode
{
    public string Name { get; }
    public TypeRefNode Type { get; }
    public ParamNode(string name, TypeRefNode type, int line, int column) : base(line, column)
    {
        Name = name;
        Type = type;
    }
}

public sealed class TypeRefNode : AstNode
{
    public string Name { get; }
    public TypeRefNode(string name, int line, int column) : base(line, column)
    {
        Name = name;
    }
}

public abstract class StatementNode : AstNode
{
    protected StatementNode(int line, int column) : base(line, column) { }
}

public sealed class LocalVarDeclStmtNode : StatementNode
{
    public string Name { get; }
    public ExprNode Initializer { get; }
    public LocalVarDeclStmtNode(string name, ExprNode initializer, int line, int column) : base(line, column)
    {
        Name = name;
        Initializer = initializer;
    }
}
// a[0] := a[0].Plus(1)
public sealed class AssignStmtNode : StatementNode
{
    public ExprNode Target { get; }
    public ExprNode Value { get; }
    public AssignStmtNode(ExprNode target, ExprNode value, int line, int column) : base(line, column)
    {
        Target = target;
        Value = value;
    }
}

public sealed class IfStmtNode : StatementNode
{
    public ExprNode Condition { get; }
    public System.Collections.Generic.List<StatementNode> ThenBranch { get; }
    public System.Collections.Generic.List<StatementNode>? ElseBranch { get; }
    public IfStmtNode(ExprNode condition, System.Collections.Generic.List<StatementNode> thenBranch, System.Collections.Generic.List<StatementNode>? elseBranch, int line, int column) : base(line, column)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

public sealed class WhileStmtNode : StatementNode
{
    public ExprNode Condition { get; }
    public System.Collections.Generic.List<StatementNode> Body { get; }
    public WhileStmtNode(ExprNode condition, System.Collections.Generic.List<StatementNode> body, int line, int column) : base(line, column)
    {
        Condition = condition;
        Body = body;
    }
}

public sealed class ReturnStmtNode : StatementNode
{
    public ExprNode Expression { get; }
    public ReturnStmtNode(ExprNode expression, int line, int column) : base(line, column)
    {
        Expression = expression;
    }
}

public sealed class ExprStmtNode : StatementNode
{
    public ExprNode Expression { get; }
    public ExprStmtNode(ExprNode expression, int line, int column) : base(line, column)
    {
        Expression = expression;
    }
}

public abstract class ExprNode : AstNode
{
    protected ExprNode(int line, int column) : base(line, column) { }
}

public sealed class IdentifierExprNode : ExprNode
{
    public string Name { get; }
    public IdentifierExprNode(string name, int line, int column) : base(line, column)
    {
        Name = name;
    }
}

public sealed class ThisExprNode : ExprNode
{
    public ThisExprNode(int line, int column) : base(line, column) { }
}

public sealed class IntLiteralExprNode : ExprNode
{
    public string Value { get; }
    public IntLiteralExprNode(string value, int line, int column) : base(line, column)
    {
        Value = value;
    }
}

public sealed class RealLiteralExprNode : ExprNode
{
    public string Value { get; }
    public RealLiteralExprNode(string value, int line, int column) : base(line, column)
    {
        Value = value;
    }
}

public sealed class BoolLiteralExprNode : ExprNode
{
    public bool Value { get; }
    public BoolLiteralExprNode(bool value, int line, int column) : base(line, column)
    {
        Value = value;
    }
}

// param1.Plus
// a[i].fieldName
public sealed class MemberAccessExprNode : ExprNode
{
    public ExprNode Target { get; }
    public string MemberName { get; }
    public MemberAccessExprNode(ExprNode target, string memberName, int line, int column) : base(line, column)
    {
        Target = target;
        MemberName = memberName;
    }
}

public sealed class CallExprNode : ExprNode
{
    public ExprNode Callee { get; }
    public System.Collections.Generic.List<ExprNode> Arguments { get; }
    public CallExprNode(ExprNode callee, System.Collections.Generic.List<ExprNode> arguments, int line, int column) : base(line, column)
    {
        Callee = callee;
        Arguments = arguments;
    }
}


