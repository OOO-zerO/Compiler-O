using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.Serialization;

public class SemanticAnalyzer
{
    private readonly Stack<string> _errors = new Stack<string>();
    private SymbolTable _symbolTable = new SymbolTable();

    private void AddError(string message, int line, int column)
    {
        _errors.Append($"[Line {line}:{column}] {message}");
    }

    // recheck the full code with marking errors
    public Stack<string> Analyze(ProgramNode program)
    {
        _errors.Clear();
        _symbolTable = new SymbolTable();

        VisitProgram(program);

        return _errors;
    }

    private void VisitProgram(ProgramNode node)
    {
        foreach (var classDecl in node.Classes)
        {
            _symbolTable.EnterScope();

            // add class to symbols scope 
            if (!_symbolTable.AddSymbol(
                classDecl.Name,
                new SymbolInfo(SymbolType.Class, classDecl)
                ))
            {
                AddError($"Duplicate class declaration: {classDecl.Name}", classDecl.Line, classDecl.Column);
            }

            // check the full class decl
            VisitClass(classDecl);
            _symbolTable.ExitScope();
        }
    }

    private void VisitClass(ClassDeclNode node)
    {
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
    }

    private void VisitMethodDecl(MethodDeclNode node)
    {
        _symbolTable.EnterScope();

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
        }

        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }

        _symbolTable.ExitScope();
    }

    private void VisitVarDecl(VarDeclNode node)
    {
        if (!_symbolTable.AddSymbol(
            node.Name,
            new SymbolInfo(SymbolType.Parameter, node)
        ))
        {
            AddError($"Duplicate variable name: {node.Name}", node.Line, node.Column);
        }

        VisitExpression(node.Initializer);
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
    }

    private void VisitAssignStmt(AssignStmtNode node)
    {
        if (node.Target is IdentifierExprNode identifierExprNode)
        {
            if (!_symbolTable.isSymbolDefined(identifierExprNode.Name))
            {
                AddError($"Undeclared variable: {identifierExprNode.Name}", identifierExprNode.Line, identifierExprNode.Column);
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

        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }
    }

    private void VisitReturnStmt(ReturnStmtNode node)
    {
        VisitExpression(node.Expression);
    }
}