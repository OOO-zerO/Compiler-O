using System;
using System.Collections.Generic;

public static class AstOptimizer
{
    public static void Optimize(ProgramNode program)
    {
        foreach (var cls in program.Classes)
        {
            OptimizeClass(cls);
        }
    }

    private static void OptimizeClass(ClassDeclNode cls)
    {
        OptimizeStatementList(cls.ThisStatements);

        foreach (var member in cls.Members)
        {
            if (member is MethodDeclNode method)
            {
                OptimizeStatementList(method.Body);
            }
        }
    }

    // Optimizes a list of statements in-place
    private static void OptimizeStatementList(System.Collections.Generic.List<StatementNode> statements)
    {
        var optimized = new System.Collections.Generic.List<StatementNode>();
        bool reachable = true;

        foreach (var stmt in statements)
        {
            if (!reachable)
            {
                // everything after an unconditional return is unreachable
                break;
            }

            var replacements = OptimizeStatement(stmt, ref reachable);
            optimized.AddRange(replacements);

            if (!reachable)
            {
                break;
            }
        }

        statements.Clear();
        statements.AddRange(optimized);
    }

    // Returns zero, one, or many statements that replace the original statement
    private static System.Collections.Generic.List<StatementNode> OptimizeStatement(StatementNode stmt, ref bool reachable)
    {
        var result = new System.Collections.Generic.List<StatementNode>();

        switch (stmt)
        {
            case LocalVarDeclStmtNode localVar:
            {
                var init = OptimizeExpr(localVar.Initializer);
                result.Add(new LocalVarDeclStmtNode(localVar.Name, init, localVar.Line, localVar.Column));
                break;
            }
            case AssignStmtNode assign:
            {
                var target = OptimizeExpr(assign.Target);
                var value = OptimizeExpr(assign.Value);
                result.Add(new AssignStmtNode(target, value, assign.Line, assign.Column));
                break;
            }
            case ExprStmtNode exprStmt:
            {
                var expr = OptimizeExpr(exprStmt.Expression);
                result.Add(new ExprStmtNode(expr, exprStmt.Line, exprStmt.Column));
                break;
            }
            case ReturnStmtNode ret:
            {
                var expr = OptimizeExpr(ret.Expression);
                result.Add(new ReturnStmtNode(expr, ret.Line, ret.Column));
                reachable = false; // code after this is unreachable
                break;
            }
            case WhileStmtNode whileStmt:
            {
                var cond = OptimizeExpr(whileStmt.Condition);

                // Optimize body first, even if loop might be removed
                var bodyCopy = new System.Collections.Generic.List<StatementNode>(whileStmt.Body);
                OptimizeStatementList(bodyCopy);

                if (cond is BoolLiteralExprNode boolCond)
                {
                    if (!boolCond.Value)
                    {
                        // while(false) - remove entire loop
                        return result;
                    }
                    // while(true) without further analysis is kept as-is,
                }

                result.Add(new WhileStmtNode(cond, bodyCopy, whileStmt.Line, whileStmt.Column));
                break;
            }
            case IfStmtNode ifStmt:
            {
                var cond = OptimizeExpr(ifStmt.Condition);

                var thenCopy = new System.Collections.Generic.List<StatementNode>(ifStmt.ThenBranch);
                OptimizeStatementList(thenCopy);

                System.Collections.Generic.List<StatementNode>? elseCopy = null;
                if (ifStmt.ElseBranch != null)
                {
                    elseCopy = new System.Collections.Generic.List<StatementNode>(ifStmt.ElseBranch);
                    OptimizeStatementList(elseCopy);
                }

                if (cond is BoolLiteralExprNode boolCond)
                {
                    // if (true) then X else Y => X
                    // if (false) then X else Y => Y (or nothing if Y is null)
                    if (boolCond.Value)
                    {
                        result.AddRange(thenCopy);
                        return result;
                    }
                    else
                    {
                        if (elseCopy != null)
                        {
                            result.AddRange(elseCopy);
                        }
                        // if (false) with no else -> removed entirely
                        return result;
                    }
                }

                // Non-constant condition: keep the if, but use optimized pieces
                result.Add(new IfStmtNode(cond, thenCopy, elseCopy, ifStmt.Line, ifStmt.Column));
                break;
            }
            default:
                // For any unhandled statement kind, just pass it through
                result.Add(stmt);
                break;
        }

        return result;
    }

