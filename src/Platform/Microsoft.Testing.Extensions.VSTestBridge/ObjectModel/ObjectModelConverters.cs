﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.TestHost;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.Testing.Extensions.VSTestBridge.ObjectModel;

/// <summary>
/// A set of extension methods to convert between the Microsoft Testing Platform and VSTest object models.
/// </summary>
internal static class ObjectModelConverters
{
    private static readonly TestProperty OriginalExecutorUriProperty = TestProperty.Register(
        VSTestTestNodeProperties.OriginalExecutorUriPropertyName, VSTestTestNodeProperties.OriginalExecutorUriPropertyName,
        typeof(Uri), typeof(TestNode));

    /// <summary>
    /// Converts a VSTest <see cref="TestCase"/> to a Microsoft Testing Platform <see cref="TestNode"/>.
    /// </summary>
    public static TestNode ToTestNode(this TestCase testCase, bool isTrxEnabled, IClientInfo client, string? displayNameFromTestResult = null)
    {
        string testNodeUid = testCase.Id.ToString();

        TestNode testNode = new()
        {
            Uid = new TestNodeUid(testNodeUid),
            DisplayName = displayNameFromTestResult ?? testCase.DisplayName ?? testCase.FullyQualifiedName,
        };

        CopyVSTestProperties(testCase.Properties, testNode, testCase, testCase.GetPropertyValue, isTrxEnabled, client);
        if (testCase.CodeFilePath is not null)
        {
            testNode.Properties.Add(new TestFileLocationProperty(testCase.CodeFilePath, new(new(testCase.LineNumber, -1), new(testCase.LineNumber, -1))));
        }

        return testNode;
    }

    private static void CopyVSTestProperties(IEnumerable<TestProperty> testProperties, TestNode testNode, TestCase testCase, Func<TestProperty, object?> getPropertyValue,
        bool isTrxEnabled, IClientInfo client)
    {
        foreach (TestProperty property in testProperties)
        {
            testNode.Properties.Add(new VSTestProperty(property, testCase));

            if (isTrxEnabled)
            {
                // TPv2 is doing some special handling for MSTest... we should probably do the same.
                // See https://github.com/microsoft/vstest/blob/main/src/Microsoft.TestPlatform.Extensions.TrxLogger/Utility/Converter.cs#L66-L70
                if (property.Id == "MSTestDiscoverer.TestCategory"
                    && getPropertyValue(property) is string[] mstestCategories)
                {
                    testNode.Properties.Add(new TrxCategoriesProperty(mstestCategories));
                }
            }

            // Implement handling of specific vstest properties for VS/VS Code Test Explorer,
            // see https://github.com/microsoft/testanywhere/blob/main/docs/design/proposed/IDE_Protocol_IDE_Integration.md#vstest-test-node
            if (client.Id == WellKnownClients.VisualStudio)
            {
                if (property.Id == TestCaseProperties.Id.Id
                        && getPropertyValue(property) is Guid testCaseId)
                {
                    testNode.Properties.Add(new SerializableKeyValuePairStringProperty("vstest.TestCase.Id", testCaseId.ToString()));
                }
                else if (property.Id == TestCaseProperties.FullyQualifiedName.Id
                    && getPropertyValue(property) is string testCaseFqn)
                {
                    testNode.Properties.Add(new SerializableKeyValuePairStringProperty("vstest.TestCase.FullyQualifiedName", testCaseFqn));
                }
                else if (property.Id == OriginalExecutorUriProperty.Id
                    && getPropertyValue(property) is Uri originalExecutorUri)
                {
                    testNode.Properties.Add(new SerializableKeyValuePairStringProperty("vstest.original-executor-uri", originalExecutorUri.AbsoluteUri));
                }

                // The TP object holding the hierarchy property is defined on adapter utilities and we don't want to enforce that dependency
                // so instead I use the string ID copied from TP.
                else if (property.Id == "TestCase.Hierarchy"
                    && getPropertyValue(property) is string[] testCaseHierarchy
                    && testCaseHierarchy.Length == 4)
                {
                    testNode.Properties.Add(new SerializableNamedArrayStringProperty("vstest.TestCase.Hierarchy", testCaseHierarchy));
                }
            }

            // ID is defined on TraitCollection but is internal so again we copy the string here.
            if (property.Id == "TestObject.Traits"
                && getPropertyValue(property) is KeyValuePair<string, string>[] traits && traits.Length > 0)
            {
                foreach (KeyValuePair<string, string> trait in traits)
                {
                    testNode.Properties.Add(new TestMetadataProperty(trait.Key, trait.Value));
                }
            }

            // TPv2 is doing some special handling for MSTest... we should probably do the same.
            // See https://github.com/microsoft/vstest/blob/main/src/Microsoft.TestPlatform.Extensions.TrxLogger/Utility/Converter.cs#L66-L70
            else if (property.Id == "MSTestDiscoverer.TestCategory"
                && getPropertyValue(property) is string[] mstestCategories && mstestCategories.Length > 0)
            {
                foreach (string category in mstestCategories)
                {
                    testNode.Properties.Add(new TestMetadataProperty(category, string.Empty));
                }
            }
        }
    }

