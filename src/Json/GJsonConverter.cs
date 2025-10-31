// Copyright notice from hk-modding/api:
//
// MIT License
//
// Copyright (c) 2017 seanpr96, iamwyza, firzen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable CS8600, CS8603, CS8604, CS8765

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Silksong.DataManager.Json;

/// <inheritdoc />
public abstract class JsonConverter<TClass> : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return typeof(TClass) == objectType;
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        if (typeof(TClass) == objectType)
        {
            Dictionary<string, object> token = new Dictionary<string, object>();
            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string name = (string)reader.Value;
                // Value
                reader.Read();
                token.Add(name, reader.Value);
                // JsonToken.PropertyName
                reader.Read();
            }
            return ReadJson(token, existingValue);
        }
        return serializer.Deserialize(reader);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        WriteJson(writer, (TClass)value);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Read from token
    /// </summary>
    /// <param name="token">JSON object</param>
    /// <param name="existingValue">Existing value</param>
    /// <returns></returns>
    public abstract TClass ReadJson(Dictionary<string, object> token, object existingValue);

    /// <summary>
    /// Write value into token
    /// </summary>
    /// <param name="writer">JSON Writer</param>
    /// <param name="value">Value to be written</param>
    public abstract void WriteJson(JsonWriter writer, TClass value);
}
