using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.Serialization;

public class SemanticAnalyzer
{
    private readonly Stack<string> _errors = new Stack<string>();
    private SymbolTable _symbolTable = new SymbolTable();
    private readonly Stack<Dictionary<string, string>> _typeScopes = new Stack<Dictionary<string, string>>();


    private bool _insideLoop = false;
    private bool _insideMethod = false;


    private void AddError(string message, int line, int column)
    {
        _errors.Push($"[Line {line}:{column}] {message}");
    }

    // recheck the full code with marking errors
    public Stack<string> Analyze(ProgramNode program)
    {
        _errors.Clear();
        _symbolTable = new SymbolTable();
        _typeScopes.Clear();
        EnterTypeScope();
        // Register simple built-ins
        var builtinWrite = new MethodDeclNode(
            "write",
            new System.Collections.Generic.List<ParamNode>(),
            null,
            new System.Collections.Generic.List<StatementNode>(),
            0,
            0);
        _symbolTable.AddSymbol("write", new SymbolInfo(SymbolType.Method, builtinWrite));

        VisitProgram(program);

        ExitTypeScope();

        // If there are no semantic errors, perform AST-level optimizations
        if (_errors.Count == 0)
        {
            AstOptimizer.Optimize(program);
        }

        return _errors;
    }

    private void VisitProgram(ProgramNode node)
    {
        // First register all class names in the global scope
        foreach (var classDecl in node.Classes)
        {
            if (!_symbolTable.AddSymbol(
                classDecl.Name,
                new SymbolInfo(SymbolType.Class, classDecl)
            ))
            {
                AddError($"Duplicate class declaration: {classDecl.Name}", classDecl.Line, classDecl.Column);
            }
        }

        // Second analyze class contents in their own scopes
        foreach (var classDecl in node.Classes)
        {
            VisitClass(classDecl);
        }
    }

    private void VisitClass(ClassDeclNode node)
    {
        _symbolTable.EnterScope();
        EnterTypeScope();
        if (node.BaseClassName != null && !_symbolTable.isSymbolDefined(node.BaseClassName))
        {
            AddError($"Base class not found: {node.BaseClassName}", node.Line, node.Column);
        }

        // Analyze members in class
        foreach (var member in node.Members)
        {
            if (member is VarDeclNode varDecl)
            {
                VisitVarDecl(varDecl); // variables
            }
            else if (member is MethodDeclNode methodDecl)
            {
                VisitMethodDecl(methodDecl); // methods
            }
        }

        foreach (var stmt in node.ThisStatements)
        {
            VisitStatement(stmt);
        }
        _symbolTable.ExitScope();
        ExitTypeScope();
    }

    private void VisitMethodDecl(MethodDeclNode node)
    {
        _symbolTable.EnterScope();
        EnterTypeScope();

        bool oldInsideMethod = _insideMethod;
        _insideMethod = true;

        if (!_symbolTable.AddSymbol(node.Name, new SymbolInfo(SymbolType.Method, node)))
        {
            AddError($"Duplicate method name: {node.Name}", node.Line, node.Column);
        }

        foreach (var param in node.Parameters)
        {
            if (!_symbolTable.AddSymbol(
                param.Name,
                new SymbolInfo(SymbolType.Parameter, param)
            ))
            {
                AddError($"Duplicate parameter name: {param.Name}", param.Line, param.Column);
            }
            if (param.Type != null)
            {
                DefineType(param.Name, param.Type.Name);
            }
        }

        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }

