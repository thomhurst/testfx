﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using FluentAssertions;

using Microsoft.MSTestV2.CLIAutomation;

namespace MSTest.IntegrationTests;

public partial class TestId : CLITestBase
{
    private const string FullyQualifiedStrategyDll = "TestIdProject.FullyQualifiedStrategy";

    public async Task TestIdUniqueness_DataRowArray_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~DataRowArraysTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "DataRowArraysTests (0,[])",
            "DataRowArraysTests (0,[0])",
            "DataRowArraysTests (0,[0,0,0])");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_DataRowString_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~DataRowStringTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "DataRowStringTests (null)",
            "DataRowStringTests (\"\")",
            "DataRowStringTests (\" \")",
            "DataRowStringTests (\"  \")");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_DynamicDataArrays_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~DynamicDataArraysTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "DynamicDataArraysTests (0,[])",
            "DynamicDataArraysTests (0,[0])",
            "DynamicDataArraysTests (0,[0,0,0])");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_DynamicDataTuple_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~DynamicDataTuplesTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "DynamicDataTuplesTests ((1, text, True))",
            "DynamicDataTuplesTests ((1, text, False))");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_DynamicDataGenericCollections_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~DynamicDataGenericCollectionsTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "DynamicDataGenericCollectionsTests (System.Collections.Generic.List`1[System.Int32],System.Collections.Generic.List`1[System.String],System.Collections.Generic.List`1[System.Boolean])",
            "DynamicDataGenericCollectionsTests (System.Collections.Generic.List`1[System.Int32],System.Collections.Generic.List`1[System.String],System.Collections.Generic.List`1[System.Boolean])",
            "DynamicDataGenericCollectionsTests (System.Collections.Generic.List`1[System.Int32],System.Collections.Generic.List`1[System.String],System.Collections.Generic.List`1[System.Boolean])",
            "DynamicDataGenericCollectionsTests (System.Collections.Generic.List`1[System.Int32],System.Collections.Generic.List`1[System.String],System.Collections.Generic.List`1[System.Boolean])");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_TestDataSourceArrays_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~TestDataSourceArraysTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "Custom name",
            "Custom name",
            "Custom name");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_TestDataSourceTuples_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~TestDataSourceTuplesTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "Custom name",
            "Custom name");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }

    public async Task TestIdUniqueness_TestDataSourceGenericCollections_FullyQualifiedStrategy()
    {
        // Arrange
        string assemblyPath = GetAssetFullPath(FullyQualifiedStrategyDll);

        // Act
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase> testCases = DiscoverTests(assemblyPath, "FullyQualifiedName~TestDataSourceGenericCollectionsTests");
        ImmutableArray<Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult> testResults = await RunTestsAsync(testCases);

        // Assert
        VerifyE2E.FailedTestCount(testResults, 0);
        VerifyE2E.TestsPassed(
            testResults,
            "Custom name",
            "Custom name",
            "Custom name",
            "Custom name");

        // We cannot assert the expected ID as it is path dependent
        testResults.Select(x => x.TestCase.Id.ToString()).Should().OnlyHaveUniqueItems();
    }
}
