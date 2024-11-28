// Jcom Runtime Libary
// Byte Reader class
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

using System.Text;

namespace JComLib;

/// <summary>
/// Classes for reading bytes from a buffer.
/// </summary>
public class ByteReader {
    private readonly byte[] _buffer;
    private int _index;

    /// <summary>
    /// Initialise a ByteReader from a stream.
    /// </summary>
    /// <param name="stream"></param>
    public ByteReader(Stream stream) {
        using BinaryReader writer = new(stream);
        long size = stream.Length;
        _buffer = new byte[size];
        do {
            int bytesToRead = size > int.MaxValue ? int.MaxValue : (int)size;
            writer.Read(_buffer, 0, bytesToRead);
            size -= bytesToRead;
        } while (size > 0);
        _index = 0;
    }

    /// <summary>
    /// Return whether we've reached the end of the buffer
    /// </summary>
    public bool End => _index == _buffer.Length;

    /// <summary>
    /// Read an integer from the byte buffer.
    /// </summary>
    /// <returns>Integer</returns>
    public int ReadInteger() {
        byte[] intBytes = new byte[sizeof(int)];
        for (int index = 0; index < sizeof(int); index++) {
            intBytes[index] = _buffer[_index + index];
        }
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(intBytes);
        }
        _index += sizeof(int);
        return BitConverter.ToInt32(intBytes, 0);
    }

    /// <summary>
    /// Read a floating point number from the byte buffer.
    /// </summary>
    /// <returns>Floating point number</returns>
    public float ReadFloat() {
        byte[] floatBytes = new byte[sizeof(float)];
        for (int index = 0; index < sizeof(float); index++) {
            floatBytes[index] = _buffer[_index + index];
        }
        _index += sizeof(float);
        return BitConverter.ToSingle(floatBytes, 0);
    }

    /// <summary>
    /// Read a string from the byte buffer.
    /// </summary>
    /// <returns>String</returns>
    public string ReadString() {
        int length = ReadInteger();
        StringBuilder str = new();
        while (length-- > 0) {
            char ch = BitConverter.ToChar(_buffer, _index);
            _index += sizeof(char);
            str.Append(ch);
        }
        return str.ToString();
    }
}