// <copyright file="IdentifierTest.cs" company="Radius Networks, Inc.">
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
namespace AltBeacon.Beacon
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Identifier
    /// <see cref="AltBeacon.Beacon.Identifier"/>
    /// </summary>
    [TestClass]
    public class IdentifierTest
    {
        /// <summary>
        /// Equals Must Ignore Case Test
        /// </summary>
        [TestMethod]
        public void TestEqualsNormalizationIgnoresCase()
        {
            Identifier identifier1 = Identifier.Parse("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6");
            Identifier identifier2 = Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6");
            Assert.IsTrue(identifier1.Equals(identifier2), "Identifiers of different case should match");
        }

        /// <summary>
        /// To String Must Ignore Case Test
        /// </summary>
        [TestMethod]
        public void TestToStringNormalizesCase()
        {
            Identifier identifier1 = Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6");
            Assert.AreEqual("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6", identifier1.ToString(), "Identifiers of different case should match");
        }

        /// <summary>
        /// To UUID String Test
        /// </summary>
        [TestMethod]
        public void TestToStringEqualsUuid()
        {
            Identifier identifier1 = Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6");
            Assert.AreEqual("2f234454-cf6d-4a0f-adf2-f4911ba9ffa6", identifier1.ToUuidString(), "uuidString of Identifier should match");
        }

        /// <summary>
        /// UUID to byte array conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayConvertsUuids()
        {
            Identifier identifier1 = Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6");
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(true);
            Assert.AreEqual(16, bytes.Length, "byte array is correct length");
            Assert.AreEqual(0x2f, bytes[0] & 0xFF, "first byte of uuid converted properly");
            Assert.AreEqual(0x23, bytes[1] & 0xFF, "second byte of uuid converted properly");
            Assert.AreEqual(0xa6, bytes[15] & 0xFF, "last byte of uuid converted properly");
        }

        /// <summary>
        /// UUID to byte array as Little Endian conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayConvertsUuidsAsLittleEndian()
        {
            Identifier identifier1 = Identifier.Parse("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6");
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(false);
            Assert.AreEqual(16, bytes.Length, "byte array is correct length");
            Assert.AreEqual(0xa6, bytes[0] & 0xFF, "first byte of uuid converted properly");
            Assert.AreEqual(0x2f, bytes[15] & 0xFF, "last byte of uuid converted properly");
        }

        /// <summary>
        /// Hex to byte array conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayConvertsHex()
        {
            Identifier identifier1 = Identifier.Parse("0x010203040506");
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(true);
            Assert.AreEqual(6, bytes.Length, "byte array is correct length");
            Assert.AreEqual(0x01, bytes[0] & 0xFF, "first byte of hex converted properly");
            Assert.AreEqual(0x06, bytes[5] & 0xFF, "last byte of hex converted properly");
        }

        /// <summary>
        /// Decimal to byte array conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayConvertsDecimal()
        {
            Identifier identifier1 = Identifier.Parse("65534");
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(true);
            Assert.AreEqual(2, bytes.Length, "byte array is correct length");
            Assert.AreEqual(2, identifier1.ByteCount, "reported byte array is correct length");
            Assert.AreEqual(0xff, bytes[0] & 0xFF, "first byte of decimal converted properly");
            Assert.AreEqual(0xfe, bytes[1] & 0xFF, "last byte of decimal converted properly");
        }

        /// <summary>
        /// Decimal to byte array and to integer conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayConvertsInt()
        {
            Identifier identifier1 = Identifier.FromInt(65534);
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(true);
            Assert.AreEqual(2, bytes.Length, "byte array is correct length");
            Assert.AreEqual(2, identifier1.ByteCount, "reported byte array is correct length");
            Assert.AreEqual(65534, identifier1.ToInt(), "conversion back equals original value");
            Assert.AreEqual(0xff, bytes[0] & 0xFF, "first byte of decimal converted properly");
            Assert.AreEqual(0xfe, bytes[1] & 0xFF, "last byte of decimal converted properly");
        }

        /// <summary>
        /// Byte array to byte array and to string conversion Test
        /// </summary>
        [TestMethod]
        public void TestToByteArrayFromByteArray()
        {
            byte[] value = new byte[] { (byte)0xFF, (byte)0xAB, 0x12, 0x25 };
            Identifier identifier1 = Identifier.FromBytes(value, 0, value.Length, false);
            byte[] bytes = identifier1.ToByteArrayOfSpecifiedEndianness(true);
            Assert.AreEqual(4, bytes.Length, "byte array is correct length");
            Assert.AreEqual("0xffab1225", identifier1.ToString(), "correct string representation");
            Assert.IsTrue(Enumerable.SequenceEqual(value, bytes), "arrays equal");
            Assert.AreNotSame(bytes, value, "arrays are copied");
        }

        /// <summary>
        /// Compare two identifier Test with different length
        /// </summary>
        [TestMethod]
        public void TestComparableDifferentLength()
        {
            byte[] value1 = new byte[] { (byte)0xFF, (byte)0xAB, 0x12, 0x25 };
            Identifier identifier1 = Identifier.FromBytes(value1, 0, value1.Length, false);
            byte[] value2 = new byte[] { (byte)0xFF, (byte)0xAB, 0x12, 0x25, 0x11, 0x11 };
            Identifier identifier2 = Identifier.FromBytes(value2, 0, value2.Length, false);
            Assert.AreEqual(-1, identifier1.CompareTo(identifier2), "identifier1 is smaller than identifier2");
            Assert.AreEqual(1, identifier2.CompareTo(identifier1), "identifier2 is larger than identifier1");
        }

        /// <summary>
        /// Compare two identifier Test with same length
        /// </summary>
        [TestMethod]
        public void TestComparableSameLength()
        {
            byte[] value1 = new byte[] { (byte)0xFF, (byte)0xAB, 0x12, 0x25, 0x22, 0x25 };
            Identifier identifier1 = Identifier.FromBytes(value1, 0, value1.Length, false);
            byte[] value2 = new byte[] { (byte)0xFF, (byte)0xAB, 0x12, 0x25, 0x11, 0x11 };
            Identifier identifier2 = Identifier.FromBytes(value2, 0, value2.Length, false);
            Assert.AreEqual(0, identifier1.CompareTo(identifier1), "identifier1 is equal to identifier1");
            Assert.AreEqual(1, identifier1.CompareTo(identifier2), "identifier1 is larger than identifier2");
            Assert.AreEqual(-1, identifier2.CompareTo(identifier1), "identifier2 is smaller than identifier1");
        }
    }
}