    // Constant folds expressions and optimizes nested expressions
    private static ExprNode OptimizeExpr(ExprNode expr)
    {
        switch (expr)
        {
            case IntLiteralExprNode:
            case RealLiteralExprNode:
            case BoolLiteralExprNode:
            case ThisExprNode:
            case IdentifierExprNode:
                return expr;

            case MemberAccessExprNode member:
                return new MemberAccessExprNode(
                    OptimizeExpr(member.Target),
                    member.MemberName,
                    member.Line,
                    member.Column);

            case CallExprNode call:
            {
                var optCallee = OptimizeExpr(call.Callee);
                var optArgs = new System.Collections.Generic.List<ExprNode>();
                foreach (var a in call.Arguments)
                {
                    optArgs.Add(OptimizeExpr(a));
                }
                return new CallExprNode(optCallee, optArgs, call.Line, call.Column);
            }

            case BinaryExprNode bin:
            {
                var left = OptimizeExpr(bin.Left);
                var right = OptimizeExpr(bin.Right);

                // Try constant folding if both sides are literals
                var folded = TryFoldBinary(bin, left, right);
                return folded ?? new BinaryExprNode(left, bin.Operator, right, bin.Line, bin.Column);
            }

            default:
                return expr;
        }
    }

    private static ExprNode? TryFoldBinary(BinaryExprNode bin, ExprNode left, ExprNode right)
    {
        // Integer arithmetic and comparisons
        if (left is IntLiteralExprNode li && right is IntLiteralExprNode ri)
        {
            if (!int.TryParse(li.Value, out var lv) || !int.TryParse(ri.Value, out var rv))
            {
                return null;
            }

            switch (bin.Operator)
            {
                case BinaryOperator.Add:
                    return new IntLiteralExprNode((lv + rv).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Subtract:
                    return new IntLiteralExprNode((lv - rv).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Multiply:
                    return new IntLiteralExprNode((lv * rv).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Divide:
                    if (rv == 0) return null; // avoid div by zero at compile time
                    return new IntLiteralExprNode((lv / rv).ToString(), bin.Line, bin.Column);

                case BinaryOperator.GreaterThan:
                    return new BoolLiteralExprNode(lv > rv, bin.Line, bin.Column);
                case BinaryOperator.LessThan:
                    return new BoolLiteralExprNode(lv < rv, bin.Line, bin.Column);
                case BinaryOperator.GreaterThanOrEqual:
                    return new BoolLiteralExprNode(lv >= rv, bin.Line, bin.Column);
                case BinaryOperator.LessThanOrEqual:
                    return new BoolLiteralExprNode(lv <= rv, bin.Line, bin.Column);
                case BinaryOperator.Equal:
                    return new BoolLiteralExprNode(lv == rv, bin.Line, bin.Column);
                case BinaryOperator.NotEqual:
                    return new BoolLiteralExprNode(lv != rv, bin.Line, bin.Column);
            }
        }

        // Real arithmetic and comparisons (or mixed int/real)
        if (TryGetDouble(left, out var ld) && TryGetDouble(right, out var rd))
        {
            switch (bin.Operator)
            {
                case BinaryOperator.Add:
                    return new RealLiteralExprNode((ld + rd).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Subtract:
                    return new RealLiteralExprNode((ld - rd).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Multiply:
                    return new RealLiteralExprNode((ld * rd).ToString(), bin.Line, bin.Column);
                case BinaryOperator.Divide:
                    if (rd == 0.0) return null;
                    return new RealLiteralExprNode((ld / rd).ToString(), bin.Line, bin.Column);

                case BinaryOperator.GreaterThan:
                    return new BoolLiteralExprNode(ld > rd, bin.Line, bin.Column);
                case BinaryOperator.LessThan:
                    return new BoolLiteralExprNode(ld < rd, bin.Line, bin.Column);
                case BinaryOperator.GreaterThanOrEqual:
                    return new BoolLiteralExprNode(ld >= rd, bin.Line, bin.Column);
                case BinaryOperator.LessThanOrEqual:
                    return new BoolLiteralExprNode(ld <= rd, bin.Line, bin.Column);
                case BinaryOperator.Equal:
                    return new BoolLiteralExprNode(Math.Abs(ld - rd) < double.Epsilon, bin.Line, bin.Column);
                case BinaryOperator.NotEqual:
                    return new BoolLiteralExprNode(Math.Abs(ld - rd) >= double.Epsilon, bin.Line, bin.Column);
            }
        }

        // Boolean == / !=
        if (left is BoolLiteralExprNode lb && right is BoolLiteralExprNode rb)
        {
            switch (bin.Operator)
            {
                case BinaryOperator.Equal:
                    return new BoolLiteralExprNode(lb.Value == rb.Value, bin.Line, bin.Column);
                case BinaryOperator.NotEqual:
                    return new BoolLiteralExprNode(lb.Value != rb.Value, bin.Line, bin.Column);
            }
        }

        return null;
    }

    private static bool TryGetDouble(ExprNode expr, out double value)
    {
        switch (expr)
        {
            case IntLiteralExprNode il when double.TryParse(il.Value, out var d1):
                value = d1;
                return true;
            case RealLiteralExprNode rl when double.TryParse(rl.Value, out var d2):
                value = d2;
                return true;
            default:
                value = 0;
                return false;
        }
    }
}


