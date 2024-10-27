// JCalc
// Sheet management
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

namespace JCalc;

public class Sheet {
    private FileInfo? _fileInfo;

    /// <summary>
    /// Create a new empty sheet not associated with any file.
    /// </summary>
    public Sheet() {
        NewFile = true;
    }

    /// <summary>
    /// Creates a sheet with the specified file.
    /// </summary>
    /// <param name="filename">Name of file</param>
    public Sheet(string filename) {

        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
        }
        else {
            NewFile = true;
        }
        Filename = filename;
    }

    /// <summary>
    /// Return the name of the sheet. This is the base part of the filename if
    /// there is one, or the New File string otherwise.
    /// </summary>
    public string Name => _fileInfo == null ? "New File" : _fileInfo.Name;

    /// <summary>
    /// Return the fully qualified file name with path.
    /// </summary>
    public string Filename {
        get => _fileInfo == null ? string.Empty : _fileInfo.FullName;
        set => _fileInfo = string.IsNullOrEmpty(value) ? null : new FileInfo(value);
    }

    /// <summary>
    /// Whether the sheet has been modified since it was last
    /// changed.
    /// </summary>
    public bool Modified { get; private set; }

    /// <summary>
    /// Is this a new file (i.e. the file has a name but does not
    /// currently exist on disk).
    /// </summary>
    public bool NewFile { get; private set; }

    /// <summary>
    /// Write the sheet back to disk.
    /// </summary>
    public void Write() {
        Modified = false;
        NewFile = false;
    }
}