using System;
using System.Collections.Generic;
using System.Text;

public class MSILCompiler
{
    private readonly StringBuilder _code = new StringBuilder();
    private int _labelCounter = 0;
    private readonly Dictionary<string, int> _localVariables = new Dictionary<string, int>();
    private int _localVarIndex = 0;
    private readonly System.Collections.Generic.List<string> _localsList = new System.Collections.Generic.List<string>();

    public string Compile(ProgramNode program)
    {
        _code.Clear();
        _localVariables.Clear();
        _localVarIndex = 0;
        _localsList.Clear();
        
        // First - collect all local variables
        CollectLocals(program);
        
        // Second - code generation
        GenerateHeader();
        VisitProgram(program);
        GenerateFooter();
        
        return _code.ToString();
    }

    private void CollectLocals(ProgramNode program)
    {
        // Collect all local variables
        foreach (var classDecl in program.Classes)
        {
            foreach (var member in classDecl.Members)
            {
                if (member is MethodDeclNode method && method.Name == "main")
                {
                    CollectMethodLocals(method);
                }
            }
        }
    }

    private void CollectMethodLocals(MethodDeclNode method)
    {
        foreach (var stmt in method.Body)
        {
            if (stmt is LocalVarDeclStmtNode localVar)
            {
                if (!_localVariables.ContainsKey(localVar.Name))
                {
                    _localVariables[localVar.Name] = _localVarIndex++;
                    _localsList.Add(localVar.Name);
                }
            }
        }
    }

    private void GenerateHeader()
    {
        _code.AppendLine(".assembly extern mscorlib {}");
        _code.AppendLine(".assembly CompilerOutput {}");
        _code.AppendLine(".class public auto ansi beforefieldinit Program extends [mscorlib]System.Object");
        _code.AppendLine("{");
        _code.AppendLine("  .method public hidebysig static void Main() cil managed");
        _code.AppendLine("  {");
        _code.AppendLine("    .entrypoint");
        _code.AppendLine("    .maxstack 16");
        
        // Generate proper locals declaration
        if (_localsList.Count > 0)
        {
            _code.Append("    .locals init (");
            for (int i = 0; i < _localsList.Count; i++)
            {
                _code.Append($"int32 V_{i}");
                if (i < _localsList.Count - 1) _code.Append(", ");
            }
            _code.AppendLine(")");
        }
    }

    private void GenerateFooter()
    {
        _code.AppendLine("    ret");
        _code.AppendLine("  }");
        _code.AppendLine("}");
    }

    private void VisitProgram(ProgramNode node)
    {
        foreach (var classDecl in node.Classes)
        {
            VisitClass(classDecl);
        }
    }

    private void VisitClass(ClassDeclNode node)
    {
        foreach (var member in node.Members)
        {
            if (member is MethodDeclNode method && method.Name == "main")
            {
                VisitMethod(method);
            }
        }
    }

