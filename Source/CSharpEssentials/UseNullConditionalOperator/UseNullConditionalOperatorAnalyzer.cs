using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.UseNullConditionalOperator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseNullConditionalOperatorAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.UseNullConditionalOperatorFadedToken, DiagnosticDescriptors.UseNullConditionalOperator);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeArgument, SyntaxKind.ConditionalExpression);
        }

        private static void AnalyzeArgument(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SyntaxTree.IsGeneratedCode())
            {
                return;
            }

            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;
            var condition = conditionalExpression.Condition;
            if (condition.Kind() != SyntaxKind.EqualsExpression &&
                condition.Kind() != SyntaxKind.NotEqualsExpression)
            {
                // has to be of the form:
                //      <expr> == <expr> ? ...    or
                //      <expr> != <expr> ? ... 
                return;
            }

            var binaryCondition = (BinaryExpressionSyntax)condition;
            if (binaryCondition.Right.Kind() != SyntaxKind.NullLiteralExpression)
            {
                // has to be of the form:
                //      <expr> == null ? ...    or
                //      <expr> != null ? ... 
                return;
            }

            var whenTrue = conditionalExpression.WhenTrue;
            var whenFalse = conditionalExpression.WhenFalse;
            if (conditionalExpression.Kind() == SyntaxKind.EqualsExpression &&
                whenTrue.Kind() != SyntaxKind.NullLiteralExpression)
            {
                // If it is an equality check, then it has to be of the form:
                //      <expr> == null ? null : ...
                return;
            }

            if (conditionalExpression.Kind() == SyntaxKind.NotEqualsExpression &&
                whenFalse.Kind() != SyntaxKind.NullLiteralExpression)
            {
                // If it is an inequality check, then it has to be of the form:
                //      <expr> != null ? ... : null
                return;
            }

            // Looks good to go.  Do the more expensive check.
            var accessExpression = binaryCondition.Kind() == SyntaxKind.EqualsExpression
                ? whenFalse
                : whenTrue;

            var semanticModel = context.SemanticModel;
            var typeInfo = semanticModel.GetTypeInfo(accessExpression);
            if (typeInfo.ConvertedType != null && typeInfo.ConvertedType.IsValueType)
            {
                // We don't want to offer this change if the expression evaluates out to a value
                // type.  Post the change the value type will now be a nullable instead, and that
                // can easily break code.
                return;
            }

            if (!StartsWith(semanticModel, binaryCondition.Left, accessExpression))
            {
                // whenTrue/whenFalse expressoin didn't start with the same expression we were
                // testing against null.
                return;
            }

            // We found a match inside the appropriate side of the branch.  Let the user know.
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UseNullConditionalOperator, conditionalExpression.GetLocation()));

            FadeOut(context, binaryCondition);
            FadeOut(context, conditionalExpression.QuestionToken);
            FadeOut(context, conditionalExpression.ColonToken);
            if (binaryCondition.Kind() == SyntaxKind.EqualsExpression)
            {
                FadeOut(context, whenTrue);
            }
            else
            {
                FadeOut(context, whenFalse);
            }
        }

        private static void FadeOut(SyntaxNodeAnalysisContext context, SyntaxNodeOrToken nodeOrToken)
        {
            if (!nodeOrToken.IsMissing)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseNullConditionalOperatorFadedToken, nodeOrToken.GetLocation()));
            }
        }

        private static bool StartsWith(
            SemanticModel semanticModel,
            ExpressionSyntax condition,
            ExpressionSyntax within)
        {
            while (true)
            {
                if (within.Kind() == SyntaxKind.InvocationExpression)
                {
                    within = ((InvocationExpressionSyntax)within).Expression;
                    continue;
                }
                else if (within.Kind() == SyntaxKind.ElementAccessExpression)
                {
                    within = ((ElementAccessExpressionSyntax)within).Expression;
                    if (within.IsEquivalentTo(condition, topLevel: false))
                    {
                        // Found it!
                        return true;
                    }

                    continue;
                }
                else if (within.Kind() == SyntaxKind.SimpleMemberAccessExpression)
                {
                    within = ((MemberAccessExpressionSyntax)within).Expression;
                    if (within.IsEquivalentTo(condition, topLevel: false)) 
                    {
                        // Found it!
                        return true;
                    }

                    continue;
                }

                return false;
            }
        }
    }
}
