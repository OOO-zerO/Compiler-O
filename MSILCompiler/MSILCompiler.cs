using System;
using System.Collections.Generic;
using System.Text;

public class MSILCompiler
{
    private readonly StringBuilder _code = new StringBuilder();
    private int _labelCounter = 0;
    private readonly Dictionary<string, Dictionary<string, int>> _methodLocals = new();
    private string _currentMethod = "main";

    public string Compile(ProgramNode program)
    {
        _code.Clear();
        _methodLocals.Clear();
        _labelCounter = 0;
        _currentMethod = "main";
        
        // First - collect all local variables from all methods
        CollectLocals(program);

        // Second - code generation
        GenerateHeader();
        VisitProgram(program);
        GenerateFooter();
        
        return _code.ToString();
    }

    private void CollectLocals(ProgramNode program)
    {
        foreach (var classDecl in program.Classes)
        {
            foreach (var member in classDecl.Members)
            {
                if (member is MethodDeclNode method)
                {
                    var methodVars = new Dictionary<string, int>();
                    _methodLocals[method.Name] = methodVars;

                    int localIndex = 0;

                    // Treat parameters as locals at the beginning of the method
                    foreach (var param in method.Parameters)
                    {
                        methodVars[param.Name] = localIndex++;
                    }

                    // Then allocate indices for explicit local variable declarations
                    foreach (var stmt in method.Body)
                    {
                        if (stmt is LocalVarDeclStmtNode localVar)
                        {
                            methodVars[localVar.Name] = localIndex++;
                        }
                    }
                }
            }
        }
    }

    private void GenerateHeader()
    {
        // MSIL assembly directives
        _code.AppendLine(".assembly extern mscorlib {}");  // Reference to standard library
        _code.AppendLine(".assembly CompilerOutput {}");   // Our output assembly
        _code.AppendLine(".class public auto ansi beforefieldinit Program extends [mscorlib]System.Object");
        _code.AppendLine("{");
        // Use int32 return for Main so we can return computed values if needed
        _code.AppendLine("  .method public hidebysig static int32 Main() cil managed");
        _code.AppendLine("  {");
        _code.AppendLine("    .entrypoint");  // Mark as application entry point
        _code.AppendLine("    .maxstack 16"); // Maximum stack size
        
        // Prefer explicit "main" method locals if present
        if (_methodLocals.ContainsKey("main"))
        {
            var mainLocals = _methodLocals["main"];
            if (mainLocals.Count > 0)
            {
                _code.Append("    .locals init (");
                bool first = true;
                foreach (var local in mainLocals)
                {
                    if (!first) _code.Append(", ");
                    _code.Append($"int32 V_{local.Value}");
                    first = false;
                }
                _code.AppendLine(")");
            }
        }
        else if (_methodLocals.Count > 0)
        {
            // Fallback: no explicit main, but we still emit locals for the entrypoint
            // Use the maximum number of locals used by any method, and declare V_0..V_n
            int maxLocals = 0;
            foreach (var kv in _methodLocals)
            {
                if (kv.Value.Count > maxLocals)
                {
                    maxLocals = kv.Value.Count;
                }
            }

            if (maxLocals > 0)
            {
                _code.Append("    .locals init (");
                for (int i = 0; i < maxLocals; i++)
                {
                    if (i > 0) _code.Append(", ");
                    _code.Append($"int32 V_{i}");
                }
                _code.AppendLine(")");
            }
        }
    }

    private void GenerateFooter()
    {
        // Default fallback return value for Main (int32)
        _code.AppendLine("    ldc.i4.0");
        _code.AppendLine("    ret");  // Return from Main method
        _code.AppendLine("  }");
        _code.AppendLine("}");
    }

    // Visit all classes in the program
    private void VisitProgram(ProgramNode node)
    {
        foreach (var classDecl in node.Classes)
        {
            VisitClass(classDecl);
        }
    }

    // Visit all methods in the class
    private void VisitClass(ClassDeclNode node)
    {
        foreach (var member in node.Members)
        {
            if (member is MethodDeclNode method)
            {
                _currentMethod = method.Name;
                VisitMethod(method);
            }
        }
    }

    // Visit all statements in the method
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

    // Evaluate initializer expression and store in local variable
    private void VisitLocalVarDecl(LocalVarDeclStmtNode node)
    {
        VisitExpression(node.Initializer);
        _code.AppendLine($"    stloc {GetLocalVarIndex(node.Name)}");
    }

    // Handle assignment to identifier (local variable)
    private void VisitAssignStmt(AssignStmtNode node)
    {
        if (node.Target is IdentifierExprNode id)
        {
            VisitExpression(node.Value);
            _code.AppendLine($"    stloc {GetLocalVarIndex(id.Name)}");
        }
    }

