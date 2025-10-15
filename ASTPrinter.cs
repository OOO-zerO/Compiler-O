using System;
using System.Collections.Generic;

public static class ASTPrinter
{
    public static void Print(ProgramNode program)
    {
        foreach (var cls in program.Classes)
        {
            PrintClass(cls, 0);
        }
    }

    private static void PrintClass(ClassDeclNode cls, int indent)
    {
        WriteLine(indent, $"Class {cls.Name}" + (cls.BaseClassName != null ? $" extends {cls.BaseClassName}" : ""));
        if (cls.ThisStatements.Count > 0)
        {
            WriteLine(indent + 1, "this is");
            foreach (var s in cls.ThisStatements)
                PrintStatement(s, indent + 2);
            WriteLine(indent + 1, "end");
        }
        foreach (var m in cls.Members)
        {
            switch (m)
            {
                case VarDeclNode v:
                    WriteLine(indent + 1, $"var {v.Name} :");
                    PrintExpr(v.Initializer, indent + 2);
                    break;
                case MethodDeclNode md:
                    var ret = md.ReturnType != null ? $": {md.ReturnType.Name}" : string.Empty;
                    WriteLine(indent + 1, $"method {md.Name}({string.Join(", ", md.Parameters.ConvertAll(p => p.Name + ": " + p.Type.Name))}){ret} is");
                    foreach (var s in md.Body)
                        PrintStatement(s, indent + 2);
                    WriteLine(indent + 1, "end");
                    break;
            }
        }
    }

    private static void PrintStatement(StatementNode s, int indent)
    {
        switch (s)
        {
            case LocalVarDeclStmtNode lv:
                WriteLine(indent, $"var {lv.Name} :");
                PrintExpr(lv.Initializer, indent + 1);
                break;
            case AssignStmtNode a:
                Write(indent, "assign ");
                PrintExpr(a.Target, 0);
                Console.Write(" := ");
                PrintExpr(a.Value, 0);
                Console.WriteLine();
                break;
            case IfStmtNode i:
                WriteLine(indent, "if");
                PrintExpr(i.Condition, indent + 1);
                WriteLine(indent, "then");
                foreach (var ts in i.ThenBranch)
                    PrintStatement(ts, indent + 1);
                if (i.ElseBranch != null)
                {
                    WriteLine(indent, "else");
                    foreach (var es in i.ElseBranch)
                        PrintStatement(es, indent + 1);
                }
                WriteLine(indent, "end");
                break;
            case WhileStmtNode w:
                WriteLine(indent, "while");
                PrintExpr(w.Condition, indent + 1);
                WriteLine(indent, "loop");
                foreach (var bs in w.Body)
                    PrintStatement(bs, indent + 1);
                WriteLine(indent, "end");
                break;
            case ReturnStmtNode r:
                Write(indent, "return ");
                PrintExpr(r.Expression, 0);
                Console.WriteLine();
                break;
            case ExprStmtNode es:
                Write(indent, "expr ");
                PrintExpr(es.Expression, 0);
                Console.WriteLine();
                break;
        }
    }

    private static void PrintExpr(ExprNode e, int indent)
    {
        switch (e)
        {
            case IdentifierExprNode id:
                WriteLine(indent, $"id({id.Name})");
                break;
            case ThisExprNode:
                WriteLine(indent, "this");
                break;
            case IntLiteralExprNode il:
                WriteLine(indent, $"int({il.Value})");
                break;
            case RealLiteralExprNode rl:
                WriteLine(indent, $"real({rl.Value})");
                break;
            case BoolLiteralExprNode bl:
                WriteLine(indent, $"bool({bl.Value.ToString().ToLower()})");
                break;
            case MemberAccessExprNode ma:
                WriteLine(indent, "member");
                PrintExpr(ma.Target, indent + 1);
                WriteLine(indent + 1, $".{ma.MemberName}");
                break;
            case CallExprNode call:
                WriteLine(indent, "call");
                PrintExpr(call.Callee, indent + 1);
                if (call.Arguments.Count > 0)
                {
                    WriteLine(indent + 1, "args");
                    foreach (var a in call.Arguments)
                        PrintExpr(a, indent + 2);
                }
                break;
        }
    }

    private static void WriteLine(int indent, string text)
    {
        Write(indent, text);
        Console.WriteLine();
    }
    private static void Write(int indent, string text)
    {
        for (int i = 0; i < indent; i++) Console.Write("  ");
        Console.Write(text);
    }
}


