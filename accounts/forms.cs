// Accounts
// Forms management
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

using JComLib;

namespace JAccounts;

public enum TDisplayFormResult {

    /// <summary>
    /// Use pressed the Enter key
    /// </summary>
    Pick,

    /// <summary>
    /// User pressed Cancel
    /// </summary>
    Cancel,

    /// <summary>
    /// User pressed F10 to save the form
    /// </summary>
    Save,

    /// <summary>
    /// User pressed F2 to insert a new row
    /// </summary>
    Insert,

    /// <summary>
    /// User pressed F4 to delete a row
    /// </summary>
    Deleted
}

public class TForm {

    // Form editing capability
    // Simple means no editing capability
    public const int Simple = 1;
    public const int CanPrint = 2;

    public const int FirstRow = 3;
    private readonly int _caps;
    private readonly List<TField> _fields;
    private int _scrollOffset;

    /// <summary>
    /// Constructor for a TForm with capabilities.
    /// </summary>
    public TForm(int caps) {
        SelectedItem = 1;
        IsModified = false;
        _fields = [];
        _scrollOffset = 0;
        _caps = caps;
    }

    /// <summary>
    /// Item selected in form when form is closed.
    /// </summary>
    public int SelectedItem { get; set; }

    /// <summary>
    /// Get or set whether the form has been edited
    /// </summary>
    public bool IsModified { get; private set; }

    /// <summary>
    /// Return count of fields on the form
    /// </summary>
    public int Count => _fields.Count;

    /// <summary>
    /// Get a single field.
    /// </summary>
    public TField Fields(int index) => _fields[index];

    /// <summary>
    /// Add a named section to the form
    /// </summary>
    public void BeginSection(string sectionName) {
        Add(new TField(0, 0, 0, sectionName, TFieldType.BeginSection));
    }

    /// <summary>
    /// Mark the end of the named section
    /// </summary>
    public void EndSection(string sectionName) {
        Add(new TField(0, 0, 0, sectionName, TFieldType.EndSection));
    }

    /// <summary>
    /// Add a static label field to the form
    /// </summary>
    public void AddLabel(int theRow, int theColumn, string theLabel) {
        Add(new TField(theRow, theColumn, theLabel.Length, theLabel, TFieldType.Label));
    }

    /// <summary>
    /// Add a static label field to the form with a width and alignment
    /// </summary>
    public void AddLabel(int theRow, int theColumn, int theWidth, TAlign theAlign, string theLabel) {
        Add(new TField(theRow, theColumn, theWidth, theAlign, theLabel, TFieldType.Label));
    }

    /// <summary>
    /// Add a text option field to the form
    /// </summary>
    public void AddOption(int theRow, int theColumn, string theLabel, object theData) {
        TField newField = new(theRow, theColumn, theLabel.Length, theLabel, TFieldType.Option) {
            Data = theData
        };
        Add(newField);
    }

    /// <summary>
    /// Add a text option field to the form
    /// </summary>
    public void AddOption(int theRow, int theColumn, string theLabel, char theCh) {
        TField newField = new(theRow, theColumn, theLabel.Length, theLabel, TFieldType.Option) {
            Ch = theCh
        };
        Add(newField);
    }

    /// <summary>
    /// Add a text field to the form
    /// </summary>
    public void AddText(int theRow, int theColumn, int theWidth, string theLabel) {
        Add(new TField(theRow, theColumn, theWidth, theLabel, TFieldType.Text));
    }

    /// <summary>
    /// Add a currency field to the form
    /// </summary>
    public void AddCurrency(int theRow, int theColumn, double theValue) {
        Add(new TField(theRow, theColumn, 10, TAlign.Right, theValue.ToString("F2"), TFieldType.Currency));
    }

    /// <summary>
    /// Add a numeric input field to the form
    /// </summary>
    public void AddNumeric(int theRow, int theColumn, int theWidth, int theValue, string theFormat) {
        Add(new TField(theRow, theColumn, theWidth, theValue.ToString(), theFormat, TFieldType.Numeric));
    }

    /// <summary>
    /// Insert a text field into the form
    /// </summary>
    public void InsertText(int insertIndex, int theRow, int theColumn, int theWidth, string theLabel) {
        Insert(insertIndex, new TField(theRow, theColumn, theWidth, theLabel, TFieldType.Text));
    }

