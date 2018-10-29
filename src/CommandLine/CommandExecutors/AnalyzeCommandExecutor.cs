﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Diagnostics;
using static Roslynator.Logger;
using System.Collections.Immutable;

namespace Roslynator.CommandLine
{
    internal class AnalyzeCommandExecutor : MSBuildWorkspaceCommandExecutor
    {
        public AnalyzeCommandExecutor(AnalyzeCommandLineOptions options, DiagnosticSeverity minimalSeverity, string language) : base(language)
        {
            Options = options;
            MinimalSeverity = minimalSeverity;
        }

        public AnalyzeCommandLineOptions Options { get; }

        public DiagnosticSeverity MinimalSeverity { get; }

        public override async Task<CommandResult> ExecuteAsync(ProjectOrSolution projectOrSolution, CancellationToken cancellationToken = default)
        {
            AssemblyResolver.Register();

            var codeAnalyzerOptions = new CodeAnalyzerOptions(
                ignoreAnalyzerReferences: Options.IgnoreAnalyzerReferences,
                ignoreCompilerDiagnostics: Options.IgnoreCompilerDiagnostics,
                reportFadeDiagnostics: Options.ReportFadeDiagnostics,
                reportSuppressedDiagnostics: Options.ReportSuppressedDiagnostics,
                executionTime: Options.ExecutionTime,
                minimalSeverity: MinimalSeverity,
                supportedDiagnosticIds: Options.SupportedDiagnostics,
                ignoredDiagnosticIds: Options.IgnoredDiagnostics,
                projectNames: Options.Projects,
                ignoredProjectNames: Options.IgnoredProjects,
                language: Language);

            CultureInfo culture = (Options.CultureName != null) ? CultureInfo.GetCultureInfo(Options.CultureName) : null;

            var codeAnalyzer = new CodeAnalyzer(analyzerAssemblies: Options.AnalyzerAssemblies, formatProvider: culture, options: codeAnalyzerOptions);

            if (projectOrSolution.IsProject)
            {
                Project project = projectOrSolution.AsProject();

                WriteLine($"Analyze project '{project.Name}'", Verbosity.Minimal);

                ProjectAnalysisResult result = await codeAnalyzer.AnalyzeProjectAsync(project, cancellationToken);
            }
            else
            {
                Solution solution = projectOrSolution.AsSolution();

                ImmutableArray<ProjectAnalysisResult> results = await codeAnalyzer.AnalyzeSolutionAsync(solution, cancellationToken);
            }

            return new CommandResult(true);
        }

        protected override void OperationCanceled(OperationCanceledException ex)
        {
            WriteLine("Analysis was canceled.", Verbosity.Quiet);
        }
    }
}
