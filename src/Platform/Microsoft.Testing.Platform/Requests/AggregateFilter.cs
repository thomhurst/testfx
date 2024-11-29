// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Platform.Requests;

public sealed class AggregateFilter(params IReadOnlyList<ITestExecutionFilter> filters) : ITestExecutionFilter
{
    public IReadOnlyList<ITestExecutionFilter> Filters { get; } = filters;

    public bool ShouldUse => true;
}
