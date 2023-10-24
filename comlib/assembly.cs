// Jcom Runtime Libary
// Assembly Support
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2023 Steven Palmer
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

using System.Diagnostics;
using System.Reflection;

namespace JComLib;

public static class AssemblySupport {

    /// <summary>
    /// Return the assembly copyright
    /// </summary>
    public static string AssemblyCopyright {
        get {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            Debug.Assert(attributes.Length > 0);
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    /// <summary>
    /// Return the assembly description
    /// </summary>
    public static string AssemblyDescription {
        get {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            Debug.Assert(attributes.Length > 0);
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    /// <summary>
    /// Return the assembly version.
    /// </summary>
    public static string AssemblyVersion {
        get {
            Version ver = Assembly.GetEntryAssembly().GetName().Version;
            return $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }
    }

    /// <summary>
    /// Return this executable filename.
    /// </summary>
    /// <returns>Executable filename string</returns>
    public static string ExecutableFilename() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
}