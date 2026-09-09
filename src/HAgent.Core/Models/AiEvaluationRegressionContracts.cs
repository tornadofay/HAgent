using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAgent.Models
{
    public enum AiEvaluationRegressionRunStatus
    {
        Completed,
        Failed,
        Canceled
    }

    public enum AiEvaluationRegressionCaseStatus
    {
        Completed,
        Failed,
        Canceled
    }

    public sealed class AiEvaluationRegressionCase
    {
        public AiEvaluationRegressionCase()
        {
            Id = string.Empty;
            Name = string.Empty;
            Inputs = new List<AiEvaluationInputReference>();
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public IList<AiEvaluationInputReference> Inputs { get; private set; }
        public IDictionary<string, string> Parameters { get; private set; }

        public AiEvaluationRegressionCase Clone()
        {
            var clone = new AiEvaluationRegressionCase
            {
                Id = Id,
                Name = Name
            };

            foreach (var input in Inputs ?? new List<AiEvaluationInputReference>())
                if (input != null) clone.Inputs.Add(input.Clone());

            foreach (var pair in Parameters ?? new Dictionary<string, string>())
                clone.Parameters[pair.Key] = pair.Value;

            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            Require(Name, nameof(Name), 256);

            if (Inputs == null)
                throw new ArgumentNullException(nameof(Inputs));
            if (Inputs.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Inputs));
            foreach (var input in Inputs)
            {
                if (input == null)
                    throw new ArgumentException("Regression case inputs cannot contain null entries.", nameof(Inputs));
                input.Validate();
            }

            if (Parameters == null)
                throw new ArgumentNullException(nameof(Parameters));
            if (Parameters.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Parameters));
            foreach (var pair in Parameters)
            {
                Require(pair.Key, nameof(Parameters), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Parameters));
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationRegressionTarget
    {
        public AiEvaluationRegressionTarget()
        {
            Id = string.Empty;
            Name = string.Empty;
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public IDictionary<string, string> Metadata { get; private set; }

        public AiEvaluationRegressionTarget Clone()
        {
            var clone = new AiEvaluationRegressionTarget
            {
                Id = Id,
                Name = Name
            };

            foreach (var pair in Metadata ?? new Dictionary<string, string>())
                clone.Metadata[pair.Key] = pair.Value;

            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            Require(Name, nameof(Name), 256);

            if (Metadata == null)
                throw new ArgumentNullException(nameof(Metadata));
            if (Metadata.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Metadata));
            foreach (var pair in Metadata)
            {
                Require(pair.Key, nameof(Metadata), 128);
                if (pair.Value != null && pair.Value.Length > 2048)
                    throw new ArgumentOutOfRangeException(nameof(Metadata));
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationRegressionSuite
    {
        public AiEvaluationRegressionSuite()
        {
            Id = string.Empty;
            Name = string.Empty;
            Cases = new List<AiEvaluationRegressionCase>();
            Targets = new List<AiEvaluationRegressionTarget>();
            MaxConcurrency = 4;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public IList<AiEvaluationRegressionCase> Cases { get; private set; }
        public IList<AiEvaluationRegressionTarget> Targets { get; private set; }
        public int MaxConcurrency { get; set; }

        public AiEvaluationRegressionSuite Clone()
        {
            var clone = new AiEvaluationRegressionSuite
            {
                Id = Id,
                Name = Name,
                MaxConcurrency = MaxConcurrency
            };

            foreach (var item in Cases ?? new List<AiEvaluationRegressionCase>())
                if (item != null) clone.Cases.Add(item.Clone());

            foreach (var item in Targets ?? new List<AiEvaluationRegressionTarget>())
                if (item != null) clone.Targets.Add(item.Clone());

            return clone;
        }

        public void Validate()
        {
            Require(Id, nameof(Id), 128);
            Require(Name, nameof(Name), 256);
            if (MaxConcurrency < 1 || MaxConcurrency > 32)
                throw new ArgumentOutOfRangeException(nameof(MaxConcurrency));

            if (Cases == null)
                throw new ArgumentNullException(nameof(Cases));
            if (Cases.Count == 0)
                throw new ArgumentException("At least one regression case is required.", nameof(Cases));
            if (Cases.Count > 128)
                throw new ArgumentOutOfRangeException(nameof(Cases));

            if (Targets == null)
                throw new ArgumentNullException(nameof(Targets));
            if (Targets.Count == 0)
                throw new ArgumentException("At least one regression target is required.", nameof(Targets));
            if (Targets.Count > 32)
                throw new ArgumentOutOfRangeException(nameof(Targets));

            if (Cases.Count * Targets.Count > 1024)
                throw new ArgumentOutOfRangeException(nameof(Cases), "A regression suite may contain at most 1024 case-target executions.");

            EnsureUnique(Cases.Select(x => x == null ? null : x.Id), "Regression case identifiers");
            EnsureUnique(Targets.Select(x => x == null ? null : x.Id), "Regression target identifiers");

            foreach (var item in Cases)
            {
                if (item == null)
                    throw new ArgumentException("Regression suite cases cannot contain null entries.", nameof(Cases));
                item.Validate();
            }

            foreach (var item in Targets)
            {
                if (item == null)
                    throw new ArgumentException("Regression suite targets cannot contain null entries.", nameof(Targets));
                item.Validate();
            }
        }

        private static void EnsureUnique(IEnumerable<string> ids, string description)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                if (!seen.Add(id))
                    throw new ArgumentException(description + " must be unique.");
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationRegressionCaseResult
    {
        public AiEvaluationRegressionCaseResult()
        {
            CaseId = string.Empty;
            VariantId = string.Empty;
            ErrorCode = string.Empty;
        }

        public string CaseId { get; set; }
        public string VariantId { get; set; }
        public AiEvaluationRegressionCaseStatus Status { get; set; }
        public AiEvaluationSample Sample { get; set; }
        public string ErrorCode { get; set; }

        public AiEvaluationRegressionCaseResult Clone()
        {
            return new AiEvaluationRegressionCaseResult
            {
                CaseId = CaseId,
                VariantId = VariantId,
                Status = Status,
                Sample = Sample == null ? null : Sample.Clone(),
                ErrorCode = ErrorCode
            };
        }

        public void Validate()
        {
            Require(CaseId, nameof(CaseId), 128);
            Require(VariantId, nameof(VariantId), 128);
            if (ErrorCode != null && ErrorCode.Length > 128)
                throw new ArgumentOutOfRangeException(nameof(ErrorCode));

            if (Status == AiEvaluationRegressionCaseStatus.Completed)
            {
                if (Sample == null)
                    throw new ArgumentNullException(nameof(Sample));
                Sample.Validate();
                if (!string.Equals(Sample.CaseId, CaseId, StringComparison.Ordinal) || !string.Equals(Sample.VariantId, VariantId, StringComparison.Ordinal))
                    throw new ArgumentException("Completed regression sample identity must match its case result.", nameof(Sample));
                if (!string.IsNullOrEmpty(ErrorCode))
                    throw new ArgumentException("A completed regression result cannot contain an error code.", nameof(ErrorCode));
            }
            else
            {
                if (Sample != null)
                    throw new ArgumentException("Failed or canceled regression results cannot contain evaluation samples.", nameof(Sample));
                if (string.IsNullOrWhiteSpace(ErrorCode))
                    throw new ArgumentException("A failed or canceled regression result requires an error code.", nameof(ErrorCode));
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public sealed class AiEvaluationRegressionRun
    {
        public AiEvaluationRegressionRun()
        {
            SuiteId = string.Empty;
            Results = new List<AiEvaluationRegressionCaseResult>();
            Samples = new List<AiEvaluationSample>();
            StartedAt = DateTimeOffset.UtcNow;
            CompletedAt = StartedAt;
        }

        public string SuiteId { get; set; }
        public AiEvaluationRegressionRunStatus Status { get; set; }
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset CompletedAt { get; private set; }
        public IList<AiEvaluationRegressionCaseResult> Results { get; private set; }
        public IList<AiEvaluationSample> Samples { get; private set; }

        internal void Complete(DateTimeOffset completedAt, AiEvaluationRegressionRunStatus status, IEnumerable<AiEvaluationRegressionCaseResult> results)
        {
            Status = status;
            CompletedAt = completedAt;
            Results.Clear();
            Samples.Clear();

            foreach (var result in results.OrderBy(x => x.CaseId, StringComparer.Ordinal).ThenBy(x => x.VariantId, StringComparer.Ordinal))
            {
                var clone = result.Clone();
                clone.Validate();
                Results.Add(clone);
                if (clone.Status == AiEvaluationRegressionCaseStatus.Completed)
                    Samples.Add(clone.Sample.Clone());
            }
        }

        public AiEvaluationAggregationRequest CreateAggregationRequest()
        {
            var request = new AiEvaluationAggregationRequest();
            foreach (var sample in Samples ?? new List<AiEvaluationSample>())
            {
                var clone = sample.Clone();
                clone.Validate();
                request.Samples.Add(clone);
            }
            request.Validate();
            return request;
        }

        public void Validate()
        {
            Require(SuiteId, nameof(SuiteId), 128);
            if (CompletedAt < StartedAt)
                throw new ArgumentException("Regression run completion time cannot precede its start time.", nameof(CompletedAt));
            if (Results == null)
                throw new ArgumentNullException(nameof(Results));
            if (Samples == null)
                throw new ArgumentNullException(nameof(Samples));
            if (Results.Count > 1024 || Samples.Count > 1024)
                throw new ArgumentOutOfRangeException(nameof(Results));

            var completedCount = 0;
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var result in Results)
            {
                if (result == null)
                    throw new ArgumentException("Regression run results cannot contain null entries.", nameof(Results));
                result.Validate();
                var identity = result.CaseId + "\n" + result.VariantId;
                if (!identities.Add(identity))
                    throw new ArgumentException("Regression run cannot contain duplicate case-target results.", nameof(Results));
                if (result.Status == AiEvaluationRegressionCaseStatus.Completed)
                    completedCount++;
            }

            if (completedCount != Samples.Count)
                throw new ArgumentException("Regression run sample count must equal the number of completed case-target results.", nameof(Samples));

            foreach (var sample in Samples)
            {
                if (sample == null)
                    throw new ArgumentException("Regression run samples cannot contain null entries.", nameof(Samples));
                sample.Validate();
            }
        }

        private static void Require(string value, string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            if (value.Length > maxLength)
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public interface IAiEvaluationRegressionExecutor
    {
        Task<AiEvaluationSample> ExecuteAsync(
            AiEvaluationRegressionCase testCase,
            AiEvaluationRegressionTarget target,
            CancellationToken cancellationToken);
    }

    public static class AiEvaluationRegressionRunner
    {
        public static async Task<AiEvaluationRegressionRun> RunAsync(
            AiEvaluationRegressionSuite suite,
            IAiEvaluationRegressionExecutor executor,
            CancellationToken cancellationToken)
        {
            if (suite == null)
                throw new ArgumentNullException(nameof(suite));
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            suite.Validate();
            var snapshot = suite.Clone();
            snapshot.Validate();

            var run = new AiEvaluationRegressionRun { SuiteId = snapshot.Id };
            var results = new List<AiEvaluationRegressionCaseResult>();
            using (var gate = new SemaphoreSlim(snapshot.MaxConcurrency, snapshot.MaxConcurrency))
            {
                var tasks = new List<Task<AiEvaluationRegressionCaseResult>>(snapshot.Cases.Count * snapshot.Targets.Count);
                foreach (var testCase in snapshot.Cases)
                {
                    foreach (var target in snapshot.Targets)
                    {
                        tasks.Add(RunOneAsync(testCase.Clone(), target.Clone(), executor, gate, cancellationToken));
                    }
                }

                var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
                results.AddRange(completed);
            }

            var status = cancellationToken.IsCancellationRequested
                ? AiEvaluationRegressionRunStatus.Canceled
                : results.Any(x => x.Status == AiEvaluationRegressionCaseStatus.Canceled)
                    ? AiEvaluationRegressionRunStatus.Canceled
                    : results.Any(x => x.Status == AiEvaluationRegressionCaseStatus.Failed)
                        ? AiEvaluationRegressionRunStatus.Failed
                        : AiEvaluationRegressionRunStatus.Completed;

            run.Complete(DateTimeOffset.UtcNow, status, results);
            run.Validate();
            return run;
        }

        private static async Task<AiEvaluationRegressionCaseResult> RunOneAsync(
            AiEvaluationRegressionCase testCase,
            AiEvaluationRegressionTarget target,
            IAiEvaluationRegressionExecutor executor,
            SemaphoreSlim gate,
            CancellationToken cancellationToken)
        {
            bool entered = false;
            try
            {
                await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
                entered = true;
                if (cancellationToken.IsCancellationRequested)
                    return Canceled(testCase.Id, target.Id);

                var sample = await executor.ExecuteAsync(testCase.Clone(), target.Clone(), cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    return Canceled(testCase.Id, target.Id);
                if (sample == null)
                    return Failed(testCase.Id, target.Id, "executor-null-sample");

                var detached = sample.Clone();
                detached.Validate();
                if (!string.Equals(detached.CaseId, testCase.Id, StringComparison.Ordinal) || !string.Equals(detached.VariantId, target.Id, StringComparison.Ordinal))
                    return Failed(testCase.Id, target.Id, "sample-identity-mismatch");

                return new AiEvaluationRegressionCaseResult
                {
                    CaseId = testCase.Id,
                    VariantId = target.Id,
                    Status = AiEvaluationRegressionCaseStatus.Completed,
                    Sample = detached
                };
            }
            catch (OperationCanceledException)
            {
                return Canceled(testCase.Id, target.Id);
            }
            catch (ArgumentException)
            {
                return Failed(testCase.Id, target.Id, "invalid-sample");
            }
            catch (Exception)
            {
                return Failed(testCase.Id, target.Id, "executor-failed");
            }
            finally
            {
                if (entered)
                    gate.Release();
            }
        }

        private static AiEvaluationRegressionCaseResult Failed(string caseId, string variantId, string errorCode)
        {
            return new AiEvaluationRegressionCaseResult
            {
                CaseId = caseId,
                VariantId = variantId,
                Status = AiEvaluationRegressionCaseStatus.Failed,
                ErrorCode = errorCode
            };
        }

        private static AiEvaluationRegressionCaseResult Canceled(string caseId, string variantId)
        {
            return new AiEvaluationRegressionCaseResult
            {
                CaseId = caseId,
                VariantId = variantId,
                Status = AiEvaluationRegressionCaseStatus.Canceled,
                ErrorCode = "canceled"
            };
        }
    }
}
