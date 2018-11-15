﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseExpressionBodiedMemberAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.UseExpressionBodiedMember,
                    DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeOperatorDeclaration, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeDestructorDeclaration, SyntaxKind.DestructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalFunctionStatement, SyntaxKind.LocalFunctionStatement);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.SetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.AddAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.RemoveAccessorDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            BlockSyntax body = method.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (OperatorDeclarationSyntax)context.Node;

            BlockSyntax body = declaration.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetReturnExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ConversionOperatorDeclarationSyntax)context.Node;

            BlockSyntax body = declaration.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetReturnExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ConstructorDeclarationSyntax)context.Node;

            BlockSyntax body = declaration.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeDestructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (DestructorDeclarationSyntax)context.Node;

            BlockSyntax body = declaration.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeLocalFunctionStatement(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;

            BlockSyntax body = localFunction.Body;

            if (body == null)
                return;

            ExpressionSyntax expression = UseExpressionBodiedMemberAnalysis.GetExpression(body);

            if (expression == null)
                return;

            AnalyzeExpression(context, body, expression);
        }

        private static void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var accessor = (AccessorDeclarationSyntax)context.Node;

            BlockSyntax body = accessor.Body;

            if (body == null)
                return;

            if (accessor.AttributeLists.Any())
                return;

            bool isGetter = accessor.IsKind(SyntaxKind.GetAccessorDeclaration);

            ExpressionSyntax expression = (isGetter)
                ? UseExpressionBodiedMemberAnalysis.GetReturnExpression(body)
                : UseExpressionBodiedMemberAnalysis.GetExpression(body);

            if (expression?.IsSingleLine() != true)
                return;

            if (isGetter
                && accessor.Parent is AccessorListSyntax accessorList
                && accessorList.Accessors.Count == 1)
            {
                if (accessorList.DescendantTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                {
                    ReportDiagnostic(context, accessorList, expression);
                    DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, accessor.Keyword);
                    CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, body);
                }

                return;
            }

            if (accessor.DescendantTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                ReportDiagnostic(context, body, expression);
        }

        private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, BlockSyntax block, ExpressionSyntax expression)
        {
            if (block.DescendantTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia())
                && expression.IsSingleLine())
            {
                ReportDiagnostic(context, block, expression);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, BlockSyntax block, ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseExpressionBodiedMember, block);

            if (expression.Parent is ReturnStatementSyntax returnStatement)
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, returnStatement.ReturnKeyword);

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, block);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, AccessorListSyntax accessorList, ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseExpressionBodiedMember, accessorList);

            if (expression.Parent is ReturnStatementSyntax returnStatement)
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, returnStatement.ReturnKeyword);

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.UseExpressionBodiedMemberFadeOut, accessorList);
        }
    }
}