    /// <summary>
    /// Insert a currency field into the form
    /// </summary>
    public void InsertCurrency(int insertIndex, int theRow, int theColumn, double theValue) {
        Insert(insertIndex, new TField(theRow, theColumn, 10, TAlign.Right, theValue.ToString("F2"), TFieldType.Currency));
    }

    /// <summary>
    /// Insert a numeric input field into the form
    /// </summary>
    public void InsertNumeric(int insertIndex, int theRow, int theColumn, int theWidth, int theValue,
        string theFormat) {
        Insert(insertIndex, new TField(theRow, theColumn, theWidth, theValue.ToString(), theFormat, TFieldType.Numeric));
    }

    /// <summary>
    /// Clear the form
    /// </summary>
    public void Clear() {
        _fields.Clear();
    }

    /// <summary>
    /// Add a field to the array of fields on the form.
    /// </summary>
    private void Add(TField item) {
        _fields.Add(item);
    }

    /// <summary>
    /// Insert a field at the specified index on the form.
    /// </summary>
    private void Insert(int insertIndex, TField item) {
        if (insertIndex < 0) {
            insertIndex = 0;
        }
        if (insertIndex > _fields.Count) {
            insertIndex = _fields.Count;
        }
        _fields.Insert(insertIndex, item);
    }

    /// <summary>
    /// Delete a field at the specified index
    /// </summary>
    public void Delete(int deleteIndex) {
        if (deleteIndex < 0) {
            deleteIndex = 0;
        }
        if (deleteIndex >= _fields.Count) {
            deleteIndex = _fields.Count - 1;
        }
        _fields.RemoveAt(deleteIndex);
    }

    /// <summary>
    /// Find a section by name and return the index of the field
    /// immediately after that section
    /// </summary>
    public int FindSection(string sectionName) {
        for (int index = 0; index < _fields.Count; index++) {
            TField field = _fields[index];
            if (field.FieldType == TFieldType.BeginSection &&
                field.Value == sectionName) {
                return index + 1;
            }
        }
        return 0;
    }

    /// <summary>
    /// Draw the form at the current scroll offset
    /// </summary>
    private void Draw() {
        int pageHeight = Terminal.Height - 3;
        int pageWidth = Terminal.Width;
        bool didShowCursor = false;

        int firstRow = _fields[0].Row;
        if (firstRow < FirstRow) {
            firstRow = FirstRow;
        }
        Utils.ScreenClear(firstRow, 0, pageHeight, pageWidth);
        for (int itemIndex = 0; itemIndex < _fields.Count; itemIndex++) {
            bool isSelected = itemIndex == SelectedItem;
            if (_fields[itemIndex].Row + _scrollOffset >= FirstRow && _fields[itemIndex].Row + _scrollOffset <= pageHeight) {
                if (!_fields[itemIndex].IsSection) {
                    bool showCursor = false;
                    if (_fields[itemIndex].IsEditable && isSelected && !didShowCursor) {
                        showCursor = true;
                        didShowCursor = true;
                    }
                    _fields[itemIndex].FormattedDraw(_scrollOffset, isSelected, showCursor);
                }
            }
        }
    }