    private void VisitIfStmt(IfStmtNode node)
    {
        string elseLabel = GenerateLabel();
        string endLabel = GenerateLabel();
        
        // condition and branch to else if false
        VisitExpression(node.Condition);
        _code.AppendLine($"    brfalse {elseLabel}");
        
        // branch
        foreach (var stmt in node.ThenBranch)
        {
            VisitStatement(stmt);
        }
        _code.AppendLine($"    br {endLabel}");
        
        // Else branch
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
        
        // Loop start
        _code.AppendLine($"{startLabel}:");
        // Check condition and exit loop if false
        VisitExpression(node.Condition);
        _code.AppendLine($"    brfalse {endLabel}");
        
         // Loop body
        foreach (var stmt in node.Body)
        {
            VisitStatement(stmt);
        }
        
        // Back to condition check
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
                _code.AppendLine($"    ldloc {GetLocalVarIndex(id.Name)}");
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
        // Built-in free function: write(x)
        if (node.Callee is IdentifierExprNode id && id.Name == "write")
        {
            if (node.Arguments.Count > 0)
            {
                VisitExpression(node.Arguments[0]);
                _code.AppendLine("    call void [mscorlib]System.Console::WriteLine(int32)");
            }
            return;
        }

        // Object-style calls like a.Plus(b), result.Greater(0), cond.And(other)
        if (node.Callee is MemberAccessExprNode member)
        {
            string name = member.MemberName;

            // Unary boolean NOT: cond.Not()
            if (name == "Not")
            {
                VisitExpression(member.Target);
                _code.AppendLine("    ldc.i4.0");
                _code.AppendLine("    ceq");
                return;
            }

            // All remaining built-ins expect exactly one argument
            if (node.Arguments.Count != 1)
            {
                throw new Exception($"Method '{name}' expects one argument in codegen.");
            }

            ExprNode arg = node.Arguments[0];

            switch (name)
            {
                // Integer arithmetic: a.Plus(b), a.Minus(b), a.Mult(b), a.Div(b), a.Rem(b)
                case "Plus":
                    EmitBinaryLikeCall(member.Target, arg, "add");
                    return;
                case "Minus":
                    EmitBinaryLikeCall(member.Target, arg, "sub");
                    return;
                case "Mult":
                    EmitBinaryLikeCall(member.Target, arg, "mul");
                    return;
                case "Div":
                    EmitBinaryLikeCall(member.Target, arg, "div");
                    return;
                case "Rem":
                    EmitBinaryLikeCall(member.Target, arg, "rem");
                    return;

                // Comparisons on Integer / Real: return Boolean (int32 0/1)
                case "Equal":
                    EmitBinaryLikeCall(member.Target, arg, "ceq");
                    return;
                case "Greater":
                    EmitBinaryLikeCall(member.Target, arg, "cgt");
                    return;
                case "Less":
                    EmitBinaryLikeCall(member.Target, arg, "clt");
                    return;
                case "GreaterEqual":
                    // !(a < b)  =>  a < b ; ldc.i4.0 ; ceq
                    EmitBinaryLikeCall(member.Target, arg, "clt");
                    _code.AppendLine("    ldc.i4.0");
                    _code.AppendLine("    ceq");
                    return;
                case "LessEqual":
                    // !(a > b)  =>  a > b ; ldc.i4.0 ; ceq
                    EmitBinaryLikeCall(member.Target, arg, "cgt");
                    _code.AppendLine("    ldc.i4.0");
                    _code.AppendLine("    ceq");
                    return;

                // Boolean operations: cond.And(p), cond.Or(p), cond.Xor(p)
                case "And":
                    EmitBinaryLikeCall(member.Target, arg, "and");
                    return;
                case "Or":
                    EmitBinaryLikeCall(member.Target, arg, "or");
                    return;
                case "Xor":
                    EmitBinaryLikeCall(member.Target, arg, "xor");
                    return;
            }
        }

        // If we got here, this is some method we don't know how to lower to IL yet
        throw new Exception($"Unsupported call expression in codegen at {node.Line}:{node.Column}");
    }

    // Helper: evaluate target and argument and emit simple binary IL op
    private void EmitBinaryLikeCall(ExprNode target, ExprNode argument, string ilOp)
    {
        VisitExpression(target);
        VisitExpression(argument);
        _code.AppendLine($"    {ilOp}");
    }

    private int GetLocalVarIndex(string varName)
    {
        if (_methodLocals.ContainsKey(_currentMethod) && 
            _methodLocals[_currentMethod].ContainsKey(varName))
        {
            return _methodLocals[_currentMethod][varName];
        }
        
        throw new Exception($"Local variable '{varName}' not found in method '{_currentMethod}'");
    }

    private string GenerateLabel()
    {
        return $"IL_{_labelCounter++:0000}";
    }
}