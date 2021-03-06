﻿namespace Cake.Issues.InspectCode.Tests
{
    using System.Linq;
    using Cake.Core.IO;
    using Cake.Issues.Testing;
    using Cake.Testing;
    using Shouldly;
    using Xunit;

    public sealed class InspectCodeIssuesProviderTests
    {
        public sealed class TheCtor
        {
            [Fact]
            public void Should_Throw_If_Log_Is_Null()
            {
                // Given / When
                var result = Record.Exception(() =>
                    new InspectCodeIssuesProvider(
                        null,
                        new InspectCodeIssuesSettings("Foo".ToByteArray())));

                // Then
                result.IsArgumentNullException("log");
            }

            [Fact]
            public void Should_Throw_If_IssueProviderSettings_Are_Null()
            {
                // Given / When
                var result = Record.Exception(() => new InspectCodeIssuesProvider(new FakeLog(), null));

                // Then
                result.IsArgumentNullException("issueProviderSettings");
            }
        }

        public sealed class TheReadIssuesMethod
        {
            [Fact]
            public void Should_Read_Issue_Correct()
            {
                // Given
                var fixture = new InspectCodeIssuesProviderFixture("inspectcode.xml");

                // When
                var issues = fixture.ReadIssues().ToList();

                // Then
                issues.Count.ShouldBe(1);
                var issue = issues.Single();
                CheckIssue(
                    issue,
                    "Cake.Issues",
                    @"src\Cake.Issues\CakeAliasConstants.cs",
                    16,
                    "UnusedMember.Global",
                    null,
                    200,
                    "Suggestion",
                    @"Constant 'PullRequestSystemCakeAliasCategory' is never used");
            }

            [Fact]
            public void Should_Read_Rule_Url()
            {
                // Given
                var fixture = new InspectCodeIssuesProviderFixture("WithWikiUrl.xml");

                // When
                var issues = fixture.ReadIssues().ToList();

                // Then
                issues.Count.ShouldBe(1);
                var issue = issues.Single();
                CheckIssue(
                    issue,
                    "Cake.CodeAnalysisReporting",
                    @"src\Cake.CodeAnalysisReporting\CodeAnalysisReportingAliases.cs",
                    3,
                    "RedundantUsingDirective",
                    "http://www.jetbrains.com/resharperplatform/help?Keyword=RedundantUsingDirective",
                    300,
                    "Warning",
                    @"Using directive is not required by the code and can be safely removed");
            }

            private static void CheckIssue(
                IIssue issue,
                string projectName,
                string affectedFileRelativePath,
                int? line,
                string rule,
                string ruleUrl,
                int priority,
                string priorityName,
                string message)
            {
                issue.ProviderType.ShouldBe("Cake.Issues.InspectCode.InspectCodeIssuesProvider");
                issue.ProviderName.ShouldBe("InspectCode");

                issue.ProjectFileRelativePath.ShouldBe(null);
                issue.ProjectName.ShouldBe(projectName);

                if (issue.AffectedFileRelativePath == null)
                {
                    affectedFileRelativePath.ShouldBeNull();
                }
                else
                {
                    issue.AffectedFileRelativePath.ToString().ShouldBe(new FilePath(affectedFileRelativePath).ToString());
                    issue.AffectedFileRelativePath.IsRelative.ShouldBe(true, "Issue path is not relative");
                }

                issue.Line.ShouldBe(line);
                issue.Rule.ShouldBe(rule);

                if (issue.RuleUrl == null)
                {
                    ruleUrl.ShouldBeNull();
                }
                else
                {
                    issue.RuleUrl.ToString().ShouldBe(ruleUrl);
                }

                issue.Priority.ShouldBe(priority);
                issue.PriorityName.ShouldBe(priorityName);
                issue.Message.ShouldBe(message);
            }
        }
    }
}