    /// <summary>
    /// Display the form and allow user navigation over the fields
    /// using the cursor keys. Returns the index of the selected field when
    /// the Enter key is pressed.
    /// </summary>
    public TDisplayFormResult DisplayForm() {
        int pageHeight = Terminal.Height - 3;

        // Adjust selection if necessary
        while (SelectedItem < _fields.Count && !_fields[SelectedItem].IsSelectable) {
            ++SelectedItem;
        }

        // If no selection, change form behaviour
        bool selectable = SelectedItem < _fields.Count;
        string footer = "";
        if ((_caps & Simple) == 0) {
            footer = "F2 - Insert row | F4 - Delete row | ";
        }
        if ((_caps & CanPrint) == CanPrint) {
            footer += "F6 - Print | ";
        }
        if ((_caps & Simple) == 0) {
            footer += "F10 - Save and Exit | ";
        }
        footer += "Esc - Exit";
        Utils.ShowFooter(footer);

        // Set the initial scroll offset so that the selected item is visible
        int diff = _fields[^1].Row - pageHeight;
        if (selectable && _fields[SelectedItem].Row > pageHeight && diff > 0) {
            _scrollOffset = -diff;
        }
        Draw();

        // Show the cursor if we're editing
        Console.CursorVisible = (_caps & Simple) != Simple;

        ConsoleKey lastKey;
        do {
            int previousSelectedIndex = SelectedItem;
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            lastKey = keyInfo.Key;
            bool indexChanged;
            switch (lastKey) {
                case ConsoleKey.Escape:
                    return TDisplayFormResult.Cancel;

                case ConsoleKey.UpArrow:
                    indexChanged = false;
                    if (selectable) {
                        int currentRow = _fields[SelectedItem].Row;
                        int currentColumn = _fields[SelectedItem].Column;
                        int newIndex = SelectedItem;
                        while (newIndex > 0) {
                            newIndex -= 1;
                            if (_fields[newIndex].Row < currentRow && _fields[newIndex].Column <= currentColumn) {
                                if (_fields[newIndex].IsSelectable) {
                                    SelectedItem = newIndex;
                                    indexChanged = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!indexChanged && _fields[0].Row + _scrollOffset < FirstRow) {
                        _scrollOffset += 1;
                        Draw();
                    }
                    break;

                case ConsoleKey.DownArrow:
                    indexChanged = false;
                    if (selectable) {
                        int currentRow = _fields[SelectedItem].Row;
                        int currentColumn = _fields[SelectedItem].Column;
                        int newIndex = SelectedItem;
                        while (newIndex < _fields.Count - 1) {
                            newIndex += 1;
                            if (_fields[newIndex].Row > currentRow && _fields[newIndex].Column >= currentColumn) {
                                if (_fields[newIndex].IsSelectable) {
                                    SelectedItem = newIndex;
                                    indexChanged = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!indexChanged && _fields[^1].Row + _scrollOffset >= pageHeight) {
                        _scrollOffset -= 1;
                        Draw();
                    }
                    break;

                case ConsoleKey.Tab:
                case ConsoleKey.RightArrow:
                    if (selectable && SelectedItem < _fields.Count - 1) {
                        int newIndex = SelectedItem;
                        while (newIndex < _fields.Count - 1) {
                            newIndex += 1;
                            if (_fields[newIndex].IsSelectable) {
                                SelectedItem = newIndex;
                                break;
                            }
                        }
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (selectable && SelectedItem > 0) {
                        int newIndex = SelectedItem;
                        while (newIndex > 0) {
                            newIndex -= 1;
                            if (_fields[newIndex].IsSelectable) {
                                SelectedItem = newIndex;
                                break;
                            }
                        }
                    }
                    break;

                case ConsoleKey.F2:
                    if ((_caps & Simple) == 0) {
                        return TDisplayFormResult.Insert;
                    }
                    break;

                case ConsoleKey.F4:
                    if ((_caps & Simple) == 0) {
                        return TDisplayFormResult.Deleted;
                    }
                    break;

                case ConsoleKey.F10:
                    if ((_caps & Simple) == 0) {
                        return TDisplayFormResult.Save;
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (selectable) {
                        if (_fields[SelectedItem].IsSelectable) {
                            string value = _fields[SelectedItem].Value;
                            if (value.Length > 0) {
                                value = value[..^1];
                                _fields[SelectedItem].Value = value;
                                _fields[SelectedItem].State = TFieldState.Modified;
                                _fields[SelectedItem].Draw(value, _scrollOffset, true, true);
                                IsModified = true;
                            }
                        }
                    }
                    break;

                default:
                    if (selectable) {
                        if (_fields[SelectedItem].IsEditable) {
                            if (_fields[SelectedItem].FieldType == TFieldType.Currency) {
                                DoCurrencyFieldChar(keyInfo.KeyChar);
                            }
                            if (_fields[SelectedItem].FieldType == TFieldType.Text) {
                                DoTextFieldChar(keyInfo.KeyChar);
                            }
                            if (_fields[SelectedItem].FieldType == TFieldType.Numeric) {
                                DoNumericFieldChar(keyInfo.KeyChar);
                            }
                        }
                        else {
                            for (int optIndex = 0; optIndex < _fields.Count; optIndex++) {
                                if (char.ToUpper(keyInfo.KeyChar) == char.ToUpper(_fields[optIndex].Ch)) {
                                    SelectedItem = optIndex;
                                    lastKey = ConsoleKey.Enter;
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }
            if (selectable) {
                if (SelectedItem != previousSelectedIndex) {
                    bool showCursor = _fields[SelectedItem].IsEditable;
                    int diff2 = _fields[SelectedItem].Row + _scrollOffset;

                    // Do we need to scroll?
                    if (diff2 < FirstRow || diff2 > pageHeight) {
                        if (_fields[^1].Row + _scrollOffset > pageHeight) {
                            _scrollOffset = pageHeight - _fields[^1].Row;
                            Draw();
                        }
                        else if (_fields[0].Row + _scrollOffset < FirstRow) {
                            _scrollOffset = FirstRow - _fields[0].Row;
                            Draw();
                        }
                    }
                    if (_fields[previousSelectedIndex].IsEditable) {
                        _fields[previousSelectedIndex].State = TFieldState.Original;
                    }
                    _fields[previousSelectedIndex].FormattedDraw(_scrollOffset, false, false);
                    if (_fields[SelectedItem].FieldType == TFieldType.Currency) {
                        double value = Convert.ToDouble(_fields[SelectedItem].Value);
                        _fields[SelectedItem].Value = value.ToString("F2");
                    }
                    _fields[SelectedItem].FormattedDraw(_scrollOffset, true, showCursor);
                }
            }
        } while (lastKey != ConsoleKey.Enter);

        // Redraw the selected cell before we exit
        _fields[SelectedItem].FormattedDraw(_scrollOffset, true, false);
        return TDisplayFormResult.Pick;
    }

    /// <summary>
    /// Handle editing in a currency field.
    /// </summary>
    private void DoCurrencyFieldChar(char theChar) {
        if (theChar is >= '0' and <= '9' or '.' or '-' or '+') {
            bool canEdit = true;
            string value = _fields[SelectedItem].Value;
            if (_fields[SelectedItem].State == TFieldState.Original) {
                value = "";
            }
            int decimalPosition = value.IndexOf('.');
            if (value.Length == _fields[SelectedItem].Width) {
                canEdit = false;
            }
            if (theChar == '.' && (decimalPosition >= 0 || value.Length == 0)) {
                canEdit = false;
            }
            if (theChar == '-' && value != "") {
                canEdit = false;
            }
            if (theChar == '+' && value != "") {
                canEdit = false;
            }
            if (theChar is >= '0' and <= '9' && decimalPosition >= 0 && value[decimalPosition..].Length == 3) {
                canEdit = false;
            }
            if (canEdit) {
                value += theChar;
                _fields[SelectedItem].Value = value;
                _fields[SelectedItem].State = TFieldState.Modified;
                _fields[SelectedItem].Draw(value, _scrollOffset, true, true);
                IsModified = true;
            }
        }
    }

    /// <summary>
    /// Handle editing in a text field.
    /// </summary>
    private void DoTextFieldChar(char theChar) {
        if (theChar is >= ' ' and <= 'z') {
            string value = _fields[SelectedItem].Value;
            if (_fields[SelectedItem].State == TFieldState.Original) {
                value = "";
            }
            if (value.Length < _fields[SelectedItem].Width) {
                value += theChar;
                _fields[SelectedItem].Value = value;
                _fields[SelectedItem].State = TFieldState.Modified;
                _fields[SelectedItem].Draw(value, _scrollOffset, true, true);
                IsModified = true;
            }
        }
    }

    /// <summary>
    /// Handle editing in a numeric field.
    /// </summary>
    private void DoNumericFieldChar(char theChar) {
        if (theChar is '-' or >= '0' and <= '9') {
            string value = _fields[SelectedItem].Value;
            if (_fields[SelectedItem].State == TFieldState.Original) {
                value = "";
            }
            if (value.Length < _fields[SelectedItem].Width) {
                value += theChar;
                _fields[SelectedItem].Value = value;
                _fields[SelectedItem].State = TFieldState.Modified;
                _fields[SelectedItem].Draw(value, _scrollOffset, true, true);
                IsModified = true;
            }
        }
    }
}