    /// <summary>
    /// Converts a VSTest <see cref="TestResult"/> to a Microsoft Testing Platform <see cref="TestNode"/>.
    /// </summary>
    public static TestNode ToTestNode(this TestResult testResult, bool isTrxEnabled, IClientInfo client)
    {
        var testNode = testResult.TestCase.ToTestNode(isTrxEnabled, client, testResult.DisplayName);
        CopyVSTestProperties(testResult.Properties, testNode, testResult.TestCase, testResult.GetPropertyValue, isTrxEnabled, client);
        testNode.AddOutcome(testResult);

        if (isTrxEnabled)
        {
            testNode.Properties.Add(new TrxExceptionProperty(testResult.ErrorMessage, testResult.ErrorStackTrace));

            if (TryParseFullyQualifiedType(testResult.TestCase.FullyQualifiedName, out string? fullyQualifiedType))
            {
                testNode.Properties.Add(new TrxFullyQualifiedTypeNameProperty(fullyQualifiedType));
            }
            else
            {
                throw new InvalidOperationException("Unable to parse fully qualified type name from test case: " + testResult.TestCase.FullyQualifiedName);
            }

            testNode.Properties.Add(new TrxMessagesProperty(testResult.Messages
                .Select(msg =>
                    msg.Category switch
                    {
                        string x when x == TestResultMessage.StandardErrorCategory => new StandardErrorTrxMessage(msg.Text),
                        string x when x == TestResultMessage.StandardOutCategory => new StandardOutputTrxMessage(msg.Text),
                        string x when x == TestResultMessage.DebugTraceCategory => new DebugOrTraceTrxMessage(msg.Text),
                        _ => new TrxMessage(msg.Text),
                    })
                .ToArray()));
        }

        testNode.Properties.Add(new TimingProperty(new(testResult.StartTime, testResult.EndTime, testResult.Duration), []));

        var standardErrorMessages = new List<string>();
        var standardOutputMessages = new List<string>();
        foreach (TestResultMessage testResultMessage in testResult.Messages)
        {
            if (testResultMessage.Category == TestResultMessage.StandardErrorCategory)
            {
                string message = testResultMessage.Text ?? string.Empty;
                testNode.Properties.Add(new SerializableKeyValuePairStringProperty("vstest.TestCase.StandardError", message));
                standardErrorMessages.Add(message);
            }

            if (testResultMessage.Category == TestResultMessage.StandardOutCategory)
            {
                string message = testResultMessage.Text ?? string.Empty;
                testNode.Properties.Add(new SerializableKeyValuePairStringProperty("vstest.TestCase.StandardOutput", message));
                standardOutputMessages.Add(message);
            }
        }

        foreach (AttachmentSet attachmentSet in testResult.Attachments)
        {
            foreach (UriDataAttachment attachment in attachmentSet.Attachments)
            {
                testNode.Properties.Add(new FileArtifactProperty(new FileInfo(attachment.Uri.LocalPath), attachmentSet.DisplayName, attachment.Description));
            }
        }

        if (standardErrorMessages.Count > 0)
        {
            testNode.Properties.Add(new StandardErrorProperty(string.Join(Environment.NewLine, standardErrorMessages)));
        }

        if (standardOutputMessages.Count > 0)
        {
            testNode.Properties.Add(new StandardOutputProperty(string.Join(Environment.NewLine, standardOutputMessages)));
        }

        return testNode;
    }

    private static void AddOutcome(this TestNode testNode, TestResult testResult)
    {
        switch (testResult.Outcome)
        {
            case TestOutcome.Passed:
                testNode.Properties.Add(PassedTestNodeStateProperty.CachedInstance);
                break;

            case TestOutcome.NotFound:
                testNode.Properties.Add(new ErrorTestNodeStateProperty(new VSTestException(testResult.ErrorMessage ?? "Not found", testResult.ErrorStackTrace)));
                break;

            case TestOutcome.Failed:
                testNode.Properties.Add(new FailedTestNodeStateProperty(new VSTestException(testResult.ErrorMessage, testResult.ErrorStackTrace)));
                break;

            // It seems that NUnit inconclusive tests are reported as None which should be considered as Skipped.
            case TestOutcome.None:
            case TestOutcome.Skipped:
                testNode.Properties.Add(testResult.ErrorMessage is null
                    ? SkippedTestNodeStateProperty.CachedInstance
                    : new SkippedTestNodeStateProperty(testResult.ErrorMessage));
                break;

            default:
                throw new NotSupportedException($"Unsupported test outcome value '{testResult.Outcome}'");
        }
    }

    internal static void FixUpTestCase(this TestCase testCase, string? testAssemblyPath = null)
    {
        // To help framework authors using code generator, we replace the Source property of the test case with the
        // test assembly path.
        if (RoslynString.IsNullOrEmpty(testCase.Source) && !RoslynString.IsNullOrEmpty(testAssemblyPath))
        {
            testCase.Source = testAssemblyPath;
        }

        // Because this project is the actually registered test adapter, we need to replace test framework executor
        // URI by ours.
        if (!testCase.Properties.Any(x => x.Id == OriginalExecutorUriProperty.Id))
        {
            testCase.SetPropertyValue(OriginalExecutorUriProperty, testCase.ExecutorUri);
        }

        testCase.ExecutorUri = new(Constants.ExecutorUri);
    }

    private static bool TryParseFullyQualifiedType(string fullyQualifiedName, [NotNullWhen(true)] out string? fullyQualifiedType)
    {
        fullyQualifiedType = null;

        // Some test frameworks display arguments in the fully qualified type name, so we need to exclude them
        // before looking at the last dot.
        int openBracketIndex = fullyQualifiedName.IndexOf('(');
        int lastDotIndexBeforeOpenBracket = openBracketIndex <= 0
            ? fullyQualifiedName.LastIndexOf('.')
            : fullyQualifiedName.LastIndexOf('.', openBracketIndex - 1);
        if (lastDotIndexBeforeOpenBracket <= 0)
        {
            return false;
        }

        fullyQualifiedType = fullyQualifiedName[..lastDotIndexBeforeOpenBracket];
        return true;
    }
}
