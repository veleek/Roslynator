﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.UseMethodChaining
{
    internal abstract class UseMethodChainingRefactoring
    {
        //TODO: UseMethodChainingAnalysis
        public UseMethodChainingAnalysis Analysis { get; }

        protected abstract InvocationExpressionSyntax GetInvocationExpression(ExpressionStatementSyntax expressionStatement);

        public async Task<Document> RefactorAsync(
            Document document,
            ExpressionStatementSyntax expressionStatement,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            InvocationExpressionSyntax invocationExpression = GetInvocationExpression(expressionStatement);

            MemberInvocationExpressionInfo invocationInfo = SyntaxInfo.MemberInvocationExpressionInfo(invocationExpression);

            ITypeSymbol returnType = semanticModel.GetMethodSymbol(invocationExpression, cancellationToken).ReturnType;

            string name = ((IdentifierNameSyntax)UseMethodChainingAnalysis.WalkDownMethodChain(invocationInfo).Expression).Identifier.ValueText;

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(expressionStatement);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(expressionStatement);

            string indentation = expressionStatement.GetIncreasedIndentation(cancellationToken).ToString();

            var sb = new StringBuilder(invocationExpression.ToString());

            int j = index;
            while (j < statements.Count - 1)
            {
                StatementSyntax statement = statements[j + 1];

                if (!Analysis.IsFixableStatement(statement, name, returnType, semanticModel, cancellationToken))
                    break;

                sb.AppendLine();
                sb.Append(indentation);
                sb.Append(GetTextToAppend((ExpressionStatementSyntax)statement));

                j++;
            }

            StatementSyntax lastStatement = statements[j];

            SyntaxList<StatementSyntax> newStatements = statements;

            while (j > index)
            {
                newStatements = newStatements.RemoveAt(j);
                j--;
            }

            ExpressionSyntax newInvocationExpression = SyntaxFactory.ParseExpression(sb.ToString());

            SyntaxTriviaList trailingTrivia = statementsInfo
                .Parent
                .DescendantTrivia(TextSpan.FromBounds(invocationExpression.Span.End, lastStatement.Span.End))
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace()
                .AddRange(lastStatement.GetTrailingTrivia());

            ExpressionStatementSyntax newExpressionStatement = expressionStatement
                .ReplaceNode(invocationExpression, newInvocationExpression)
                .WithLeadingTrivia(expressionStatement.GetLeadingTrivia())
                .WithTrailingTrivia(trailingTrivia)
                .WithFormatterAndSimplifierAnnotation();

            newStatements = newStatements.ReplaceAt(index, newExpressionStatement);

            return await document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken).ConfigureAwait(false);
        }

        private string GetTextToAppend(ExpressionStatementSyntax expressionStatement)
        {
            MemberInvocationExpressionInfo invocationInfo = SyntaxInfo.MemberInvocationExpressionInfo(GetInvocationExpression(expressionStatement));

            MemberInvocationExpressionInfo firstMemberInvocation = UseMethodChainingAnalysis.WalkDownMethodChain(invocationInfo);

            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            return invocationExpression
                .ToString()
                .Substring(firstMemberInvocation.OperatorToken.SpanStart - invocationExpression.SpanStart);
        }
    }
}