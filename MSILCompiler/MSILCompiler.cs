using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MSILCompiler
{
    private readonly StringBuilder _code = new StringBuilder();
    private int _labelCounter = 0;
    private readonly Dictionary<string, Dictionary<string, int>> _methodLocals = new();
    private readonly Dictionary<string, System.Collections.Generic.List<string>> _classFields = new();
    private readonly Dictionary<string, string> _classHierarchy = new();
    private string _currentClass = "";
    private string _currentMethodKey = "";
    private string? _mainMethodKey = null;
    private readonly Dictionary<string, System.Collections.Generic.List<MethodDeclNode>> _classMethods = new();
    private bool _hasMainMethod = false;

    public string Compile(ProgramNode program)
    {
        _code.Clear();
        _methodLocals.Clear();
        _classFields.Clear();
        _classHierarchy.Clear();
        _classMethods.Clear();
        _labelCounter = 0;
        _currentClass = "";
        _currentMethodKey = "";
        _mainMethodKey = null;
        _hasMainMethod = false;
        
        // Check if we have a main method
        CheckForMainMethod(program);
        
        CollectClassInfo(program);
        CollectLocals(program);
        GenerateAssemblies();
        GenerateClasses(program);
        
        return _code.ToString();
    }

    private void CheckForMainMethod(ProgramNode program)
    {
        foreach (var classDecl in program.Classes)
        {
            foreach (var member in classDecl.Members)
            {
                if (member is MethodDeclNode method && method.Name == "main")
                {
                    _hasMainMethod = true;
                    _mainMethodKey = GetMethodKey(classDecl.Name, method.Name);
                    return;
                }
            }
        }
    }

    private void CollectClassInfo(ProgramNode program)
    {
        foreach (var classDecl in program.Classes)
        {
            var fields = new System.Collections.Generic.List<string>();
            var methods = new System.Collections.Generic.List<MethodDeclNode>();
            
            foreach (var member in classDecl.Members)
            {
                if (member is VarDeclNode field)
                {
                    fields.Add(field.Name);
                }
                else if (member is MethodDeclNode method)
                {
                    methods.Add(method);
                }
            }
            
            _classFields[classDecl.Name] = fields;
            _classMethods[classDecl.Name] = methods;
            
            if (classDecl.BaseClassName != null)
            {
                _classHierarchy[classDecl.Name] = classDecl.BaseClassName;
            }
        }
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
                    var methodKey = GetMethodKey(classDecl.Name, method.Name);
                    _methodLocals[methodKey] = methodVars;

                    int localIndex = 0;

                    foreach (var stmt in method.Body)
                    {
                        if (stmt is LocalVarDeclStmtNode localVar)
                        {
                            if (!methodVars.ContainsKey(localVar.Name))
                            {
                                methodVars[localVar.Name] = localIndex++;
                            }
                        }
                    }

                    if (method.Name == "main")
                    {
                        _mainMethodKey = methodKey;
                    }
                }
            }
        }
    }

    private void GenerateAssemblies()
    {
        _code.AppendLine(".assembly extern mscorlib {}");
        _code.AppendLine(".assembly CompilerOutput {}");
        _code.AppendLine();
    }

    private void GenerateClasses(ProgramNode program)
    {
        // Generate user classes
        foreach (var classDecl in program.Classes)
        {
            if (classDecl.Name == "Program")
            {
                // Handle Program class specially
                GenerateProgramClass(classDecl);
            }
            else
            {
                GenerateUserClass(classDecl);
            }
        }
        
        // If no Program class with main method, generate default one
        if (!_hasMainMethod)
        {
            GenerateDefaultProgramClass();
        }
    }

    private void GenerateUserClass(ClassDeclNode classDecl)
    {
        string baseClass = classDecl.BaseClassName != null ? 
            $" extends {classDecl.BaseClassName}" : 
            " extends [mscorlib]System.Object";
        
        _code.AppendLine($".class public auto ansi beforefieldinit {classDecl.Name}{baseClass}");
        _code.AppendLine("{{");
        
        GenerateClassFields(classDecl);
        GenerateClassConstructors(classDecl);
        
        var prevClass = _currentClass;
        _currentClass = classDecl.Name;
        
        if (_classMethods.ContainsKey(classDecl.Name))
        {
            foreach (var method in _classMethods[classDecl.Name])
            {
                if (method.Name == "main")
                {
                    GenerateStaticMethod(method); // main should be static
                }
                else
                {
                    GenerateInstanceMethod(method);
                }
            }
        }
        
        _currentClass = prevClass;
        
        _code.AppendLine("}}");
        _code.AppendLine();
    }

    private void GenerateProgramClass(ClassDeclNode classDecl)
    {
        _code.AppendLine($".class public auto ansi beforefieldinit Program extends [mscorlib]System.Object");
        _code.AppendLine("{{");
        
        GenerateClassFields(classDecl);
        GenerateClassConstructors(classDecl);
        
        var prevClass = _currentClass;
        _currentClass = "Program";
        
        // Find and generate main method
        var mainMethod = classDecl.Members.OfType<MethodDeclNode>().FirstOrDefault(m => m.Name == "main");
        if (mainMethod != null)
        {
            GenerateMainMethod(mainMethod);
        }
        
        // Generate other methods
        foreach (var method in _classMethods[classDecl.Name].Where(m => m.Name != "main"))
        {
            GenerateInstanceMethod(method);
        }
        
        _currentClass = prevClass;
        
        _code.AppendLine("}}");
        _code.AppendLine();
    }

    private void GenerateDefaultProgramClass()
    {
        _code.AppendLine(".class public auto ansi beforefieldinit Program extends [mscorlib]System.Object");
        _code.AppendLine("{{");
        _code.AppendLine("  .method public hidebysig static int32 Main() cil managed");
        _code.AppendLine("  {{");
        _code.AppendLine("    .entrypoint");
        _code.AppendLine("    .maxstack 8");
        _code.AppendLine("    ldc.i4.0");
        _code.AppendLine("    ret");
        _code.AppendLine("  }}");
        _code.AppendLine("}}");
    }

    private void GenerateClassFields(ClassDeclNode classDecl)
    {
        if (_classFields.ContainsKey(classDecl.Name))
        {
            foreach (var field in _classFields[classDecl.Name])
            {
                _code.AppendLine($"  .field public float64 {field}");
            }
            if (_classFields[classDecl.Name].Count > 0)
            {
                _code.AppendLine();
            }
        }
    }

    private void GenerateClassConstructors(ClassDeclNode classDecl)
    {
        _code.AppendLine($"  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed");
        _code.AppendLine("  {{");
        _code.AppendLine("    .maxstack 8");
        _code.AppendLine("    ldarg.0");
        
        if (classDecl.BaseClassName != null)
        {
            _code.AppendLine($"    call instance void {classDecl.BaseClassName}::.ctor()");
        }
        else
        {
            _code.AppendLine("    call instance void [mscorlib]System.Object::.ctor()");
        }
        
        if (_classFields.ContainsKey(classDecl.Name))
        {
            foreach (var field in _classFields[classDecl.Name])
            {
                _code.AppendLine($"    ldarg.0");
                _code.AppendLine("    ldc.r8 0.0");
                _code.AppendLine($"    stfld float64 {classDecl.Name}::{field}");
            }
        }
        
        _code.AppendLine("    ret");
        _code.AppendLine("  }}");
        _code.AppendLine();
    }

    private void GenerateMainMethod(MethodDeclNode method)
    {
        _currentMethodKey = GetMethodKey(_currentClass, method.Name);
        
        _code.AppendLine($"  .method public hidebysig static void Main() cil managed");
        _code.AppendLine("  {{");
        _code.AppendLine("    .entrypoint");
        _code.AppendLine("    .maxstack 16");
        
        GenerateMethodLocals(method);
        GenerateMethodBody(method);
        
        _code.AppendLine("    ret");
        _code.AppendLine("  }}");
        _code.AppendLine();
    }

    private void GenerateStaticMethod(MethodDeclNode method)
    {
        _currentMethodKey = GetMethodKey(_currentClass, method.Name);
        
        string returnType = "void";
        if (method.ReturnType != null)
        {
            returnType = GetTypeName(method.ReturnType.Name);
        }
        
        string paramList = GenerateParameterList(method.Parameters);
        
        _code.AppendLine($"  .method public hidebysig static {returnType} {method.Name}({paramList}) cil managed");
        _code.AppendLine("  {{");
        _code.AppendLine("    .maxstack 16");
        
        GenerateMethodLocals(method);
        GenerateMethodBody(method);
        
        if (returnType == "void")
        {
            _code.AppendLine("    ret");
        }
        
        _code.AppendLine("  }}");
        _code.AppendLine();
    }

    private void GenerateInstanceMethod(MethodDeclNode method)
    {
        _currentMethodKey = GetMethodKey(_currentClass, method.Name);
        
        string returnType = "void";
        if (method.ReturnType != null)
        {
            returnType = GetTypeName(method.ReturnType.Name);
        }
        
        string paramList = GenerateParameterList(method.Parameters);
        
        _code.AppendLine($"  .method public hidebysig instance {returnType} {method.Name}({paramList}) cil managed");
        _code.AppendLine("  {{");
        _code.AppendLine("    .maxstack 16");
        
        GenerateMethodLocals(method);
        GenerateMethodBody(method);
        
        if (returnType == "void")
        {
            _code.AppendLine("    ret");
        }
        
        _code.AppendLine("  }}");
        _code.AppendLine();
    }

    private string GetTypeName(string typeName)
    {
        return typeName switch
        {
            "Real" => "float64",
            "Integer" => "int32",
            "Boolean" => "bool",
            _ => "object"
        };
    }

    private string GenerateParameterList(System.Collections.Generic.List<ParamNode> parameters)
    {
        var paramStrings = new System.Collections.Generic.List<string>();
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            string typeName = GetTypeName(param.Type.Name);
            paramStrings.Add($"{typeName} '{param.Name}'");
        }
        return string.Join(", ", paramStrings);
    }

    private void GenerateMethodLocals(MethodDeclNode method)
    {
        if (_methodLocals.ContainsKey(_currentMethodKey))
        {
            var locals = _methodLocals[_currentMethodKey];
            if (locals.Count > 0)
            {
                _code.Append("    .locals init (");
                bool first = true;
                
                foreach (var local in locals.OrderBy(kv => kv.Value))
                {
                    if (!first) _code.Append(", ");
                    _code.Append($"float64 '{local.Key}'");
                    first = false;
                }
                _code.AppendLine(")");
            }
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
        _code.AppendLine($"    stloc '{node.Name}'");
    }

    private void VisitAssignStmt(AssignStmtNode node)
    {
        if (node.Target is IdentifierExprNode id)
        {
            if (IsField(id.Name))
            {
                _code.AppendLine("    ldarg.0");
                VisitExpression(node.Value);
                _code.AppendLine($"    stfld float64 {_currentClass}::{id.Name}");
            }
            else
            {
                VisitExpression(node.Value);
                _code.AppendLine($"    stloc '{id.Name}'");
            }
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
                if (IsParameter(id.Name, _currentMethodKey))
                {
                    int paramIndex = GetParameterIndex(id.Name, _currentMethodKey);
                    _code.AppendLine($"    ldarg.{paramIndex}");
                }
                else if (IsField(id.Name))
                {
                    _code.AppendLine("    ldarg.0");
                    _code.AppendLine($"    ldfld float64 {_currentClass}::{id.Name}");
                }
                else
                {
                    _code.AppendLine($"    ldloc '{id.Name}'");
                }
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
                _code.AppendLine("    conv.r8");
                break;

            case RealLiteralExprNode realLit:
                if (!double.TryParse(realLit.Value, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out var d))
                {
                    throw new Exception($"Invalid real literal '{realLit.Value}' at {realLit.Line}:{realLit.Column}");
                }
                _code.AppendLine($"    ldc.r8 {d.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                break;

            case BoolLiteralExprNode boolLit:
                _code.AppendLine(boolLit.Value ? "    ldc.i4.1" : "    ldc.i4.0");
                _code.AppendLine("    conv.r8");
                break;
                
            case BinaryExprNode bin:
                VisitBinaryExpression(bin);
                break;
                
            case CallExprNode call:
                VisitCallExpression(call);
                break;
                
            case MemberAccessExprNode member:
                VisitMemberAccess(member);
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
                _code.AppendLine("    conv.r8");
                break;
            case BinaryOperator.GreaterThan:
                _code.AppendLine("    cgt");
                _code.AppendLine("    conv.r8");
                break;
            case BinaryOperator.LessThan:
                _code.AppendLine("    clt");
                _code.AppendLine("    conv.r8");
                break;
            default:
                _code.AppendLine("    add");
                break;
        }
    }

    private void VisitCallExpression(CallExprNode node)
    {
        if (node.Callee is IdentifierExprNode id)
        {
            switch (id.Name)
            {
                case "write":
                    if (node.Arguments.Count > 0)
                    {
                        VisitExpression(node.Arguments[0]);
                        _code.AppendLine("    call void [mscorlib]System.Console::WriteLine(float64)");
                    }
                    return;

                case "Integer":
                case "Boolean":
                case "Real":
                    if (node.Arguments.Count != 1)
                    {
                        throw new Exception($"Constructor '{id.Name}' expects exactly one argument.");
                    }
                    VisitExpression(node.Arguments[0]);
                    return;
            }
        }

        if (node.Callee is MemberAccessExprNode member)
        {
            string name = member.MemberName;

            if (name == "Not")
            {
                VisitExpression(member.Target);
                _code.AppendLine("    ldc.i4.0");
                _code.AppendLine("    ceq");
                _code.AppendLine("    conv.r8");
                return;
            }

            if (node.Arguments.Count != 1)
            {
                throw new Exception($"Method '{name}' expects one argument.");
            }

            ExprNode arg = node.Arguments[0];

            switch (name)
            {
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
                case "Equal":
                    EmitBinaryLikeCall(member.Target, arg, "ceq");
                    _code.AppendLine("    conv.r8");
                    return;
                case "Greater":
                    EmitBinaryLikeCall(member.Target, arg, "cgt");
                    _code.AppendLine("    conv.r8");
                    return;
                case "Less":
                    EmitBinaryLikeCall(member.Target, arg, "clt");
                    _code.AppendLine("    conv.r8");
                    return;
                case "GreaterEqual":
                    EmitBinaryLikeCall(member.Target, arg, "clt");
                    _code.AppendLine("    ldc.i4.0");
                    _code.AppendLine("    ceq");
                    _code.AppendLine("    conv.r8");
                    return;
                case "LessEqual":
                    EmitBinaryLikeCall(member.Target, arg, "cgt");
                    _code.AppendLine("    ldc.i4.0");
                    _code.AppendLine("    ceq");
                    _code.AppendLine("    conv.r8");
                    return;
                case "And":
                    EmitBinaryLikeCall(member.Target, arg, "and");
                    _code.AppendLine("    conv.r8");
                    return;
                case "Or":
                    EmitBinaryLikeCall(member.Target, arg, "or");
                    _code.AppendLine("    conv.r8");
                    return;
                case "Xor":
                    EmitBinaryLikeCall(member.Target, arg, "xor");
                    _code.AppendLine("    conv.r8");
                    return;
            }
        }

        throw new Exception($"Unsupported call expression at {node.Line}:{node.Column}");
    }

    private void VisitMemberAccess(MemberAccessExprNode node)
    {
        VisitExpression(node.Target);
    }

    private void EmitBinaryLikeCall(ExprNode target, ExprNode argument, string ilOp)
    {
        VisitExpression(target);
        VisitExpression(argument);
        _code.AppendLine($"    {ilOp}");
    }

    private bool IsField(string name)
    {
        return _classFields.ContainsKey(_currentClass) && 
               _classFields[_currentClass].Contains(name);
    }

    private string GenerateLabel()
    {
        return $"IL_{_labelCounter++:0000}";
    }

    private static string GetMethodKey(string className, string methodName)
    {
        return $"{className}.{methodName}";
    }
    
    private void GenerateMethodBody(MethodDeclNode method)
    {
        foreach (var stmt in method.Body)
        {
            VisitStatement(stmt);
        }
    }
}