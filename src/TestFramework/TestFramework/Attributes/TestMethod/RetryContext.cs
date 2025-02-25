﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestTools.UnitTesting;

public readonly struct RetryContext
{
    internal RetryContext(Func<Task<TestResult[]>> executeTaskGetter)
        => ExecuteTaskGetter = executeTaskGetter;

    public Func<Task<TestResult[]>> ExecuteTaskGetter { get; }
}
