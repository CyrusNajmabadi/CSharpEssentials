using Microsoft.CodeAnalysis;

namespace CSharpEssentials
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor UseGetterOnlyAutoProperty = new DiagnosticDescriptor(
            id: DiagnosticIds.UseGetterOnlyAutoProperty,
            title: "Use getter-only auto properties",
            messageFormat: "Consider using a getter-only auto property",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public static readonly DiagnosticDescriptor UseExpressionBodiedMemberFadedToken = new DiagnosticDescriptor(
            id: "UseExpressionBodiedMemberFadedToken",
            title: "Use expression-bodied members",
            messageFormat: "Consider using an expression-bodied member",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public static readonly DiagnosticDescriptor UseExpressionBodiedMember = new DiagnosticDescriptor(
            id: DiagnosticIds.UseExpressionBodiedMember,
            title: "Use expression-bodied members",
            messageFormat: "Consider using an expression-bodied member",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UseNameOf = new DiagnosticDescriptor(
            id: DiagnosticIds.UseNameOf,
            title: "Use nameof when passing parameter names as arguments",
            messageFormat: "Consider using nameof for the parameter name, '{0}'",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UseNullConditionalOperator = new DiagnosticDescriptor(
            id: DiagnosticIds.UseNullConditionalOperator,
            title: "Use ?. instead of explicitly checking for 'null'",
            messageFormat: "Consider using ?.",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UseNullConditionalOperatorFadedToken = new DiagnosticDescriptor(
            id: "UseNullConditionalOperatorFadedToken",
            title: "Use ?. instead of explicitly checking for 'null'",
            messageFormat: "Consider using ?.",
            category: DiagnosticCategories.Language,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            customTags: new[] { WellKnownDiagnosticTags.Unnecessary });
    }
}