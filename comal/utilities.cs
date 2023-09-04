// JComal
// Helper functions
//
// Authors:
//  Steven Palmer
//
// Copyright (C) 2021 Steven Palmer
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

namespace JComal; 

public static class Utilities {

    /// <summary>
    /// Add the specified extension to the filename is no extension was
    /// already supplied.
    /// </summary>
    /// <param name="filename">Filename</param>
    /// <param name="extension">Extension to supply</param>
    /// <returns>Filename with an extension</returns>
    public static string AddExtensionIfMissing(string filename, string extension) {
        if (string.IsNullOrEmpty(Path.GetExtension(filename))) {
            return Path.ChangeExtension(filename, extension);
        }
        return filename;
    }
}
