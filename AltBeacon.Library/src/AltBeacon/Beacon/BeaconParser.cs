// <copyright file="BeaconParser.cs" company="Radius Networks, Inc.">
//     Copyright (c) Radius Networks, Inc.
//     http://www.radiusnetworks.com
// </copyright>
// <author>
//     David G. Young
// </author>
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
namespace AltBeacon.Beacon
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using AltBeacon.Bluetooth;

    /// <summary>
    /// <para>
    /// A <code>BeaconParser</code> may be used to tell the library how to decode
    /// a beacon's fields from a Bluetooth LE advertisement by specifying what
    /// byte offsets match what fields, and what byte sequence signifies the beacon.
    /// Defining a parser for a specific beacon type may be handled via sub classing
    /// or by simply constructing an instance and calling the <code>setLayout</code>
    /// method. Either way, you will then need to tell the BeaconManager about it like so:
    /// </para>
    /// <pre><code>
    /// new BeaconParser()
    ///     .setBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
    /// </code></pre>
    /// <para>
    /// For more information on how to set up parsing of a beacon,
    /// <see cref="SetBeaconLayout(string)"/>.
    /// </para>
    /// </summary>
    public partial class BeaconParser
    {
        #region Static Fields
        /// <summary>
        /// Logger Tag
        /// </summary>
        private const string Tag = "BeaconParser";

        /// <summary>
        /// Identifier Pattern
        /// </summary>
        private static readonly Regex IdentiferPattern = new Regex("i\\:(\\d+)\\-(\\d+)(l?)");

        /// <summary>
        /// Manufacturer Pattern
        /// </summary>
        private static readonly Regex ManufacturerPattern = new Regex("m\\:(\\d+)-(\\d+)\\=([0-9A-F-a-f]+)");

        /// <summary>
        /// Service UUID Pattern
        /// </summary>
        private static readonly Regex ServicePattern = new Regex("s\\:(\\d+)-(\\d+)\\=([0-9A-Fa-f]+)");

        /// <summary>
        /// Data Pattern
        /// </summary>
        private static readonly Regex DataPattern = new Regex("d\\:(\\d+)\\-(\\d+)([bl]?)");

        /// <summary>
        /// Calculated TxPower Pattern
        /// </summary>
        private static readonly Regex PowerPattern = new Regex("p\\:(\\d+)\\-(\\d+)");

        /// <summary>
        /// Hex Array
        /// </summary>
        private static readonly char[] HexArray = "0123456789abcdef".ToCharArray();
        #endregion Static Fields

        #region Class Fields
        private long matchingBeaconTypeCode;
        private List<int?> identifierStartOffsets;
        private List<int?> identifierEndOffsets;
        private List<bool> identifierLittleEndianFlags;
        private List<int?> dataStartOffsets;
        private List<int?> dataEndOffsets;
        private List<bool> dataLittleEndianFlags;
        private int? matchingBeaconTypeCodeStartOffset;
        private int? matchingBeaconTypeCodeEndOffset;
        private int? serviceUuidStartOffset;
        private int? serviceUuidEndOffset;
        private long? serviceUuid;

        private int? powerStartOffset;
        private int? powerEndOffset;
        private int[] hardwareAssistManufacturers = new int[] { 0x004c };
        #endregion Class Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BeaconParser"/> class.
        /// Should normally be immediately followed by a call to <see cref="SetLayout(string)"/>.
        /// </summary>
        public BeaconParser()
        {
            this.identifierStartOffsets = new List<int?>();
            this.identifierEndOffsets = new List<int?>();
            this.dataStartOffsets = new List<int?>();
            this.dataEndOffsets = new List<int?>();
            this.dataLittleEndianFlags = new List<bool>();
            this.identifierLittleEndianFlags = new List<bool>();
        }
        #endregion Constructors

        #region Class Properties
        /// <summary>
        /// <para>
        /// Gets or sets a list of bluetooth manufacturer codes which will be used
        /// for hardware-assisted accelerated looking for this beacon type
        /// </para>
        /// <para>
        /// The possible codes are defined on this list:
        /// https://www.bluetooth.org/en-us/specification/assigned-numbers/company-identifiers
        /// </para>
        /// </summary>
        public int[] HardwareAssistantManufacturers
        {
            get
            {
                return this.hardwareAssistManufacturers;
            }

            set
            {
                this.hardwareAssistManufacturers = value;
            }
        }

        /// <summary>
        /// Gets or sets Matching Beacon Type Code
        /// </summary>
        public long MatchingBeaconTypeCode
        {
            get
            {
                return this.matchingBeaconTypeCode;
            }

            set
            {
                this.matchingBeaconTypeCode = value;
            }
        }

        /// <summary>
        /// Gets Matching Beacon Type Code Start Offset
        /// </summary>
        public int MatchingBeaconTypeCodeStartOffset
        {
            get
            {
                return this.matchingBeaconTypeCodeStartOffset.Value;
            }
        }

        /// <summary>
        /// Gets Matching Beacon Type Code End Offset
        /// </summary>
        public int MatchingBeaconTypeCodeEndOffset
        {
            get
            {
                return this.matchingBeaconTypeCodeEndOffset.Value;
            }
        }

        /// <summary>
        /// Gets Bluetooth Service UUID
        /// </summary>
        public long? ServiceUuid
        {
            get
            {
                return this.serviceUuid;
            }
        }

        /// <summary>
        /// Gets Bluetooth Service UUID Start Offset
        /// </summary>
        public int ServiceUuidStartOffset
        {
            get
            {
                return this.serviceUuidStartOffset.Value;
            }
        }

        /// <summary>
        /// Gets Bluetooth Service UUID End Offset
        /// </summary>
        public int ServiceUuidEndOffset
        {
            get
            {
                return this.serviceUuidEndOffset.Value;
            }
        }
        #endregion Class Properties

        #region Public Methods
        /// <summary>
        /// <para>
        /// Defines a beacon field parsing algorithm based on a string designating
        /// the zero-indexed offsets to bytes within a BLE advertisement.
        /// </para>
        /// <para>
        /// If you want to see examples of how other folks have set up BeaconParsers
        /// for different kinds of beacons, try doing a Google search for 
        /// "getBeaconParsers" (include the quotes in the search).
        /// </para>
        /// <para>
        /// Four prefixes are allowed in the string:
        /// </para>
        /// <pre>
        /// m - matching byte sequence for this beacon type to parse (exactly one required)
        /// i - identifier (at least one required, multiple allowed)
        /// p - power calibration field (exactly one required)
        /// d - data field (optional, multiple allowed)
        /// </pre>
        /// <para>
        /// Each prefix is followed by a colon, then an inclusive decimal byte offset
        /// for the field from the beginning of the advertisement. In the case of
        /// the m prefix, an = sign follows the byte offset, followed by a big endian
        /// hex representation of the bytes that must be matched for this beacon type.
        /// When multiple i or d entries exist in the string, they will be added in
        /// order of definition to the identifier or data array for the beacon
        /// when parsing the beacon advertisement. Terms are separated by commas.
        /// </para>
        /// <para>
        /// All offsets from the start of the advertisement are relative to the first byte
        /// of the two byte manufacturer code. The manufacturer code is therefore always
        /// at position 0-1.
        /// </para>
        /// <para>
        /// All data field and identifier expressions may be optionally suffixed with
        /// the letter l, which indicates the field should be parsed as little endian.
        /// If not present, the field will be presumed to be big endian.
        /// </para>
        /// <para>
        /// If the expression cannot be parsed, a <see cref="BeaconLayoutException"/> is thrown
        /// </para>
        /// <para>
        /// Example of a parser string for AltBeacon:
        /// </para>
        /// <pre>
        /// "m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25"
        /// </pre>
        /// <para>
        /// This signifies that the beacon type will be decoded when an advertisement is
        /// found with 0xbeac in bytes 2-3, and a three-part identifier will be pulled out
        /// of bytes 4-19, bytes 20-21 and bytes 22-23, respectively. A signed power
        /// calibration value will be pulled out of byte 24, and a data field will be
        /// pulled out of byte 25.
        /// </para>
        /// <para>
        /// <b>Note</b>: bytes 0-1 of the BLE manufacturer advertisements are the two byte
        /// manufacturer code. Generally you should not match on these two bytes when using
        /// a <see cref="BeaconParser"/>, because it will limit your parser to matching
        /// only a transmitter made by a specific manufacturer. Software and operating
        /// systems that scan for beacons typically ignore these two bytes, allowing
        /// beacon manufacturers to use their own company code assigned by Bluetooth SIG.
        /// The default parser implementation will already pull out this company code
        /// and store it in the beacon.mManufacturer field. Matcher expressions should
        /// therefore start with "m2-3:" followed by the multi-byte hex value
        /// that signifies the beacon type.
        /// </para>
        /// </summary>
        /// <param name="beaconLayout">
        /// Beacon Layout
        /// </param>
        /// <returns>
        /// The BeaconParser instance
        /// </returns>
        public BeaconParser SetBeaconLayout(string beaconLayout)
        {
            string[] terms = beaconLayout.Split(new[] { ',' });

            foreach (string term in terms)
            {
                bool found = false;

                MatchCollection matches = IdentiferPattern.Matches(term);
                foreach (Match match in matches)
                {
                    GroupCollection group = match.Groups;
                    found = true;
                    try
                    {
                        int startOffset = int.Parse(group[1].ToString());
                        int endOffset = int.Parse(group[2].ToString());
                        bool littleEndian = group[3].ToString().Equals("l");
                        this.identifierLittleEndianFlags.Add(littleEndian);
                        this.identifierStartOffsets.Add(startOffset);
                        this.identifierEndOffsets.Add(endOffset);
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse integer byte offset in term: " + term);
                    }
                }

                matches = DataPattern.Matches(term);
                foreach (Match match in matches)
                {
                    GroupCollection group = match.Groups;
                    found = true;
                    try
                    {
                        int startOffset = int.Parse(group[1].ToString());
                        int endOffset = int.Parse(group[2].ToString());
                        bool littleEndian = group[3].ToString().Equals("l");
                        this.dataLittleEndianFlags.Add(littleEndian);
                        this.dataStartOffsets.Add(startOffset);
                        this.dataEndOffsets.Add(endOffset);
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse integer byte offset in term: " + term);
                    }
                }

                matches = PowerPattern.Matches(term);
                foreach (Match match in matches)
                {
                    GroupCollection group = match.Groups;
                    found = true;
                    try
                    {
                        int startOffset = int.Parse(group[1].ToString());
                        int endOffset = int.Parse(group[2].ToString());
                        this.powerStartOffset = startOffset;
                        this.powerEndOffset = endOffset;
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse integer power byte offset in term: " + term);
                    }
                }

                matches = ManufacturerPattern.Matches(term);
                foreach (Match match in matches)
                {
                    GroupCollection group = match.Groups;
                    found = true;
                    try
                    {
                        int startOffset = int.Parse(group[1].ToString());
                        int endOffset = int.Parse(group[2].ToString());
                        this.matchingBeaconTypeCodeStartOffset = startOffset;
                        this.matchingBeaconTypeCodeEndOffset = endOffset;
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse integer byte offset in term: " + term);
                    }

                    string hexString = group[3].ToString();
                    try
                    {
                        this.matchingBeaconTypeCode = Convert.ToInt64(hexString, 16);
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse beacon type code: " + hexString + " in term: " + term);
                    }
                }

                matches = ServicePattern.Matches(term);
                foreach (Match match in matches)
                {
                    GroupCollection group = match.Groups;
                    found = true;
                    try
                    {
                        int startOffset = int.Parse(group[1].ToString());
                        int endOffset = int.Parse(group[2].ToString());
                        this.serviceUuidStartOffset = startOffset;
                        this.serviceUuidEndOffset = endOffset;
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse integer byte offset in term: " + term);
                    }

                    string hexString = group[3].ToString();
                    try
                    {
                        this.serviceUuid = Convert.ToInt64(hexString, 16);
                    }
                    catch (FormatException)
                    {
                        throw new BeaconLayoutException("Cannot parse serviceUuid: " + hexString + " in term: " + term);
                    }
                }

                if (!found)
                {
                    ////TODO LogManager
                    ////LogManager.d(Tag, "cannot parse term " + term);
                    throw new BeaconLayoutException("Cannot parse beacon layout term: " + term);
                }
            }

            if (this.powerStartOffset == null || this.powerEndOffset == null)
            {
                throw new BeaconLayoutException("You must supply a power byte offset with a prefix of 'p'");
            }

            if (this.matchingBeaconTypeCodeStartOffset == null || this.matchingBeaconTypeCodeEndOffset == null)
            {
                throw new BeaconLayoutException("You must supply a matching beacon type expression with a prefix of 'm'");
            }

            if (this.identifierStartOffsets.Count == 0 || this.identifierEndOffsets.Count == 0)
            {
                throw new BeaconLayoutException("You must supply at least one identifier offset withh a prefix of 'i'");
            }

            return this;
        }

        /// <summary>
        /// Construct a Beacon from a Bluetooth LE packet including the raw bluetooth device info.
        /// </summary>
        /// <param name="scanData">
        /// The actual packet bytes
        /// </param>
        /// <param name="rssi">
        /// The measured signal strength of the packet
        /// </param>
        /// <param name="device">
        /// The bluetooth device that was detected
        /// </param>
        /// <returns>
        /// An instance of a <code>Beacon</code>
        /// </returns>
        public Beacon FromScanData(byte[] scanData, int rssi, BluetoothDevice device)
        {
            return this.FromScanData(scanData, rssi, device, new Beacon.Builder());
        }

        /// <summary>
        /// Get BLE advertisement bytes for a Beacon
        /// </summary>
        /// <param name="beacon">the beacon containing the data to be transmitted</param>
        /// <returns>the byte array of the advertisement</returns>
        public byte[] GetBeaconAdvertisementData(Beacon beacon)
        {
            byte[] advertisingBytes;

            int lastIndex = -1;
            if (this.matchingBeaconTypeCodeEndOffset != null &&
                this.matchingBeaconTypeCodeEndOffset.Value > lastIndex)
            {
                lastIndex = this.matchingBeaconTypeCodeEndOffset.Value;
            }

            if (this.powerEndOffset != null && this.powerEndOffset > lastIndex)
            {
                lastIndex = this.powerEndOffset.Value;
            }

            for (int identifierNum = 0;
                identifierNum < this.identifierStartOffsets.Count;
                identifierNum++)
            {
                if (this.identifierEndOffsets[identifierNum] != null &&
                    this.identifierEndOffsets[identifierNum] > lastIndex)
                {
                    lastIndex = this.identifierEndOffsets[identifierNum].Value;
                }
            }

            for (int identifierNum = 0; identifierNum < this.dataEndOffsets.Count; identifierNum++)
            {
                if (this.dataEndOffsets[identifierNum] != null &&
                    this.dataEndOffsets[identifierNum] > lastIndex)
                {
                    lastIndex = this.dataEndOffsets[identifierNum].Value;
                }
            }

            advertisingBytes = new byte[lastIndex + 1 - 2];
            long beaconTypeCode = this.MatchingBeaconTypeCode;

            // set type code
            for (int index = this.matchingBeaconTypeCodeStartOffset.Value;
                index <= this.matchingBeaconTypeCodeEndOffset; index++)
            {
                byte value = (byte)(this.MatchingBeaconTypeCode >>
                    (8 * (this.matchingBeaconTypeCodeEndOffset - index)) & 0xff);
                advertisingBytes[index - 2] = value;
            }

            // set identifiers
            for (int identifierNum = 0; identifierNum < this.identifierStartOffsets.Count;
                identifierNum++)
            {
                byte[] identifierBytes = beacon.Identifiers[identifierNum]
                    .ToByteArrayOfSpecifiedEndianness(this.identifierLittleEndianFlags[identifierNum]);

                for (int index = this.identifierStartOffsets[identifierNum].Value;
                    index <= this.identifierEndOffsets[identifierNum]; index++)
                {
                    int identifierByteIndex = this.identifierEndOffsets[identifierNum].Value - index;
                    if (identifierByteIndex < identifierBytes.Length)
                    {
                        advertisingBytes[index - 2] =
                            identifierBytes[this.identifierEndOffsets[identifierNum].Value - index];
                    }
                    else
                    {
                        advertisingBytes[index - 2] = 0;
                    }
                }
            }

            // set power
            for (int index = this.powerStartOffset.Value; index <= this.powerEndOffset; index++)
            {
                advertisingBytes[index - 2] = (byte)(beacon.TxPower >>
                    (8 * (index - this.powerStartOffset)) & 0xff);
            }

            // set data fields
            for (int dataFieldNum = 0; dataFieldNum < this.dataStartOffsets.Count; dataFieldNum++)
            {
                long dataField = beacon.DataFields[dataFieldNum];
                for (int index = this.dataStartOffsets[dataFieldNum].Value;
                    index <= this.dataEndOffsets[dataFieldNum]; index++)
                {
                    int endianCorrectedIndex = index;
                    if (this.dataLittleEndianFlags[dataFieldNum])
                    {
                        endianCorrectedIndex = this.dataEndOffsets[dataFieldNum].Value - index;
                    }

                    advertisingBytes[endianCorrectedIndex - 2] = (byte)(dataField >>
                        (8 * (index - this.dataStartOffsets[dataFieldNum])) & 0xff);
                }
            }

            return advertisingBytes;
        }

        /// <summary>
        /// Calculates the byte size of the specified identifier in this format
        /// </summary>
        /// <param name="identifierNum">
        /// Identifier number (zero "0" based)
        /// </param>
        /// <returns>
        /// Byte count
        /// </returns>
        public int GetIdentifierByteCount(int identifierNum)
        {
            return this.identifierEndOffsets[identifierNum].Value -
                this.identifierStartOffsets[identifierNum].Value + 1;
        }
        #endregion Public Methods

        #region Static Utility Methods
        /// <summary>
        /// Converts long to byte array
        /// </summary>
        /// <param name="longValue">
        /// Long value to be converted
        /// </param>
        /// <param name="length">
        /// Result byte array length
        /// </param>
        /// <returns>
        /// Converted byte array
        /// </returns>
        protected static byte[] LongToByteArray(long longValue, int length)
        {
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++)
            {
                ////long mask = (long)(Math.pow(256.0, (1.0 * (length-i))) - 1);
                long mask = 0xffL << ((length - i - 1) * 8);
                int shift = (length - i - 1) * 8;
                long value = (longValue & mask) >> shift;
                array[i] = (byte)value;
            }

            //// TODO Maybe this one ?
            //// byte[] array = BitConverter.GetBytes(longValue);

            return array;
        }

        /// <summary>
        /// Converts bytes to hex string
        /// </summary>
        /// <param name="bytes">
        /// Bytes to convert
        /// </param>
        /// <returns>
        /// Converted hex string
        /// </returns>
        protected static string BytesToHex(byte[] bytes)
        {
            char[] hexChars = new char[bytes.Length * 2];
            int v;

            for (int j = 0; j < bytes.Length; j++)
            {
                v = bytes[j] & 0xFF;
                hexChars[(j * 2)] = HexArray[v >> 4];
                hexChars[(j * 2) + 1] = HexArray[v & 0x0F];
            }

            return new string(hexChars);
        }

        /// <summary>
        /// Converts byte array to string
        /// </summary>
        /// <param name="bytes">
        /// Byte array to be converted
        /// </param>
        /// <returns>
        /// Converted String
        /// </returns>
        protected static string ByteArrayToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Converts byte array to formatted string
        /// </summary>
        /// <param name="byteBuffer">
        /// Byte array to be converted
        /// </param>
        /// <param name="startIndex">
        /// Start index
        /// </param>
        /// <param name="endIndex">
        /// End index
        /// </param>
        /// <param name="littleEndian">
        /// Shows whether byte array is little endian
        /// </param>
        /// <returns>
        /// Converted formatted string
        /// </returns>
        protected static string ByteArrayToFormattedString(byte[] byteBuffer, int startIndex, int endIndex, bool littleEndian)
        {
            byte[] bytes = new byte[endIndex - startIndex + 1];
            if (littleEndian)
            {
                for (int i = 0; i <= endIndex - startIndex; i++)
                {
                    bytes[i] = byteBuffer[startIndex + bytes.Length - 1 - i];
                }
            }
            else
            {
                for (int i = 0; i <= endIndex - startIndex; i++)
                {
                    bytes[i] = byteBuffer[startIndex + i];
                }
            }

            int length = endIndex - startIndex + 1;

            // We treat a 1-4 byte number as decimal string
            if (length < 5)
            {
                long number = 0L;

                //// TODO LogManager
                //// LogManager.d(Tag, "Byte array is size " + bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                {
                    //// TODO LogManager
                    //// LogManager.d(Tag, "index is " + i);
                    long byteValue = (long)(bytes[bytes.Length - i - 1] & 0xff);
                    long positionValue = (long)Math.Pow(256.0, i * 1.0);
                    long calculatedValue = (long)(byteValue * positionValue);
                    //// TODO LogManager
                    //// LogManager.d(Tag, "calculatedValue for position " + i + 
                    ////     " with positionValue " + positionValue + " and byteValue " + 
                    ////     byteValue + " is " + calculatedValue);
                    number += calculatedValue;
                }

                return number.ToString();
            }

            // We treat a 7+ byte number as a hex string
            string hexString = BytesToHex(bytes);

            // And if it is a 12 byte number we add dashes to it to make it look like a standard UUID
            if (bytes.Length == 16)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(hexString.Substring(0, 8));
                sb.Append("-");
                sb.Append(hexString.Substring(8, 4));
                sb.Append("-");
                sb.Append(hexString.Substring(12, 4));
                sb.Append("-");
                sb.Append(hexString.Substring(16, 4));
                sb.Append("-");
                sb.Append(hexString.Substring(20, 12));
                return sb.ToString();
            }

            return "0x" + hexString;
        }

        /// <summary>
        /// Checks whether byte arrays match or not
        /// </summary>
        /// <param name="array1">
        /// First array
        /// </param>
        /// <param name="offset1">
        /// First array offset
        /// </param>
        /// <param name="array2">
        /// Second array
        /// </param>
        /// <param name="offset2">
        /// Second array offset
        /// </param>
        /// <returns>
        /// <code>true</code> if both array are match, <code>false</code> otherwise.
        /// </returns>
        protected static bool AreByteArraysMatch(byte[] array1, int offset1, byte[] array2, int offset2)
        {
            int minSize = array1.Length > array2.Length ? array2.Length : array1.Length;
            for (int i = 0; i < minSize; i++)
            {
                if (array1[i + offset1] != array2[i + offset2])
                {
                    return false;
                }
            }

            return true;
        }
        #endregion Static Utility Methods

        #region Protected Methods
        /// <summary>
        /// Construct a Beacon from a Bluetooth LE packet collected by Android's Bluetooth APIs,
        /// including the raw bluetooth device info
        /// </summary>
        /// <param name="scanData">
        /// The actual packet bytes
        /// </param>
        /// <param name="rssi">
        /// The measured signal strength of the packet
        /// </param>
        /// <param name="device">
        /// The bluetooth device that was detected
        /// </param>
        /// <param name="beaconBuilder">
        /// Beacon Builder
        /// </param>
        /// <returns>
        /// An instance of a <code>Beacon</code>
        /// </returns>
        protected Beacon FromScanData(byte[] scanData, int rssi, BluetoothDevice device, Beacon.Builder beaconBuilder)
        {
            int startByte = 2;
            bool patternFound = false;
            int matchingBeaconSize = this.matchingBeaconTypeCodeEndOffset.Value - 
                this.matchingBeaconTypeCodeStartOffset.Value + 1;
            byte[] typeCodeBytes = LongToByteArray(this.MatchingBeaconTypeCode, matchingBeaconSize);

            while (startByte <= 5)
            {
                if (AreByteArraysMatch(scanData, startByte + this.matchingBeaconTypeCodeStartOffset.Value, typeCodeBytes, 0))
                {
                    patternFound = true;
                    break;
                }

                startByte++;
            }

            if (patternFound == false)
            {
                // This is not a beacon
                // TODO LogManager
                /*if (this.ServiceUuid == null)
                {
                    TODO LogManager
                    if (LogManager.isVerboseLoggingEnabled())
                    {
                        LogManager.d(TAG, "This is not a matching Beacon advertisement. " + 
                            "(Was expecting %s. The bytes I see are: %s",
                            byteArrayToString(typeCodeBytes), bytesToHex(scanData));
                    }
                }   
                else
                {
                    if (LogManager.isVerboseLoggingEnabled())
                    {
                        LogManager.d(TAG, "This is not a matching Beacon advertisement. " + 
                            "(Was expecting %s and %s. The bytes I see are: %s", 
                            byteArrayToString(serviceUuidBytes), 
                            byteArrayToString(typeCodeBytes), bytesToHex(scanData));
                    }
                }*/
                return null;
            }
            else
            {
                //// TODO LogManager
                //// if (LogManager.isVerboseLoggingEnabled())
                //// {
                ////     LogManager.d(TAG, "This is a recognized beacon advertisement -- %s seen",
                ////             byteArrayToString(typeCodeBytes));
                //// }
                ////
                //// TODO LogManager
                //// LogManager.d(Tag, "This is a recognized beacon advertisement -- " + 
                ////     getMatchingBeaconTypeCode().ToString("x4") + " seen");
            }

            List<Identifier> identifiers = new List<Identifier>();
            for (int i = 0; i < this.identifierEndOffsets.Count; i++)
            {
                Identifier identifier = Identifier.FromBytes(
                    scanData,
                    this.identifierStartOffsets[i].Value + startByte,
                    this.identifierEndOffsets[i].Value + startByte + 1,
                    this.identifierLittleEndianFlags[i]);

                identifiers.Add(identifier);
            }

            List<long> dataFields = new List<long>();
            for (int i = 0; i < this.dataEndOffsets.Count; i++)
            {
                string dataString = ByteArrayToFormattedString(
                    scanData,
                    this.dataStartOffsets[i].Value + startByte,
                    this.dataEndOffsets[i].Value + startByte,
                    this.dataLittleEndianFlags[i]);

                dataFields.Add(long.Parse(dataString));
                //// TODO LogManager
                //// LogManager.d(Tag, "parsing found data field " + i);
                //// TODO: error handling needed here on the parse
            }

            int txPower = 0;
            string powerString = ByteArrayToFormattedString(
                scanData,
                this.powerStartOffset.Value + startByte,
                this.powerEndOffset.Value + startByte,
                false);
            txPower = int.Parse(powerString);

            // make sure it is a signed integer
            if (txPower > 127)
            {
                txPower -= 256;
            }

            // TODO: error handling needed on the parse
            int beaconTypeCode = 0;
            string beaconTypeString = ByteArrayToFormattedString(
                scanData,
                this.matchingBeaconTypeCodeStartOffset.Value + startByte,
                this.matchingBeaconTypeCodeEndOffset.Value + startByte,
                false);
            beaconTypeCode = int.Parse(beaconTypeString);

            // TODO: error handling needed on the parse
            int manufacturer = 0;
            string manufacturerString =
                ByteArrayToFormattedString(scanData, startByte, startByte + 1, true);
            manufacturer = int.Parse(manufacturerString);

            string macAddress = null;
            string name = null;
            if (device != null)
            {
                macAddress = device.Address;
                name = device.Name;
            }

            return beaconBuilder
                .SetIdentifiers(identifiers)
                .SetDataFields(dataFields)
                .SetTxPower(txPower)
                .SetRssi(rssi)
                .SetBeaconTypeCode(beaconTypeCode)
                .SetBluetoothAddress(macAddress)
                .SetBluetoothAddress(macAddress)
                .SetManufacturer(manufacturer)
                .Build();
        }
        #endregion Protected Methods

        #region BeaconLayoutException
        /// <summary>
        /// Beacon Layout Exception
        /// </summary>
        public class BeaconLayoutException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BeaconLayoutException"/> class.
            /// </summary>
            /// <param name="message">
            /// Error message
            /// </param>
            public BeaconLayoutException(string message)
                : base(message)
            {
            }
        }
        #endregion BeaconLayoutException
    }
}
