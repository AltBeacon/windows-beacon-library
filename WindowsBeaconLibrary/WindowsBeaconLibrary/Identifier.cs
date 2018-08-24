// <copyright file="Identifier.cs" company="Radius Networks, Inc.">
//     Copyright (c) Radius Networks, Inc.
//     http://www.radiusnetworks.com
// </copyright>
// 
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
namespace Altbeacon.Beacon
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Text.RegularExpressions;

    /// <summary>
    /// <para>
    /// Encapsulates a beacon identifier of arbitrary byte length.
    /// It can encapsulate an identifier that is a 16-byte UUID, or an integer.
    /// </para>
    /// <para>
    /// Instances of this class are immutable, so those can be shared without problem between threads.
    /// </para>
    /// <para>
    /// The value is internally this is stored as a byte array.
    /// </para>
    /// </summary>
    public class Identifier : IComparable<Identifier>
    {
        #region Static Fields
        /// <summary>
        /// Hexadecimal Identifier String Format
        /// </summary>
        private static readonly Regex HexPattern = new Regex("^0x[0-9A-Fa-f]*$");

        /// <summary>
        /// Decimal Identifier String Format
        /// </summary>
        private static readonly Regex DecimalPattern = new Regex("^[0-9]+$");

        /// <summary>
        /// UUID Identifier String Format
        /// </summary>
        private static readonly Regex UuidPattern = new Regex("^[0-9A-Fa-f]{8}-?[0-9A-Fa-f]{4}-?[0-9A-Fa-f]{4}-?[0-9A-Fa-f]{4}-?[0-9A-Fa-f]{12}$");

        /// <summary>
        /// Hex array to fasten Identifier to hexadecimal string conversation
        /// </summary>
        private static readonly char[] HexArray = "0123456789abcdef".ToCharArray();

        /// <summary>
        /// Maximum value of Identifier if it is an integer (2 bytes)
        /// </summary>
        private static readonly BigInteger MaxInteger = new BigInteger(65535);
        #endregion Static Fields

        #region Class Fields
        /// <summary>
        /// Identifier value as byte array
        /// </summary>
        private readonly byte[] value;
        #endregion Class Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier"/> class.
        /// </summary>
        /// <param name="value">
        /// Value to use.
        /// </param>
        /// <exception cref="System.NullReferenceException">
        /// Thrown when <code>value</code> is <code>null</code>.
        /// </exception>
        protected Identifier(byte[] value)
        {
            if (value == null)
            {
                throw new NullReferenceException("identifier == null");
            }

            this.value = (byte[])value.Clone();
        }
        #endregion Constructors

        #region Class Properties
        /// <summary>
        /// Gets the byte length of this identifier.
        /// </summary>
        public int ByteCount
        {
            get
            {
                return this.value.Length;
            }
        }
        #endregion Class Properties

        #region Static Factory Methods
        /// <summary>
        /// Allowed formats:
        /// <list type="bullet">
        /// <item>
        ///   <description>
        ///     UUID: 2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6 (16 bytes Identifier, dashes optional)
        ///   </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     HEX: 0x000000000003 (number of bytes is based on String length)
        ///   </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     Decimal: 65536 (2 bytes Identifier)
        ///   </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="stringValue">
        /// stringValue string to parse
        /// </param>
        /// <returns>
        /// identifier representing the specified value
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when String cannot be parsed
        /// </exception>
        /// <exception cref="System.NullReferenceException">
        /// Thrown when stringValue is <code>null</code>
        /// </exception>
        public static Identifier Parse(string stringValue)
        {
            if (stringValue == null)
            {
                throw new NullReferenceException("stringValue == null");
            }

            if (HexPattern.IsMatch(stringValue))
            {
                return ParseHex(stringValue.Substring(2));
            }
            else if (UuidPattern.IsMatch(stringValue))
            {
                return ParseHex(stringValue.Replace("-", string.Empty));
            }
            else if (DecimalPattern.IsMatch(stringValue))
            {
                BigInteger i = BigInteger.Parse(stringValue);

                if (i.CompareTo(BigInteger.Zero) < 0 || i.CompareTo(MaxInteger) > 0)
                {
                    throw new ArgumentException("Decimal formatted integers must be " +
                        "between 0 and 65535. Value: " + stringValue);
                }

                return FromInt((int)i);
            }
            else
            {
                throw new ArgumentException("Unable to parse identifier: " + stringValue);
            }
        }

        /// <summary>
        /// Creates an Identifier backed by a two byte Array (big endian).
        /// </summary>
        /// <param name="intValue">
        /// An integer between 0 and 65535 (inclusive).
        /// </param>
        /// <returns>
        /// An Identifier with the specified value
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when given intValue is not between 0 and 65535 (inclusive).
        /// </exception>
        public static Identifier FromInt(int intValue)
        {
            if (intValue < 0 || intValue > 0xFFFF)
            {
                throw new ArgumentException("value must be between 0 and 65535");
            }

            byte[] newValue = new byte[2];
            newValue[0] = (byte)(intValue >> 8);
            newValue[1] = (byte)intValue;
            return new Identifier(newValue);
        }

        /// <summary>
        /// Creates an Identifier from the specified byte array.
        /// </summary>
        /// <param name="bytes">
        /// Bytes array to copy from.
        /// </param>
        /// <param name="start">
        /// <code>start</code> the start index, inclusive.
        /// </param>
        /// <param name="end">
        /// <code>end</code> the end index, exclusive.
        /// </param>
        /// <param name="littleEndian">
        /// <code>littleEndian</code> whether the bytes are ordered in little endian.
        /// </param>
        /// <returns>
        /// A new Identifier.
        /// </returns>
        /// <exception cref="System.NullReferenceException">
        /// Thrown when <code>bytes</code> is <code>null</code>.
        /// </exception>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when <code>start</code> or <code>end</code> are outside the bounds of the array
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <code>start</code> is larger than or equal to <code>end</code>.
        /// </exception>
        public static Identifier FromBytes(byte[] bytes, int start, int end, bool littleEndian)
        {
            if (bytes == null)
            {
                throw new NullReferenceException("bytes == null");
            }

            if (start < 0 || start > bytes.Length)
            {
                throw new IndexOutOfRangeException("start < 0 || start > bytes.Length");
            }

            if (end > bytes.Length)
            {
                throw new IndexOutOfRangeException("end > bytes.Length");
            }

            if (start >= end)
            {
                throw new ArgumentException("start >= end");
            }

            byte[] byteRange = new byte[end - start];
            Array.Copy(bytes, start, byteRange, 0, byteRange.Length);

            if (littleEndian)
            {
                ReverseArray(byteRange);
            }

            return new Identifier(byteRange);
        }
        #endregion Static Factory Methods

        #region Converter Methods
        /// <summary>
        /// Represents the value as a String. The output varies based on the length of the value.
        /// <list type="bullet">
        /// <item>
        ///   <description>
        ///     When the value is 2 bytes long: decimal, for example 6536
        ///   </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     When the value is 16 bytes long: uuid, for example 2f234454-cf6d-4a0f-adf2-f4911ba9ffa6
        ///   </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     Else: hexadecimal prefixed with <code>0x</code>, for example 0x0012ab
        ///   </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>
        /// string representation of the current value
        /// </returns>
        public override string ToString()
        {
            ////
            // Note: the ToString() method is also used for serialization and deserialization.
            // So ToString() and Parse() must always return objects that return true when 
            // you call Equals()
            ////

            if (this.value.Length == 2)
            {
                return this.ToInt().ToString();
            }

            if (this.value.Length == 16)
            {
                return this.ToUuidString();
            }

            return "0x" + this.ToHexString();
        }

        /// <summary>
        /// Represents the value as an <code>int</code>.
        /// </summary>
        /// <returns>
        /// Value represented as int.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when value length is longer than 2.
        /// </exception>
        public int ToInt()
        {
            if (this.value.Length > 2)
            {
                throw new InvalidOperationException("Only supported for Identifiers " + 
                    "with max byte length of 2");
            }

            int result = 0;

            for (int i = 0; i < this.value.Length; i++)
            {
                result |= (this.value[i] & 0xFF) << ((this.value.Length - i - 1) * 8);
            }

            return result;
        }

        /// <summary>
        /// Converts identifier to a byte array.
        /// </summary>
        /// <param name="bigEndian">
        /// <code>true</code> if bytes are MSB first.
        /// </param>
        /// <returns>
        /// A new byte array with a copy of the value.
        /// </returns>
        public byte[] ToByteArrayOfSpecifiedEndianness(bool bigEndian)
        {
            byte[] copy = (byte[])this.value.Clone();

            if (!bigEndian)
            {
                ReverseArray(copy);
            }

            return copy;
        }

        /// <summary>
        /// Represents the value as a hexadecimal String.
        /// The String is prefixed with <code>0x</code>. For example 0x0034ab.
        /// </summary>
        /// <returns>
        /// Value as hexadecimal String
        /// </returns>
        public string ToHexString()
        {
            return ToHexString(this.value, 0, this.value.Length);
        }

        /// <summary>
        /// Returns the value of this Identifier in uuid form. For example 2f234454-cf6d-4a0f-adf2-f4911ba9ffa6
        /// </summary>
        /// <returns>
        /// Value in uuid form
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when value length is 16 bytes.
        /// </exception>
        public string ToUuidString()
        {
            if (this.value.Length != 16)
            {
                throw new InvalidOperationException("Only available for values with length of 16 bytes");
            }

            return ToHexString(this.value, 0, 4) + "-" + ToHexString(this.value, 4, 2) + "-" +
                ToHexString(this.value, 6, 2) + "-" + ToHexString(this.value, 8, 2) + "-" +
                ToHexString(this.value, 10, 6);
        }
        #endregion Converter Methods

        #region Overriden Object Methods
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Returns whether both Identifiers contain equal value. 
        /// This is the case when the value is the same and has the same length.
        /// </summary>
        /// <param name="that">
        /// Object to compare to
        /// </param>
        /// <returns>
        /// Whether that equals this.
        /// </returns>
        public override bool Equals(object that)
        {
            if (!(that is Identifier))
            {
                return false;
            }

            Identifier thatIdentifier = (Identifier)that;
            return Enumerable.SequenceEqual(this.value, thatIdentifier.value);
        }
        #endregion Overriden Object Methods

        #region IComparable Methods
        /// <summary>
        /// Compares two identifiers.
        /// When the Identifiers don't have the same length, 
        /// the Identifier having the shortest
        /// array is considered smaller than the other.
        /// </summary>
        /// <param name="that">
        /// <code>that</code> the other identifier
        /// </param>
        /// <returns>
        /// 0 if both identifiers are equal. Otherwise returns -1 or 1 depending on which is
        /// bigger than the other
        /// </returns>
        public int CompareTo(Identifier that)
        {
            if (this.value.Length != that.value.Length)
            {
                return this.value.Length < that.value.Length ? -1 : 1;
            }

            for (int i = 0; i < this.value.Length; i++)
            {
                if (this.value[i] != that.value[i])
                {
                    return this.value[i] < that.value[i] ? -1 : 1;
                }
            }

            return 0;
        }
        #endregion IComparable Methods

        #region Private Static Helper Methods
        /// <summary>
        /// Parses hexadecimal string value.
        /// </summary>
        /// <param name="identifierString">
        /// Hexadecimal string value.
        /// </param>
        /// <returns>
        /// A new Identifier parsed from <code>identifierString</code>.
        /// </returns>
        private static Identifier ParseHex(string identifierString)
        {
            string str = (identifierString.Length % 2 == 0) ? identifierString.ToLower() :
                "0" + identifierString.ToLower();

            byte[] result = new byte[str.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int v = Convert.ToInt32(str.Substring(i * 2, 2), 16);
                result[i] = (byte)v;
            }

            return new Identifier(result);
        }

        /// <summary>
        /// Reverses given array.
        /// </summary>
        /// <param name="bytes">
        /// Byte array to reverse.
        /// </param>
        private static void ReverseArray(byte[] bytes)
        {
            // Maybe use Array.Reverse instead?
            for (int i = 0; i < bytes.Length / 2; i++)
            {
                byte a = bytes[i];
                byte b = bytes[bytes.Length - i - 1];
                bytes[i] = b;
                bytes[bytes.Length - i - 1] = a;
            }
        }

        /// <summary>
        /// Converts given byte array to hexadecimal string.
        /// </summary>
        /// <param name="bytes">
        /// Byte array to convert
        /// </param>
        /// <param name="start">
        /// Start index of the <code>bytes</code>.
        /// </param>
        /// <param name="length">
        /// The number of bytes in the byte array to use.
        /// </param>
        /// <returns>
        /// Converted Hexadecimal String
        /// </returns>
        private static string ToHexString(byte[] bytes, int start, int length)
        {
            char[] hexChars = new char[length * 2];

            for (int i = 0; i < length; i++)
            {
                int v = bytes[start + i] & 0xFF;
                hexChars[i * 2] = HexArray[v >> 4];
                hexChars[(i * 2) + 1] = HexArray[v & 0x0F];
            }

            return new string(hexChars);
        }
        #endregion Private Static Helper Methods
    }
}