        _insideMethod = oldInsideMethod;
        _symbolTable.ExitScope();
        ExitTypeScope();
    }

    private void VisitVarDecl(VarDeclNode node)
    {
        if (!_symbolTable.AddSymbol(
            node.Name,
            new SymbolInfo(SymbolType.Variable, node)
        ))
        {
            AddError($"Duplicate variable name: {node.Name}", node.Line, node.Column);
        }

        VisitExpression(node.Initializer);
        var initType = InferExpressionType(node.Initializer);
        if (initType != null)
        {
            DefineType(node.Name, initType);
        }
    }

    private void VisitStatement(StatementNode node)
    {
        switch (node)
        {
            case LocalVarDeclStmtNode localVar:
                VisitLocalVarDecl(localVar);
                break;
            case AssignStmtNode stmtNode:
                VisitAssignStmt(stmtNode);
                break;
            case IfStmtNode ifStmtNode:
                VisitIfStmt(ifStmtNode);
                break;
            case WhileStmtNode whileStmtNode:
                VisitWhileStmt(whileStmtNode);
                break;
            case ReturnStmtNode returnStmtNode:
                VisitReturnStmt(returnStmtNode);
                break;
            case ExprStmtNode exprStmtNode:
                VisitExpression(exprStmtNode.Expression);
                break;
            case BreakStmtNode breakStmt:
                CheckBreakUsage(breakStmt.Line, breakStmt.Column);
                break;
        }
    }

    private void CheckBreakUsage(int line, int column)
    {
        if (!_insideLoop)
        {
            AddError("Break statement can only be used inside loops", line, column);
        }
    }

    private void VisitLocalVarDecl(LocalVarDeclStmtNode node)
    {
        if (!_symbolTable.AddSymbol(
            node.Name,
            new SymbolInfo(SymbolType.Variable, node)
        ))
        {
            AddError($"Duplicate local variable name: {node.Name}", node.Line, node.Column);
        }

        // check expr init
        VisitExpression(node.Initializer);
        var initType = InferExpressionType(node.Initializer);
        if (initType != null)
        {
            DefineType(node.Name, initType);
        }
    }

    private void VisitAssignStmt(AssignStmtNode node)
    {
        if (node.Target is IdentifierExprNode identifierExprNode)
        {
            if (!_symbolTable.isSymbolDefined(identifierExprNode.Name))
            {
                AddError($"Undeclared variable: {identifierExprNode.Name}", identifierExprNode.Line, identifierExprNode.Column);
            }
            else
            {
                var targetType = LookupType(identifierExprNode.Name);
                VisitExpression(node.Value);
                var valueType = InferExpressionType(node.Value);
                if (targetType != null && valueType != null && targetType != valueType)
                {
                    AddError($"Type mismatch: cannot assign {valueType} to {targetType}", node.Value.Line, node.Value.Column);
                }
                return;
            }
        }
        else
        {
            AddError("Assignment target must be a variable", node.Line, node.Column);
        }

        VisitExpression(node.Value);
    }

    private void VisitExpression(ExprNode node)
    {
        switch (node)
        {
            case IdentifierExprNode id:
                if (!_symbolTable.isSymbolDefined(id.Name))
                {
                    AddError($"Undeclared identifier: {id.Name}", id.Line, id.Column);
                }
                break;
            case MemberAccessExprNode member:
                VisitExpression(member.Target);
                break;
            case CallExprNode call:
                VisitExpression(call.Callee);
                foreach (var arg in call.Arguments)
                {
                    VisitExpression(arg);
                }
                break;
            case BinaryExprNode bin:
                VisitExpression(bin.Left);
                VisitExpression(bin.Right);
                break;
            case IntLiteralExprNode:
            case RealLiteralExprNode:
            case BoolLiteralExprNode:
            case ThisExprNode:
                break;
        }
    }

    private void VisitIfStmt(IfStmtNode node)
    {
        VisitExpression(node.Condition);

        if (node.Condition is BoolLiteralExprNode boolCond)
        {
            if (boolCond.Value)
            {
                AddError($"Condition is always true - 'if (true)' can be simplified", node.Line, node.Column);
            }
            else
            {
                AddError($"Condition is always false - 'if (false)' is dead code", node.Line, node.Column);
            }
        }

        foreach (var stmt in node.ThenBranch)
        {
            VisitStatement(stmt);
        }

        if (node.ElseBranch != null)
        {
            foreach (var stmt in node.ElseBranch)
            {
                VisitStatement(stmt);
            }
        }
    }

    private void VisitWhileStmt(WhileStmtNode node)
    {
        VisitExpression(node.Condition);
        
        if (node.Body.Count == 0)
        {
            AddError("Empty while loop - loop body is empty", node.Line, node.Column);
        }
        
        if (node.Condition is BoolLiteralExprNode boolCond)
        {
            if (!boolCond.Value)
            {
                AddError("Loop condition is always false - while loop will never execute", node.Line, node.Column);
            }
            else if (boolCond.Value)
            {
                if (!HasBreakStatement(node.Body))
                {
                    AddError("Infinite loop detected - 'while (true)' without break statement", node.Line, node.Column);
                }
            }
        }

        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }
    }

    private bool HasBreakStatement(System.Collections.Generic.List<StatementNode> statements)
    {
        foreach (var stmt in statements)
        {
            if (stmt is BreakStmtNode) return true;
            
            if (stmt is IfStmtNode ifStmt)
            {
                if (HasBreakStatement(ifStmt.ThenBranch)) return true;
                if (ifStmt.ElseBranch != null && HasBreakStatement(ifStmt.ElseBranch)) return true;
            }
            
            if (stmt is WhileStmtNode whileStmt)
            {
                if (HasBreakStatement(whileStmt.Body)) return true;
            }
        }
        return false;
    }

    private void VisitReturnStmt(ReturnStmtNode node)
    {
        if (!_insideMethod)
        {
            AddError("Return statement can only be used inside methods", node.Line, node.Column);
        }
        VisitExpression(node.Expression);
    }

    private void EnterTypeScope()
    {
        _typeScopes.Push(new Dictionary<string, string>());
    }

    private void ExitTypeScope()
    {
        if (_typeScopes.Count > 0) _typeScopes.Pop();
    }

    private void DefineType(string name, string typeName)
    {
        var current = _typeScopes.Peek();
        current[name] = typeName;
    }

    private string? LookupType(string name)
    {
        foreach (var scope in _typeScopes)
        {
            if (scope.TryGetValue(name, out var t)) return t;
        }
        return null;
    }

    private string? InferExpressionType(ExprNode node)
    {
        switch (node)
        {
            case IntLiteralExprNode:
                return "Integer";
            case RealLiteralExprNode:
                return "Real";
            case BoolLiteralExprNode:
                return "Boolean";
            case IdentifierExprNode id:
                return LookupType(id.Name);
            case CallExprNode call when call.Callee is IdentifierExprNode cid:
                // Constructors used as expressions: Integer(10), Boolean(false), Real(1.23)
                return cid.Name switch
                {
                    "Integer" => "Integer",
                    "Real" => "Real",
                    "Boolean" => "Boolean",
                    _ => null
                };
            case BinaryExprNode bin:
                var lt = InferExpressionType(bin.Left);
                var rt = InferExpressionType(bin.Right);
                if (bin.Operator == BinaryOperator.Equal || bin.Operator == BinaryOperator.NotEqual
                    || bin.Operator == BinaryOperator.GreaterThan || bin.Operator == BinaryOperator.LessThan
                    || bin.Operator == BinaryOperator.GreaterThanOrEqual || bin.Operator == BinaryOperator.LessThanOrEqual)
                {
                    // comparisons yield Boolean when operands are known
                    if (lt != null && rt != null) return "Boolean";
                    return null;
                }
                // arithmetic: if any Real -> Real, else Integer (when known)
                if (lt == null || rt == null) return null;
                if (lt == "Real" || rt == "Real") return "Real";
                if (lt == "Integer" && rt == "Integer") return "Integer";
                return null;
            case CallExprNode:
            case MemberAccessExprNode:
            case ThisExprNode:
            default:
                return null;
        }
    }
}