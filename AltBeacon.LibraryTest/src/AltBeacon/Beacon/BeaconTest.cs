// <copyright file="BeaconTest.cs" company="Radius Networks, Inc.">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Beacon
    /// <see cref="AltBeacon.Beacon.Beacon"/>
    /// </summary>
    [TestClass]
    public class BeaconTest
    {
        /// <summary>
        /// Beacon Identifiers Test
        /// </summary>
        [TestMethod]
        public void TestAccessBeaconIdentifiers()
        {
            Beacon beacon = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Assert.AreEqual("1", beacon.Identifiers[0].ToString(), "First beacon id should be 1");
            Assert.AreEqual("2", beacon.Identifiers[1].ToString(), "Second beacon id should be 1");
            Assert.AreEqual("3", beacon.Identifiers[2].ToString(), "Third beacon id should be 1");
            Assert.AreEqual("1", beacon.Id1.ToString(), "First beacon id should be 1");
            Assert.AreEqual("2", beacon.Id2.ToString(), "Second beacon id should be 1");
            Assert.AreEqual("3", beacon.Id3.ToString(), "Third beacon id should be 1");
        }

        /// <summary>
        /// Two Beacon with same Identifiers Test
        /// </summary>
        [TestMethod]
        public void TestBeaconsWithSameIdentifersAreEqual()
        {
            Beacon beacon1 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();
            
            Beacon beacon2 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Assert.AreEqual(beacon1, beacon2, "Beacons with same identifiers should be equal");
        }

        /// <summary>
        /// Two Beacon with only first Identifier different Test
        /// </summary>
        [TestMethod]
        public void TestBeaconsWithDifferentId1AreNotEqual()
        {
            Beacon beacon1 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();
            
            Beacon beacon2 = new Beacon.Builder().SetId1("11").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Assert.IsTrue(!beacon1.Equals(beacon2), "Beacons with different id1 are not equal");
        }

        /// <summary>
        /// Two Beacon with only second Identifier different Test
        /// </summary>
        [TestMethod]
        public void TestBeaconsWithDifferentId2AreNotEqual()
        {
            Beacon beacon1 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Beacon beacon2 = new Beacon.Builder().SetId1("1").SetId2("12").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Assert.IsTrue(!beacon1.Equals(beacon2), "Beacons with different id2 are not equal");
        }

        /// <summary>
        /// Two Beacon with only third Identifier different Test
        /// </summary>
        [TestMethod]
        public void TestBeaconsWithDifferentId3AreNotEqual()
        {
            Beacon beacon1 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("3").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Beacon beacon2 = new Beacon.Builder().SetId1("1").SetId2("2").SetId3("13").SetRssi(4)
                .SetBeaconTypeCode(5).SetTxPower(6).SetBluetoothAddress("1:2:3:4:5:6").Build();

            Assert.IsTrue(!beacon1.Equals(beacon2), "Beacons with different id3 are not equal");
        }

        ////TODO ModelSpecificDistanceCalculator
        /*[TestMethod]
        public void TestCalculateAccuracyWithRssiEqualsPower()
        {
            Beacon.DistanceCalculator = new ModelSpecificDistanceCalculator(null, null);
            double accuracy = Beacon.CalculateDistance(-55, -55);
            Assert.AreEqual(1.0, accuracy, 0.1, "Distance should be one meter if mRssi is the same as power");
        }
        [TestMethod]
        public void TestCalculateAccuracyWithRssiGreaterThanPower()
        {
            Beacon.DistanceCalculator = new ModelSpecificDistanceCalculator(null, null);
            double accuracy = Beacon.CalculateDistance(-55, -50);
            Assert.IsTrue(accuracy < 1.0, "Distance should be under one meter if mRssi is less negative than power. Accuracy was " + accuracy);
        }
        [TestMethod]
        public void TestCalculateAccuracyWithRssiLessThanPower()
        {
            Beacon.DistanceCalculator = new ModelSpecificDistanceCalculator(null, null);
            double accuracy = Beacon.CalculateDistance(-55, -60);
            Assert.IsTrue(accuracy > 1.0, "Distance should be over one meter if mRssi is less negative than power. Accuracy was " + accuracy);
        }
        [TestMethod]
        public void TestCalculateAccuracyWithRssiEqualsPowerOnInternalProperties()
        {
            Beacon.DistanceCalculator = new ModelSpecificDistanceCalculator(null, null);
            Beacon beacon = new Beacon.Builder().SetTxPower(-55).SetRssi(-55).Build();
            double distance = beacon.Distance;
            Assert.AreEqual(distance, 1.0, 0.1, "Distance should be one meter if mRssi is the same as power");
        }
        [TestMethod]
        public void TestCalculateAccuracyWithRssiEqualsPowerOnInternalPropertiesAndRunningAverage()
        {
            Beacon.DistanceCalculator = new ModelSpecificDistanceCalculator(null, null);
            Beacon beacon = new Beacon.Builder().SetTxPower(-55).SetRssi(0).Build();
            beacon.RunningAverageRssi = -55;
            double distance = beacon.Distance;
            Assert.AreEqual(distance, 1.0, 0.1, "Distance should be one meter if mRssi is the same as power");
        }*/
    }
}
