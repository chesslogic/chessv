using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    internal enum ApmwFuzzFailureKind
    {
        ConfigurationException,
        ItemHandlerGenerationException,
        StandardStartupException,
        SuperStartupException,
        MoveGenerationException,
        InvariantAssertionFailure,
        Timeout,
        UnexpectedException,
    }

    internal sealed class ApmwFuzzFailure
    {
        public ApmwFuzzFailure(
            ApmwFuzzFailureKind kind,
            string stageName,
            ApmwFuzzCase fuzzCase,
            Exception exception)
            : this(kind, stageName, fuzzCase, exception, null)
        {
        }

        private ApmwFuzzFailure(
            ApmwFuzzFailureKind kind,
            string stageName,
            ApmwFuzzCase fuzzCase,
            Exception exception,
            ApmwFuzzFailure minimizedFailure)
        {
            FuzzCase = fuzzCase ?? throw new ArgumentNullException(nameof(fuzzCase));
            Exception = exception;
            MinimizedFailure = minimizedFailure;

            Kind = kind;
            StageName = stageName ?? string.Empty;
            CaseLabel = fuzzCase.Label;
            CaseCategory = fuzzCase.Category;
            MasterSeed = fuzzCase.MasterSeed;
            CaseIndex = fuzzCase.CaseIndex;
            CaseDiagnostics = fuzzCase.ToDiagnosticString();
            ExceptionType = exception == null ? string.Empty : exception.GetType().FullName;
            ExceptionMessage = exception == null ? string.Empty : exception.Message;
            ExceptionStackTrace = exception == null ? string.Empty : exception.StackTrace ?? string.Empty;
        }

        public ApmwFuzzFailureKind Kind { get; }

        public string StageName { get; }

        public string CaseLabel { get; }

        public string CaseCategory { get; }

        public string MasterSeed { get; }

        public int CaseIndex { get; }

        public string CaseDiagnostics { get; }

        public string ExceptionType { get; }

        public string ExceptionMessage { get; }

        public string ExceptionStackTrace { get; }

        public Exception Exception { get; }

        public ApmwFuzzCase FuzzCase { get; }

        public ApmwFuzzFailure MinimizedFailure { get; }

        public ApmwFuzzFailure WithMinimizedFailure(ApmwFuzzFailure minimizedFailure)
        {
            if (minimizedFailure == null)
                throw new ArgumentNullException(nameof(minimizedFailure));

            if (minimizedFailure.CaseDiagnostics == CaseDiagnostics)
                return this;

            return new ApmwFuzzFailure(Kind, StageName, FuzzCase, Exception, minimizedFailure);
        }

        public string ToDiagnosticString(int failureNumber)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Failure #" + failureNumber);
            builder.AppendLine("  Kind: " + Kind);
            builder.AppendLine("  Stage: " + StageName);
            builder.AppendLine("  Case: " + CaseLabel);
            builder.AppendLine("  Category: " + CaseCategory);
            builder.AppendLine("  Master seed: " + MasterSeed);
            builder.AppendLine("  Case index: " + CaseIndex);
            builder.AppendLine("  Exception type: " + ExceptionType);
            builder.AppendLine("  Exception message: " + ExceptionMessage);

            if (!string.IsNullOrEmpty(ExceptionStackTrace))
            {
                builder.AppendLine("  Exception stack:");
                AppendIndented(builder, ExceptionStackTrace, "    ");
            }

            if (Exception != null)
            {
                builder.AppendLine("  Exception details:");
                AppendIndented(builder, Exception.ToString(), "    ");
            }

            builder.AppendLine("  Case diagnostics:");
            AppendIndented(builder, CaseDiagnostics, "    ");
            if (MinimizedFailure != null)
            {
                builder.AppendLine("  Minimized repro case:");
                builder.AppendLine("    Case: " + MinimizedFailure.CaseLabel);
                builder.AppendLine("    Master seed: " + MinimizedFailure.MasterSeed);
                builder.AppendLine("    Case index: " + MinimizedFailure.CaseIndex);
                if (MinimizedFailure.ExceptionType != ExceptionType ||
                    MinimizedFailure.ExceptionMessage != ExceptionMessage)
                {
                    builder.AppendLine("    Exception type: " + MinimizedFailure.ExceptionType);
                    builder.AppendLine("    Exception message: " + MinimizedFailure.ExceptionMessage);
                }

                builder.AppendLine("    Case diagnostics:");
                AppendIndented(builder, MinimizedFailure.CaseDiagnostics, "      ");
            }

            return builder.ToString();
        }

        private static void AppendIndented(StringBuilder builder, string value, string indent)
        {
            string[] lines = (value ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            foreach (string line in lines)
                builder.AppendLine(indent + line);
        }
    }

    internal sealed class ApmwFuzzRunResult
    {
        public ApmwFuzzRunResult(
            int totalCaseCount,
            int executedCaseCount,
            int maxFailures,
            IEnumerable<ApmwFuzzFailure> failures)
        {
            if (totalCaseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCaseCount), "Total case count cannot be negative.");
            if (executedCaseCount < 0 || executedCaseCount > totalCaseCount)
                throw new ArgumentOutOfRangeException(nameof(executedCaseCount), "Executed case count must be between zero and total case count.");
            if (maxFailures <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFailures), "Failure cap must be greater than zero.");

            TotalCaseCount = totalCaseCount;
            ExecutedCaseCount = executedCaseCount;
            MaxFailures = maxFailures;
            Failures = new ReadOnlyCollection<ApmwFuzzFailure>(
                (failures ?? Enumerable.Empty<ApmwFuzzFailure>()).ToList());
        }

        public int TotalCaseCount { get; }

        public int ExecutedCaseCount { get; }

        public int MaxFailures { get; }

        public IReadOnlyList<ApmwFuzzFailure> Failures { get; }

        public bool Succeeded { get { return Failures.Count == 0; } }

        public bool ReachedFailureCap { get { return Failures.Count >= MaxFailures; } }

        public void AssertNoFailures()
        {
            if (!Succeeded)
                Assert.Fail(ToFailureMessage());
        }

        public string ToFailureMessage()
        {
            if (Succeeded)
                return "APMW fuzz run succeeded for " + ExecutedCaseCount + " case(s).";

            var builder = new StringBuilder();
            builder.Append("APMW fuzz run collected ");
            builder.Append(Failures.Count);
            builder.Append(" failure(s) across ");
            builder.Append(ExecutedCaseCount);
            builder.Append(" executed case(s) out of ");
            builder.Append(TotalCaseCount);
            builder.Append(" total case(s), maxFailures=");
            builder.Append(MaxFailures);
            builder.AppendLine(".");

            if (ReachedFailureCap)
            {
                builder.AppendLine(ExecutedCaseCount < TotalCaseCount
                    ? "Failure cap reached; remaining cases were not executed."
                    : "Failure cap reached on the final executed case.");
            }

            for (int index = 0; index < Failures.Count; index++)
            {
                builder.AppendLine();
                builder.Append(Failures[index].ToDiagnosticString(index + 1));
            }

            return builder.ToString();
        }
    }

    internal static class ApmwFuzzRunner
    {
        public static ApmwFuzzRunResult RunCases(IEnumerable<ApmwFuzzCase> cases, int maxFailures)
        {
            return RunCases(cases, maxFailures, ApmwFuzzStage.MoveGeneration);
        }

        public static ApmwFuzzRunResult RunCases(
            IEnumerable<ApmwFuzzCase> cases,
            int maxFailures,
            ApmwFuzzStage finalStage)
        {
            if (cases == null)
                throw new ArgumentNullException(nameof(cases));
            if (maxFailures <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFailures), "Failure cap must be greater than zero.");

            var materializedCases = cases.ToList();
            if (materializedCases.Count == 0)
                throw new ArgumentException("At least one APMW fuzz case is required.", nameof(cases));

            var runnableCases = DeduplicateCases(materializedCases).ToList();
            var failures = new List<ApmwFuzzFailure>();
            int executedCaseCount = 0;

            foreach (var fuzzCase in runnableCases)
            {
                if (failures.Count >= maxFailures)
                    break;

                executedCaseCount++;
                var failure = RunCase(fuzzCase, finalStage);
                if (failure != null)
                    failures.Add(failure);
            }

            return new ApmwFuzzRunResult(
                materializedCases.Count,
                executedCaseCount,
                maxFailures,
                failures);
        }

        private static IEnumerable<ApmwFuzzCase> DeduplicateCases(IEnumerable<ApmwFuzzCase> cases)
        {
            var seenOptionKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (ApmwFuzzCase fuzzCase in cases)
            {
                if (fuzzCase == null || seenOptionKeys.Add(fuzzCase.CanonicalOptionKey))
                    yield return fuzzCase;
            }
        }

        public static void AssertNoFailures(ApmwFuzzRunResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.AssertNoFailures();
        }

        private static ApmwFuzzFailure RunCase(ApmwFuzzCase fuzzCase, ApmwFuzzStage finalStage)
        {
            return RunCase(fuzzCase, finalStage, true);
        }

        private static ApmwFuzzFailure RunCase(ApmwFuzzCase fuzzCase, ApmwFuzzStage finalStage, bool allowShrinking)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            ApmwFuzzFailure failure;
            ApmwFuzzScope scope;
            if (!TryRunStage(fuzzCase, ApmwFuzzStage.Configuration, () => ApmwFuzzScope.Configure(fuzzCase), out scope, out failure))
                return FinalizeFailure(failure, finalStage, allowShrinking);

            using (scope)
            {
                if (ShouldStopAfter(ApmwFuzzStage.Configuration, finalStage))
                    return null;

                ApmwFuzzGenerationResult generation;
                if (!TryRunStage(fuzzCase, ApmwFuzzStage.ItemHandlerGeneration, () => scope.RunItemHandlerGeneration(), out generation, out failure))
                    return FinalizeFailure(failure, finalStage, allowShrinking);
                if (!TryRunStage(fuzzCase, ValidationStageName(ApmwFuzzStage.ItemHandlerGeneration), ApmwFuzzStage.InvariantValidation, () =>
                    AssertGenerationResult(fuzzCase, generation), out failure))
                    return FinalizeFailure(failure, finalStage, allowShrinking);
                if (ShouldStopAfter(ApmwFuzzStage.ItemHandlerGeneration, finalStage))
                    return null;

                ApmwFuzzStage startupStage = ApmwFuzzStages.BoardStartup(fuzzCase.IsSuperSized);
                ApmwFuzzStartupResult startup;
                if (!TryRunStage(fuzzCase, startupStage, () => scope.RunGameStartup(), out startup, out failure))
                    return FinalizeFailure(failure, finalStage, allowShrinking);
                if (!TryRunStage(fuzzCase, ValidationStageName(startupStage), ApmwFuzzStage.InvariantValidation, () =>
                    AssertStartupResult(fuzzCase, startup), out failure))
                    return FinalizeFailure(failure, finalStage, allowShrinking);
                if (ShouldStopAfter(startupStage, finalStage))
                    return null;

                if (!TryRunStage(fuzzCase, ApmwFuzzStage.MoveGeneration, () => RunInitialMoveGeneration(fuzzCase, startup), out failure))
                    return FinalizeFailure(failure, finalStage, allowShrinking);
            }

            return null;
        }

        private static ApmwFuzzFailure FinalizeFailure(
            ApmwFuzzFailure failure,
            ApmwFuzzStage finalStage,
            bool allowShrinking)
        {
            if (!allowShrinking)
                return failure;

            return TryShrinkFailure(failure, finalStage);
        }

        private static ApmwFuzzFailure TryShrinkFailure(ApmwFuzzFailure failure, ApmwFuzzStage finalStage)
        {
            if (failure == null)
                return failure;

            ApmwFuzzFailure currentFailure = failure;
            ApmwFuzzCase currentCase = failure.FuzzCase;
            var testedOptionKeys = new HashSet<string>(StringComparer.Ordinal) { currentCase.CanonicalOptionKey };
            bool reduced;

            do
            {
                reduced = false;
                foreach (var candidate in ShrinkCandidates(currentCase))
                {
                    if (!testedOptionKeys.Add(candidate.CanonicalOptionKey))
                        continue;

                    ApmwFuzzFailure candidateFailure = RunCase(candidate, finalStage, false);
                    if (candidateFailure != null && IsSameFailure(failure, candidateFailure))
                    {
                        currentCase = candidate;
                        currentFailure = candidateFailure;
                        reduced = true;
                        break;
                    }
                }
            }
            while (reduced);

            return failure.WithMinimizedFailure(currentFailure);
        }

        private static bool IsSameFailure(ApmwFuzzFailure expected, ApmwFuzzFailure actual)
        {
            return expected.Kind == actual.Kind &&
                expected.StageName == actual.StageName &&
                expected.ExceptionType == actual.ExceptionType;
        }

        internal static IEnumerable<ApmwFuzzCase> ShrinkCandidates(ApmwFuzzCase fuzzCase)
        {
            var seenOptionKeys = new HashSet<string>(StringComparer.Ordinal) { fuzzCase.CanonicalOptionKey };

            foreach (var candidate in BuildShrinkCandidates(fuzzCase))
            {
                if (seenOptionKeys.Add(candidate.CanonicalOptionKey))
                    yield return candidate;
            }
        }

        private static IEnumerable<ApmwFuzzCase> BuildShrinkCandidates(ApmwFuzzCase fuzzCase)
        {
            foreach (ApmwFuzzAxis axis in ApmwFuzzCaseOptionVector.DefaultOptionSpace.Axes)
            {
                foreach (ApmwFuzzOptionValue value in ShrinkValues(axis, fuzzCase))
                {
                    yield return fuzzCase.With(builder =>
                        ApmwFuzzOptionSpace.ApplyCaseValue(builder, axis.Name, value));
                }
            }

            ApmwFuzzCase defaults = DefaultCaseForShrink(fuzzCase);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.PocketSeed = defaultCase.PocketSeed);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.PawnSeed = defaultCase.PawnSeed);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.MinorSeed = defaultCase.MinorSeed);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.MajorSeed = defaultCase.MajorSeed);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.QueenSeed = defaultCase.QueenSeed);
            yield return Reset(fuzzCase, defaults, (builder, defaultCase) => builder.DeterministicChaosSeed = defaultCase.DeterministicChaosSeed);
        }

        private static IEnumerable<ApmwFuzzOptionValue> ShrinkValues(ApmwFuzzAxis axis, ApmwFuzzCase fuzzCase)
        {
            ApmwFuzzOptionValue currentValue = ApmwFuzzOptionSpace.BuildCanonicalCaseValue(axis, fuzzCase);
            var seenValueKeys = new HashSet<string>(StringComparer.Ordinal) { currentValue.CanonicalKey };
            bool currentIsDefault = string.Equals(
                currentValue.CanonicalKey,
                axis.DefaultValue.CanonicalKey,
                StringComparison.Ordinal);

            if (currentIsDefault)
                yield break;

            if (seenValueKeys.Add(axis.DefaultValue.CanonicalKey))
                yield return axis.DefaultValue;

            foreach (ApmwFuzzOptionValue value in axis.Values)
            {
                if (seenValueKeys.Add(value.CanonicalKey))
                    yield return value;
            }
        }

        private static ApmwFuzzCase Reset(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzCase defaults,
            Action<ApmwFuzzCase.Builder, ApmwFuzzCase> reset)
        {
            return fuzzCase.With(builder => reset(builder, defaults));
        }

        private static ApmwFuzzCase DefaultCaseForShrink(ApmwFuzzCase fuzzCase)
        {
            ApmwFuzzCase defaults = fuzzCase.IsSuperSized
                ? ApmwFuzzCase.DefaultSuperSized()
                : ApmwFuzzCase.DefaultStandard();

            return defaults.With(builder =>
            {
                builder.CaseName = fuzzCase.CaseName;
                builder.CaseIndex = fuzzCase.CaseIndex;
                builder.MasterSeed = fuzzCase.MasterSeed;
                builder.TargetStage = fuzzCase.TargetStage;
                builder.Category = fuzzCase.Category;
                builder.IsSuperSized = fuzzCase.IsSuperSized;
            });
        }

        private static bool TryRunStage<T>(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStage stage,
            Func<T> action,
            out T result,
            out ApmwFuzzFailure failure)
        {
            return TryRunStage(fuzzCase, ApmwFuzzStages.GetName(stage), stage, action, out result, out failure);
        }

        private static bool TryRunStage(
            ApmwFuzzCase fuzzCase,
            ApmwFuzzStage stage,
            Action action,
            out ApmwFuzzFailure failure)
        {
            return TryRunStage(fuzzCase, ApmwFuzzStages.GetName(stage), stage, action, out failure);
        }

        private static bool TryRunStage(
            ApmwFuzzCase fuzzCase,
            string stageName,
            ApmwFuzzStage stage,
            Action action,
            out ApmwFuzzFailure failure)
        {
            bool result;
            return TryRunStage(fuzzCase, stageName, stage, () =>
            {
                action();
                return true;
            }, out result, out failure);
        }

        private static bool TryRunStage<T>(
            ApmwFuzzCase fuzzCase,
            string stageName,
            ApmwFuzzStage stage,
            Func<T> action,
            out T result,
            out ApmwFuzzFailure failure)
        {
            try
            {
                result = action();
                failure = null;
                return true;
            }
            catch (Exception ex)
            {
                result = default(T);
                failure = new ApmwFuzzFailure(ClassifyFailure(stage, ex), stageName, fuzzCase, ex);
                return false;
            }
        }

        private static ApmwFuzzFailureKind ClassifyFailure(ApmwFuzzStage stage, Exception exception)
        {
            if (exception is TimeoutException)
                return ApmwFuzzFailureKind.Timeout;
            if (IsAssertionFailure(exception))
                return ApmwFuzzFailureKind.InvariantAssertionFailure;

            switch (stage)
            {
                case ApmwFuzzStage.Configuration:
                    return ApmwFuzzFailureKind.ConfigurationException;
                case ApmwFuzzStage.ItemHandlerGeneration:
                    return ApmwFuzzFailureKind.ItemHandlerGenerationException;
                case ApmwFuzzStage.StandardBoardStartup:
                    return ApmwFuzzFailureKind.StandardStartupException;
                case ApmwFuzzStage.SuperBoardStartup:
                    return ApmwFuzzFailureKind.SuperStartupException;
                case ApmwFuzzStage.MoveGeneration:
                    return ApmwFuzzFailureKind.MoveGenerationException;
                default:
                    return ApmwFuzzFailureKind.UnexpectedException;
            }
        }

        private static bool IsAssertionFailure(Exception exception)
        {
            return exception is AssertFailedException || exception is AssertInconclusiveException;
        }

        private static bool ShouldStopAfter(ApmwFuzzStage completedStage, ApmwFuzzStage finalStage)
        {
            return StageOrder(completedStage) >= StageOrder(finalStage);
        }

        private static int StageOrder(ApmwFuzzStage stage)
        {
            switch (stage)
            {
                case ApmwFuzzStage.Configuration:
                    return 0;
                case ApmwFuzzStage.ItemHandlerGeneration:
                    return 1;
                case ApmwFuzzStage.StandardBoardStartup:
                case ApmwFuzzStage.SuperBoardStartup:
                    return 2;
                case ApmwFuzzStage.MoveGeneration:
                    return 3;
                case ApmwFuzzStage.InvariantValidation:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, "Unknown APMW fuzz stage.");
            }
        }

        private static void AssertGenerationResult(ApmwFuzzCase fuzzCase, ApmwFuzzGenerationResult result)
        {
            string stageName = ValidationStageName(ApmwFuzzStage.ItemHandlerGeneration);
            Assert.IsNotNull(result, StageMessage(fuzzCase, stageName, "result was null."));
            Assert.AreEqual(
                ApmwFuzzStage.ItemHandlerGeneration,
                result.Stage,
                StageMessage(fuzzCase, stageName, "result stage did not match item handler generation."));
            Assert.AreEqual(
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration,
                result.TargetStage,
                StageMessage(fuzzCase, stageName, "result target stage did not match item handler generation."));
            Assert.AreEqual(
                fuzzCase.IsSuperSized ? 10 : 8,
                result.NumFiles,
                StageMessage(fuzzCase, stageName, "generated board width did not match the case."));
            Assert.IsNotNull(
                result.PlayerPieceSet,
                StageMessage(fuzzCase, stageName, "player piece set was null."));
            Assert.IsNotNull(
                result.PromotionTypes,
                StageMessage(fuzzCase, stageName, "promotion type string was null."));
            Assert.IsNotNull(
                result.PocketPieces,
                StageMessage(fuzzCase, stageName, "pocket piece list was null."));
            ApmwFuzzInvariants.AssertGenerationInvariants(fuzzCase, result, stageName);
        }

        private static void AssertStartupResult(ApmwFuzzCase fuzzCase, ApmwFuzzStartupResult result)
        {
            ApmwFuzzStage expectedStage = ApmwFuzzStages.BoardStartup(fuzzCase.IsSuperSized);
            string stageName = ValidationStageName(expectedStage);
            Assert.IsNotNull(result, StageMessage(fuzzCase, stageName, "result was null."));
            Assert.AreEqual(
                expectedStage,
                result.Stage,
                StageMessage(fuzzCase, stageName, "result stage did not match the case board startup stage."));
            Assert.AreEqual(
                ApmwFuzzCase.TargetStages.BoardStartup(fuzzCase.IsSuperSized),
                result.TargetStage,
                StageMessage(fuzzCase, stageName, "result target stage did not match the case board startup stage."));
            Assert.AreEqual(
                fuzzCase.GameName,
                result.GameName,
                StageMessage(fuzzCase, stageName, "startup game name did not match the case."));
            Assert.AreEqual(
                fuzzCase.IsSuperSized,
                result.IsSuperSized,
                StageMessage(fuzzCase, stageName, "startup super-sized flag did not match the case."));
            Assert.AreEqual(
                fuzzCase.IsSuperSized ? 10 : 8,
                result.ExpectedNumFiles,
                StageMessage(fuzzCase, stageName, "expected board width did not match the case."));
            Assert.IsNotNull(
                result.Game,
                StageMessage(fuzzCase, stageName, "created game was null."));
            Assert.AreEqual(
                fuzzCase.GameName,
                result.Game.GameAttribute.GameName,
                StageMessage(fuzzCase, stageName, "created game name did not match Manager.CreateGame input."));
            Assert.AreEqual(
                result.ExpectedNumFiles,
                result.Game.NumFiles,
                StageMessage(fuzzCase, stageName, "created game file count did not match the case."));
            Assert.IsNotNull(
                result.Game.Board,
                StageMessage(fuzzCase, stageName, "created game board was null."));
            Assert.AreEqual(
                result.ExpectedNumFiles,
                result.Game.Board.NumFiles,
                StageMessage(fuzzCase, stageName, "created board file count did not match the case."));
            ApmwFuzzInvariants.AssertCreatedGameInvariants(fuzzCase, result, stageName);
        }

        private static void RunInitialMoveGeneration(ApmwFuzzCase fuzzCase, ApmwFuzzStartupResult startup)
        {
            ApmwFuzzInvariants.AssertInitialMoveGeneration(
                fuzzCase,
                startup,
                ApmwFuzzStages.GetName(ApmwFuzzStage.MoveGeneration));
        }

        private static string ValidationStageName(ApmwFuzzStage validatedStage)
        {
            return ApmwFuzzStages.GetName(ApmwFuzzStage.InvariantValidation) + "/" + ApmwFuzzStages.GetName(validatedStage);
        }

        private static string StageMessage(ApmwFuzzCase fuzzCase, string stageName, string message)
        {
            return stageName + " failed for " + fuzzCase.ToDiagnosticString() + ": " + message;
        }
    }
}
