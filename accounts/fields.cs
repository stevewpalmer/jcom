// Accounts
// Fields management.
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

using System.Drawing;
using JComLib;

namespace JAccounts;

public enum TFieldType {

    /// <summary>
    /// Text field
    /// </summary>
    Text,

    /// <summary>
    /// Numeric (non-currency) field
    /// </summary>
    Numeric,

    /// <summary>
    /// Non-editable label field
    /// </summary>
    Label,

    /// <summary>
    /// Option field
    /// </summary>
    Option,

    /// <summary>
    /// Currency field
    /// </summary>
    Currency,

    /// <summary>
    /// Marks the start of a named section
    /// </summary>
    BeginSection,

    /// <summary>
    /// Marks the end of the current section
    /// </summary>
    EndSection
}

public enum TFieldState {

    /// <summary>
    /// Field contains its original value
    /// </summary>
    Original,

    /// <summary>
    /// Field has been modified
    /// </summary>
    Modified
}

public enum TAlign {

    /// <summary>
    /// Left align value in field
    /// </summary>
    Left,

    /// <summary>
    /// Right align value in field
    /// </summary>
    Right
}

public class TField {

    /// <summary>
    /// Get or set the Row property
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Get or set the Column property
    /// </summary>
    public int Column { get; private set; }

    /// <summary>
    /// Get or set the Width property
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Get or set the Align property
    /// </summary>
    private TAlign Align { get; set; }

    /// <summary>
    /// Get or set the field type property
    /// </summary>
    public TFieldType FieldType { get; private set; }

    /// <summary>
    /// Get or set the option character.
    /// </summary>
    public char Ch { get; init; }

    /// <summary>
    /// Set or get the field state
    /// </summary>
    public TFieldState State { get; set; }

    /// <summary>
    /// Get or set the field value
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Get or set the field data property
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Get or set the format string
    /// </summary>
    private string Format { get; set; } = "";

    /// <summary>
    /// Return whether this field is a modifiable type field
    /// where the user can edit the contents
    /// </summary>
    public bool IsEditable => FieldType is TFieldType.Currency or TFieldType.Text or TFieldType.Numeric;

    /// <summary>
    /// Return whether this field is a selectable type.
    /// </summary>
    public bool IsSelectable => IsEditable || FieldType == TFieldType.Option;

    /// <summary>
    /// Return whether this field is a section marker
    /// </summary>
    public bool IsSection => FieldType is TFieldType.BeginSection or TFieldType.EndSection;

    /// <summary>
    /// Constructor for a TField initialised with the specified
    /// field data and type.
    /// </summary>
    public TField(int theRow, int theColumn, int theWidth, string theValue, TFieldType theType) {
        Init(theRow, theColumn, theWidth, TAlign.Left, theValue, "", theType);
    }

    /// <summary>
    /// Constructor for a TField initialised with the specified
    /// field data, format and type.
    /// </summary>
    public TField(int theRow, int theColumn, int theWidth, string theValue, string theFormat, TFieldType theType) {
        Init(theRow, theColumn, theWidth, TAlign.Left, theValue, theFormat, theType);
    }

    /// <summary>
    /// Constructor for a TField initialised with the specified
    /// field alignment, data and type.
    /// </summary>
    public TField(int theRow, int theColumn, int theWidth, TAlign theAlign, string theValue, TFieldType theType) {
        Init(theRow, theColumn, theWidth, theAlign, theValue, "", theType);
    }

    /// <summary>
    /// Common initialisation for a TField.
    /// </summary>
    private void Init(int theRow, int theColumn, int theWidth, TAlign theAlign, string theValue, string theFormat,
        TFieldType theType) {
        Row = theRow;
        Column = theColumn;
        Width = theWidth;
        Value = theValue;
        FieldType = theType;
        State = TFieldState.Original;
        Data = null;
        Align = theAlign;
        Format = theFormat;
    }

    /// <summary>
    /// Do a formatted draw of a single field in either selected
    /// or unselected style as specified
    /// </summary>
    public void FormattedDraw(int offset, bool isSelected, bool showCursor) {
        string text = Value;
        if (FieldType == TFieldType.Currency) {
            double thisValue = Convert.ToDouble(Value);
            Value = thisValue.ToString("F2");
        }
        if (!string.IsNullOrEmpty(Format)) {
            text = string.Format(Format, Value);
        }
        Draw(text, offset, isSelected, showCursor);
    }

    /// <summary>
    /// Draw a single field in either selected or unselected
    /// style as specified
    /// </summary>
    public void Draw(string formattedText, int offset, bool isSelected, bool showCursor) {
        ConsoleColor fgColour = Utils.ForegroundColour;
        Point savedCursor = Terminal.GetCursor();
        Point newCursor = new(Column, Row + offset);

        if (formattedText.StartsWith('_')) {
            formattedText = formattedText[1..];
            fgColour = Utils.TitleColour;
        }
        formattedText = $" {formattedText} ";
        if (isSelected) {
            Console.ForegroundColor = Utils.BackgroundColour;
            Console.BackgroundColor = Utils.ReverseColour;
        }
        else {
            Console.ForegroundColor = fgColour;
            Console.BackgroundColor = Utils.BackgroundColour;
        }
        Terminal.SetCursor(newCursor);
        switch (Align) {
            case TAlign.Right:
                Terminal.Write(formattedText.PadLeft(Width + 2));
                newCursor.X = Column + Width + 1;
                break;

            case TAlign.Left:
                Terminal.Write(formattedText.PadRight(Width + 2));
                newCursor.X = Column + Value.Length + 1;
                break;
        }
        Terminal.SetCursor(showCursor ? newCursor : savedCursor);
    }
}