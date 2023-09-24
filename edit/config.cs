// JEdit
// Configuration file
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

using System.Text.Json;
using JEdit.Resources;

namespace JEdit;

/// <summary>
/// Configuration file
/// </summary>
public class Config {

    /// <summary>
    /// Load a config file.
    /// </summary>
    public static Config Load() {
        Config fileConfig = new Config();
        if (File.Exists(Consts.ConfigurationFilename)) {
            try {
                using FileStream stream = File.Create(Consts.ConfigurationFilename);
                fileConfig = JsonSerializer.Deserialize<Config>(stream);
            }
            catch (Exception) {
                Screen.StatusBar.Error(string.Format(Edit.UnableToReadConfig, Consts.ConfigurationFilename));
            }
        }
        return fileConfig;
    }

    /// <summary>
    /// Save configurations back to the config file.
    /// </summary>
    public void Save() {
        try {
            using FileStream stream = File.OpenWrite(Consts.ConfigurationFilename);
            JsonSerializer.Serialize(stream, this, new JsonSerializerOptions {
                WriteIndented = true
            });
        }
        catch (Exception) {
            Screen.StatusBar.Error(string.Format(Edit.UnableToSaveConfig, Consts.ConfigurationFilename));
        }
    }

    /// <summary>
    /// Background colour
    /// </summary>
    public string BackgroundColour { get; set; }

    /// <summary>
    /// Foreground colour
    /// </summary>
    public string ForegroundColour { get; set; }

    /// <summary>
    /// Title of selected window colour
    /// </summary>
    public string SelectedTitleColour { get; set; }

    /// <summary>
    /// Normal status bar message colour
    /// </summary>
    public string NormalMessageColour { get; set; }

    /// <summary>
    /// Status bar error message colour
    /// </summary>
    public string ErrorMessageColour { get; set; }
}