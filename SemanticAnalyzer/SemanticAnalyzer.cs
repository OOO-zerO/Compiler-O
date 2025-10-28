using System;
using System.Collections.Generic;

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

    }

    private void VisitVarDecl(VarDeclNode node)
    {

    }

    private void VisitStatement(StatementNode node)
    {

    }

    private void VisitLocalVarDecl(LocalVarDeclStmtNode node)
    {

    }

    private void VisitAssignStmt(AssignStmtNode node)
    {

    }

    private void VisitExpression(ExprNode node)
    {

    }

    private void VisitIfStmt(IfStmtNode node)
    {

    }

    private void VisitWhileStmt(WhileStmtNode node)
    {

    }

    private void VisitReturnStmt(ReturnStmtNode node)
    {
        
    }
}