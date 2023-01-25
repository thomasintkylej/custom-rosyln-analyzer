using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace CustomAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FirstCustomAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TI00001";

        private const string _category = "Design";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, 
            "Custom analyzer, is this working?", 
            "This is highlighted due to a custom analyzer", 
            _category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: "This is my analyzer description.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(CheckForMethodInvocationInLoop, SyntaxKind.InvocationExpression);
        }

        private void CheckForMethodInvocationInLoop(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var containsMethod = false;
            foreach (var token in invocation.Expression?.DescendantTokens() ?? Enumerable.Empty<SyntaxToken>())
            {
                // Do a fast scan to see if text contains possible match to our anticipated method names
                if (_methodNames.Contains(token.Text))
                {
                    // TODO: Check for Namespace/Assembly for a more precise lookup

                    containsMethod = true;
                    break;
                }
            }

            if (!containsMethod)
                return;

            var loopFound = invocation.FirstAncestorOrSelf<StatementSyntax>(ss =>
                ss.IsKind(SyntaxKind.ForStatement) ||
                ss.IsKind(SyntaxKind.ForEachStatement) ||
                ss.IsKind(SyntaxKind.WhileStatement));

            if (loopFound != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
        }

        static readonly ImmutableHashSet<string> _methods = ImmutableHashSet.Create(StringComparer.Ordinal, "Send", "Publish");

        static readonly ImmutableHashSet<string> _methodNames = _methods.Select(m => m.Split('.').Last()).ToImmutableHashSet();
    }
}
