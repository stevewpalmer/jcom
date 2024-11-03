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

/// <summary>
/// List of command IDs
/// </summary>
public enum KeyCommand {
    KC_NONE,
    KC_EDIT,
    KC_GRAPH,
    KC_FILE,
    KC_COPY,
    KC_MOVE,
    KC_PRINT,
    KC_QUIT,
    KC_GLOBAL,
    KC_INSERT,
    KC_DELETE,
    KC_ERASE,
    KC_TITLES,
    KC_WINDOW,
    KC_STATUS,
    KC_VALUE,
    KC_LEFT,
    KC_RIGHT,
    KC_UP,
    KC_DOWN,
    KC_HOME,
    KC_PAGEUP,
    KC_PAGEDOWN,
    KC_GOTO,
    KC_NAME,
    KC_JUSTIFY,
    KC_PROTECT,
    KC_UNPROTECT,
    KC_INPUT,
    KC_RETRIEVE,
    KC_SAVE,
    KC_COMBINE,
    KC_XTRACT,
    KC_LIST,
    KC_IMPORT,
    KC_DIRECTORY,
    KC_FILL,
    KC_TABLE,
    KC_SORT,
    KC_QUERY,
    KC_DISTRIBUTION,
    KC_SET_COLUMN_WIDTH,
    KC_RESET_COLUMN_WIDTH,
    KC_COMMAND_BAR,
    KC_ALIGN_LEFT,
    KC_ALIGN_RIGHT,
    KC_ALIGN_CENTRE,
    KC_DATE_DMY,
    KC_DATE_DM,
    KC_DATE_MY,
    KC_FORMAT_FIXED,
    KC_FORMAT_SCI,
    KC_FORMAT_CURRENCY,
    KC_FORMAT_COMMAS,
    KC_FORMAT_GENERAL,
    KC_FORMAT_BAR,
    KC_FORMAT_PERCENT,
    KC_FORMAT_TEXT,
    KC_FORMAT_RESET
}