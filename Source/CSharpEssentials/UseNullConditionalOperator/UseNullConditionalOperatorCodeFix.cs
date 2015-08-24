using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpEssentials.UseNullConditionalOperator
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Use Null Conditional Operator")]
    internal class UseNullConditionalOperatorCodeFix : CodeFixProvider
    {
        private readonly object EquivalenceKey = new object();

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.UseNullConditionalOperator);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var conditionalExpression = root.FindNode(context.Span, getInnermostNodeForTie: true) as ConditionalExpressionSyntax;

            if (conditionalExpression != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create("Use ?.", c => FixAsync(context.Document, conditionalExpression, c)),
                    context.Diagnostics);
            }
        }

        private async Task<Document> FixAsync(
            Document document, ConditionalExpressionSyntax conditionalExpression, CancellationToken cancellationToken)
        {
            var binaryCondition = (BinaryExpressionSyntax)conditionalExpression.Condition;

            var accessExpression = binaryCondition.Kind() == SyntaxKind.EqualsExpression
                ? conditionalExpression.WhenFalse
                : conditionalExpression.WhenTrue;

            var updatedAccessExpression = accessExpression.Accept(new Rewriter(binaryCondition.Left));

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(conditionalExpression, updatedAccessExpression);

            return document.WithSyntaxRoot(newRoot);
        }

        internal class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ExpressionSyntax checkedExpression;

            public Rewriter(ExpressionSyntax checkedExpression)
            {
                this.checkedExpression = checkedExpression;
            }

            public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                if (node.Kind() == SyntaxKind.SimpleMemberAccessExpression &&
                    checkedExpression.IsEquivalentTo(node.Expression, topLevel: false))
                {
                    return ConditionalAccessExpression(
                        node.Expression,
                        MemberBindingExpression(node.OperatorToken, node.Name));
                }

                return node;
            }

            public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
            {
                if (checkedExpression.IsEquivalentTo(node.Expression, topLevel: false))
                {
                    return ConditionalAccessExpression(
                        node.Expression,
                        ElementBindingExpression(node.ArgumentList));
                }

                return node;
            }
        }
    }
}
