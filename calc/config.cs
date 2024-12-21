// JCalc
// Configuration file
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

using System.Text.Json;
using System.Text.Json.Serialization;
using JCalc.Resources;
using JCalcLib;
using JComLib;

namespace JCalc;

/// <summary>
/// Configuration file
/// </summary>
public class Config {

    /// <summary>
    /// Background colour
    /// </summary>
    public int? BackgroundColour { get; set; }

    /// <summary>
    /// Foreground colour
    /// </summary>
    public int? ForegroundColour { get; set; }

    /// <summary>
    /// Normal status bar message colour
    /// </summary>
    public int? NormalMessageColour { get; set; }

    /// <summary>
    /// Selection colour
    /// </summary>
    public int? SelectionColour { get; set; }

    /// <summary>
    /// Specifies whether or not a backup file is created when a sheet
    /// is saved.
    /// </summary>
    public bool BackupFile { get; set; }

    /// <summary>
    /// Default cell alignment
    /// </summary>
    public CellAlignment DefaultCellAlignment { get; set; } = CellAlignment.GENERAL;

    /// <summary>
    /// Default cell format
    /// </summary>
    public CellFormat DefaultCellFormat { get; set; } = CellFormat.GENERAL;

    /// <summary>
    /// Default number of decimal places
    /// </summary>
    public int DefaultDecimals { get; set; } = 2;

    /// <summary>
    /// Load a config file if one is found, otherwise return a default
    /// configuration.
    /// </summary>
    /// <returns>A Config object initialised from the file</returns>
    public static Config Load() {
        Config fileConfig = new();
        string configFile = Path.Combine(Utilities.ConfigPath, Consts.ConfigurationFilename);
        if (File.Exists(configFile)) {
            try {
                using FileStream stream = File.OpenRead(configFile);
                fileConfig = JsonSerializer.Deserialize<Config>(stream) ?? fileConfig;
            }
            catch (Exception) {
                fileConfig = new Config();
            }
        }
        return fileConfig;
    }

    /// <summary>
    /// Save configurations back to the config file.
    /// </summary>
    public void Save() {
        try {
            using FileStream stream = File.Create(Path.Combine(Utilities.ConfigPath, Consts.ConfigurationFilename));
            JsonSerializer.Serialize(stream, this, new JsonSerializerOptions {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = true
            });
        }
        catch (Exception e) {
            Screen.Status.Message(string.Format(Calc.CannotSaveConfigError, e.Message));
        }
    }
}