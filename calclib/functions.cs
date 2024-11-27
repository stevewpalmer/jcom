// JCalcLib
// Implementation of functions
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2024 Steve Palmer
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
// # http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using JComLib;

namespace JCalcLib;

// ReSharper disable UnusedMember.Global

public static class Functions {

    /// <summary>
    /// Calculate the sum of all cells and constants in the argument list.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the sum of the arguments</returns>
    public static Variant SUM(IEnumerable<Variant> arguments) {
        Variant sumTotal = new Variant(0);
        return arguments.Aggregate(sumTotal, (current, value) => current + value);
    }

    /// <summary>
    /// Returns the current date and time as a serial number.
    /// </summary>
    /// <param name="_">Function parameters</param>
    /// <returns>A variant containing the serial number of the current date and time</returns>
    public static Variant NOW(IEnumerable<Variant> _) {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Returns the current date as a serial number.
    /// </summary>
    /// <param name="_">Function parameters</param>
    /// <returns>A variant containing the serial number of the current date</returns>
    public static Variant TODAY(IEnumerable<Variant> _) {
        return new Variant(DateTime.Now.ToOADate());
    }

    /// <summary>
    /// Extract and return the year part of a date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A vatiant containing the year part of a date</returns>
    public static Variant YEAR(IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Year);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Extract and return the month part of a date.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the month value of a date</returns>
    public static Variant MONTH(IEnumerable<Variant> arguments) {
        Variant result = arguments.First();
        try {
            DateTime date = DateTime.FromOADate(result.DoubleValue);
            return new Variant(date.Month);
        }
        catch {
            return new Variant(0);
        }
    }

    /// <summary>
    /// Concatenate the result of all arguments into a single text string.
    /// </summary>
    /// <param name="arguments">Function parameters</param>
    /// <returns>A variant containing the result of the concatenation</returns>
    public static Variant CONCATENATE(IEnumerable<Variant> arguments) {
        return new Variant(string.Concat(arguments));
    }
}