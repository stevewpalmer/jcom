// JCalc
// A single spreadsheet cell
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

namespace JCalc;

public class Cell {

    /// <summary>
    /// Cell value
    /// </summary>
    public CellValue Value { get; set; } = new CellValue();

    /// <summary>
    /// Cell row
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Cell column
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Draw this cell at the given cell position (1-based row and column) at
    /// the given physical screen offset where (0,0) is the top left corner.
    /// </summary>
    /// <param name="sheet">Sheet to which cell belongs</param>
    /// <param name="x">X position of cell</param>
    /// <param name="y">Y position of cell</param>
    public void Draw(Sheet sheet, int x, int y) {
        Terminal.SetCursor(x, y);
        string cellValue = "";
        if (Value.Type != CellType.NONE) {
            cellValue = Value.StringValue.ToString();
        }
        int width = sheet.ColumnWidth(Column);
        Terminal.Write(Utilities.SpanBound(cellValue, 0, width).PadRight(width));
    }
}