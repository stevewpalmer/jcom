// JEdit
// Buffer management
//
// Authors:
//  Steve Palmer
//
// Copyright (C) 2023 Steve Palmer
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

namespace JEdit; 

public class Buffer {

    /// <summary>
    /// Creates a buffer with the specified file.
    /// </summary>
    /// <param name="filename">Name of file</param>
    public Buffer(string filename) {

        if (!string.IsNullOrEmpty(filename) && File.Exists(filename)) {
            Lines = File.ReadAllLines(filename).ToList();
        } else {
            Lines = new List<string> {""};
        }
        Filename = filename ?? string.Empty;
    }

    /// <summary>
    /// Buffer contents
    /// </summary>
    private List<string> Lines { get; set; }

    /// <summary>
    /// Name of file associated with buffer, or empty string
    /// if the buffer has not yet been saved to a file
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// Whether the buffer has been modified since it was last
    /// changed.
    /// </summary>
    public bool Modified { get; set; }

    /// <summary>
    /// Number of lines in the buffer
    /// </summary>
    public int Length => Lines.Count;

    /// <summary>
    /// Index of line being edited
    /// </summary>
    public int LineIndex { get; set; }

    /// <summary>
    /// Offset on line being edited
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Retrieve the line at the specified index.
    /// </summary>
    /// <param name="index">Zero based index</param>
    /// <returns>Line, or null if the index is out of range</returns>
    public string GetLine(int index) {
        return index >= 0 && index < Lines.Count ? Lines[index] : null;
    }

    /// <summary>
    /// Write the buffer back to disk.
    /// </summary>
    public void Write() {
        
    }
}

