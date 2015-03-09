// <copyright file="BeaconParserTest.cs" company="Radius Networks, Inc.">
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
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for BeaconParser
    /// <see cref="AltBeacon.Beacon.BeaconParser"/>
    /// </summary>
    [TestClass]
    public class BeaconParserTest
    {
        /// <summary>
        /// Test Set Beacon Layout. This test tries to access private fields.
        /// When run this test, make those fields protected intnernal (like protected in Java).
        /// </summary>
        [TestMethod]
        public void TestSetBeaconLayout()
        {
            byte[] bytes = HexStringToByteArray("02011a1affbeac2f234454cf6d4a0fadf2f4911ba9ffa600010002c509");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");

            Assert.AreEqual(2, parser.MatchingBeaconTypeCodeStartOffset, "parser should get beacon type code start offset");
            Assert.AreEqual(3, parser.MatchingBeaconTypeCodeEndOffset, "parser should get beacon type code end offset");
            Assert.AreEqual(0xbeacL, parser.MatchingBeaconTypeCode, "parser should get beacon type code");
            ////Assert.AreEqual(4, parser.identifierStartOffsets[0], "parser should get identifier start offset");
            ////Assert.AreEqual(19, parser.identifierEndOffsets[0], "parser should get identifier end offset");
            ////Assert.AreEqual(20, parser.identifierStartOffsets[1], "parser should get identifier start offset");
            ////Assert.AreEqual(21, parser.identifierEndOffsets[1], "parser should get identifier end offset");
            ////Assert.AreEqual(22, parser.identifierStartOffsets[2], "parser should get identifier start offset");
            ////Assert.AreEqual(23, parser.identifierEndOffsets[2], "parser should get identifier end offset");
            ////Assert.AreEqual(24, parser.powerStartOffset, "parser should get power start offset");
            ////Assert.AreEqual(24, parser.powerEndOffset, "parser should get power end offset");
            ////Assert.AreEqual(25, parser.dataStartOffsets[0], "parser should get data start offset");
            ////Assert.AreEqual(25, parser.dataEndOffsets[0], "parser should get data end offset");
        }

        /// <summary>
        /// Test Long To Byte Array Conversion Method.
        /// This test tries to access protected fields so when test it make them public.
        /// </summary>
        [TestMethod]
        public void TestLongToByteArray()
        {
            ////byte[] bytes = BeaconParser.LongToByteArray(10, 1);
            ////Assert.AreEqual(10, bytes[0], "first byte should be 10");
        }

        /// <summary>
        /// Test Recognize Beacon
        /// </summary>
        [TestMethod]
        public void TestRecognizeBeacon()
        {
            byte[] bytes = HexStringToByteArray("02011a1aff1801beac2f234454cf6d4a0fadf2f4911ba9ffa600010002c509");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
            Beacon beacon = parser.FromScanData(bytes, -55, null);

            Assert.AreEqual(-55, beacon.Rssi, "mRssi should be as passed in");
            Assert.AreEqual("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6", beacon.Identifiers[0].ToString(), "uuid should be parsed");
            Assert.AreEqual("1", beacon.Identifiers[1].ToString(), "id2 should be parsed");
            Assert.AreEqual("2", beacon.Identifiers[2].ToString(), "id3 should be parsed");
            Assert.AreEqual(-59, beacon.TxPower, "txPower should be parsed");
            Assert.AreEqual(0x118, beacon.Manufacturer, "manufacturer should be parsed");
        }

        /// <summary>
        /// Test Re-Encode Beacon
        /// </summary>
        [TestMethod]
        public void TestReEncodesBeacon()
        {
            byte[] bytes = HexStringToByteArray("02011a1aff1801beac2f234454cf6d4a0fadf2f4911ba9ffa600010002c509");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
            Beacon beacon = parser.FromScanData(bytes, -55, null);

            byte[] regeneratedBytes = parser.GetBeaconAdvertisementData(beacon);
            byte[] expectedMatch = new byte[bytes.Length - 7];
            Array.Copy(bytes, 7, expectedMatch, 0, expectedMatch.Length);

            CollectionAssert.AreEqual(expectedMatch, regeneratedBytes, "beacon advertisement bytes should be the same after re-encoding");
        }

        /// <summary>
        /// Test Little Endian Identifier Parsing
        /// </summary>
        [TestMethod]
        public void TestLittleEndianIdentifierParsing()
        {
            byte[] bytes = HexStringToByteArray("02011a1aff1801beac0102030405060708090a0b0c0d0e0f1011121314c509");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-9,i:10-15l,i:16-23,p:24-24,d:25-25");
            Beacon beacon = parser.FromScanData(bytes, -55, null);

            Assert.AreEqual(-55, beacon.Rssi, "mRssi should be as passed in");
            Assert.AreEqual("0x010203040506", beacon.Identifiers[0].ToString(), "id1 should be big endian");
            Assert.AreEqual("0x0c0b0a090807", beacon.Identifiers[1].ToString(), "id2 should be little endian");
            Assert.AreEqual("0x0d0e0f1011121314", beacon.Identifiers[2].ToString(), "id3 should be big endian");
            Assert.AreEqual(-59, beacon.TxPower, "txPower should be parsed");
            Assert.AreEqual(0x118, beacon.Manufacturer, "manufacturer should be parsed");
        }

        /// <summary>
        /// Test Re-Encode Little Endian Beacon
        /// </summary>
        [TestMethod]
        public void TestReEncodesLittleEndianBeacon()
        {
            byte[] bytes = HexStringToByteArray("02011a1aff1801beac0102030405060708090a0b0c0d0e0f1011121314c509");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-9,i:10-15l,i:16-23,p:24-24,d:25-25");
            Beacon beacon = parser.FromScanData(bytes, -55, null);

            byte[] regeneratedBytes = parser.GetBeaconAdvertisementData(beacon);
            byte[] expectedMatch = new byte[bytes.Length - 7];
            Array.Copy(bytes, 7, expectedMatch, 0, expectedMatch.Length);

            CollectionAssert.AreEqual(expectedMatch, regeneratedBytes, "beacon advertisement bytes should be the same after re-encoding");
        }

        /// <summary>
        /// Test Recognize Beacon Captured Manufacturer
        /// </summary>
        [TestMethod]
        public void TestRecognizeBeaconCapturedManufacturer()
        {
            byte[] bytes = HexStringToByteArray("0201061affaabbbeace2c56db5dffb48d2b060d0f5a71096e000010004c50000000000000000000000000000000000000000000000000000000000000000");
            BeaconParser parser = new BeaconParser();
            parser.SetBeaconLayout("m:2-3=beac,i:4-19,i:20-21,i:22-23,p:24-24,d:25-25");
            Beacon beacon = parser.FromScanData(bytes, -55, null);

            Assert.AreEqual("bbaa", beacon.Manufacturer.ToString("x4"), "manufacturer should be parsed");
        }

        #region Static Utility Methods
        /// <summary>
        /// Hex to Byte array
        /// </summary>
        /// <param name="s">
        /// hex string
        /// </param>
        /// <returns>
        /// byte array
        /// </returns>
        private static byte[] HexStringToByteArray(string s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte)((Convert.ToInt32(s[i].ToString(), 16) << 4)
                    + Convert.ToInt32(s[i + 1].ToString(), 16));
            }

            return data;
        }

        /// <summary>
        /// byte array to hex
        /// </summary>
        /// <param name="bytes">
        /// byte array
        /// </param>
        /// <returns>
        /// hex string
        /// </returns>
        private static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
        #endregion Static Utility Methods
    }
}