    private void VisitMethod(MethodDeclNode node)
    {
        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }
    }

    private void VisitStatement(StatementNode node)
    {
        switch (node)
        {
            case LocalVarDeclStmtNode localVar:
                VisitLocalVarDecl(localVar);
                break;
            case AssignStmtNode assign:
                VisitAssignStmt(assign);
                break;
            case IfStmtNode ifStmt:
                VisitIfStmt(ifStmt);
                break;
            case WhileStmtNode whileStmt:
                VisitWhileStmt(whileStmt);
                break;
            case ReturnStmtNode returnStmt:
                VisitReturnStmt(returnStmt);
                break;
            case ExprStmtNode exprStmt:
                VisitExpression(exprStmt.Expression);
                if (!(exprStmt.Expression is CallExprNode))
                {
                    _code.AppendLine("    pop");
                }
                break;
        }
    }

    private void VisitLocalVarDecl(LocalVarDeclStmtNode node)
    {
        VisitExpression(node.Initializer);
        _code.AppendLine($"    stloc.{GetLocalVarIndex(node.Name)}");
    }

    private void VisitAssignStmt(AssignStmtNode node)
    {
        if (node.Target is IdentifierExprNode id)
        {
            VisitExpression(node.Value);
            _code.AppendLine($"    stloc.{GetLocalVarIndex(id.Name)}");
        }
    }

    private void VisitIfStmt(IfStmtNode node)
    {
        string elseLabel = GenerateLabel();
        string endLabel = GenerateLabel();
        
        VisitExpression(node.Condition);
        _code.AppendLine($"    brfalse {elseLabel}");
        
        foreach (var stmt in node.ThenBranch)
        {
            VisitStatement(stmt);
        }
        _code.AppendLine($"    br {endLabel}");
        
        _code.AppendLine($"{elseLabel}:");
        if (node.ElseBranch != null)
        {
            foreach (var stmt in node.ElseBranch)
            {
                VisitStatement(stmt);
            }
        }
        
        _code.AppendLine($"{endLabel}:");
    }

    private void VisitWhileStmt(WhileStmtNode node)
    {
        string startLabel = GenerateLabel();
        string endLabel = GenerateLabel();
        
        _code.AppendLine($"{startLabel}:");
        VisitExpression(node.Condition);
        _code.AppendLine($"    brfalse {endLabel}");
        
        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }
        
        _code.AppendLine($"    br {startLabel}");
        _code.AppendLine($"{endLabel}:");
    }

    private void VisitReturnStmt(ReturnStmtNode node)
    {
        VisitExpression(node.Expression);
        _code.AppendLine("    ret");
    }

    private void VisitExpression(ExprNode node)
    {
        switch (node)
        {
            case IdentifierExprNode id:
                _code.AppendLine($"    ldloc.{GetLocalVarIndex(id.Name)}");
                break;
                
            case IntLiteralExprNode intLit:
                int value = int.Parse(intLit.Value);
                if (value >= 0 && value <= 8)
                {
                    _code.AppendLine($"    ldc.i4.{value}");
                }
                else
                {
                    _code.AppendLine($"    ldc.i4 {value}");
                }
                break;
                
            case BoolLiteralExprNode boolLit:
                _code.AppendLine(boolLit.Value ? "    ldc.i4.1" : "    ldc.i4.0");
                break;
                
            case BinaryExprNode bin:
                VisitBinaryExpression(bin);
                break;
                
            case CallExprNode call:
                VisitCallExpression(call);
                break;
        }
    }

    private void VisitBinaryExpression(BinaryExprNode node)
    {
        VisitExpression(node.Left);
        VisitExpression(node.Right);
        
        switch (node.Operator)
        {
            case BinaryOperator.Add:
                _code.AppendLine("    add");
                break;
            case BinaryOperator.Subtract:
                _code.AppendLine("    sub");
                break;
            case BinaryOperator.Multiply:
                _code.AppendLine("    mul");
                break;
            case BinaryOperator.Divide:
                _code.AppendLine("    div");
                break;
            case BinaryOperator.Equal:
                _code.AppendLine("    ceq");
                break;
            case BinaryOperator.GreaterThan:
                _code.AppendLine("    cgt");
                break;
            case BinaryOperator.LessThan:
                _code.AppendLine("    clt");
                break;
            default:
                _code.AppendLine("    add");
                break;
        }
    }

    private void VisitCallExpression(CallExprNode node)
    {
        if (node.Callee is IdentifierExprNode id && id.Name == "write")
        {
            // For write calls, we expect one argument
            if (node.Arguments.Count > 0)
            {
                VisitExpression(node.Arguments[0]);
                _code.AppendLine("    call void [mscorlib]System.Console::WriteLine(int32)");
            }
        }
    }

    private int GetLocalVarIndex(string varName)
    {
        return _localVariables[varName];
    }

    private string GenerateLabel()
    {
        return $"IL_{_labelCounter++:0000}";
    }
}