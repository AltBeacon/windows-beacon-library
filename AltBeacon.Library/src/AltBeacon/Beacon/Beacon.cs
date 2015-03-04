// <copyright file="Beacon.cs" company="Radius Networks, Inc.">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    /// <summary>
    /// <para>
    /// The <code>Beacon</code> class represents a single hardware Beacon detected by
    /// an Android device.
    /// </para>
    /// <para>
    /// A Beacon is identified by a unique multi-part identifier, with the first of the ordered
    /// identifiers being more significant for the purposes of grouping beacons.
    /// </para>
    /// <para>
    /// An Beacon sends a Bluetooth Low Energy (BLE) advertisement that contains these
    /// three identifiers, along with the calibrated tx power (in RSSI) of the 
    /// Beacon's Bluetooth transmitter.  
    /// </para>
    /// <para>
    /// This class may only be instantiated from a BLE packet, and an RSSI measurement for
    /// the packet.  The class parses out the identifier, along with the calibrated
    /// tx power.  It then uses the measured RSSI and calibrated tx power to do a rough
    /// distance measurement (the mDistance field)
    /// </para>
    /// </summary>
    /// <see cref="AltBeacon.Beacon.Region#MatchesBeacon(Beacon)"/>
    /// <author>
    ///     David G. Young
    /// </author>
    public class Beacon
    {
        /// <summary>
        /// Logger Tag
        /// </summary>
        private const string Tag = "Beacon";

        /// TODO DistanceCalculator
        /*protected static DistanceCalculator sDistanceCalculator = null;*/
        
        /// <summary>
        /// The a list of the multi-part identifiers of the beacon. Together, 
        /// these identifiers signify a unique beacon. The identifiers are ordered 
        /// by significance for the purpose of grouping beacons.
        /// </summary>
        private List<Identifier> identifiers;

        /// <summary>
        /// A list of generic non-identifying data fields included in the beacon advertisement. 
        /// Data fields are limited to the size of a long, or six bytes.
        /// </summary>
        private List<long> dataFields;

        /// <summary>
        /// A double that is an estimate of how far the Beacon is away in meters. 
        /// Note that this number fluctuates quite a bit with RSSI,
        /// so despite the name, it is not super accurate.
        /// </summary>
        private double? distance;

        /// <summary>
        /// The running average rssi for use in distance calculations.
        /// </summary>
        private double? runningAverageRssi = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Beacon"/> class.
        /// Basic constructor that simply allocates fields
        /// </summary>
        protected Beacon()
        {
            this.identifiers = new List<Identifier>(1);
            this.dataFields = new List<long>(1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Beacon"/> class.
        /// Copy constructor.
        /// </summary>
        /// <param name="otherBeacon">
        /// Other beacon to be copied
        /// </param>
        /// <see cref="Beacon" />
        protected Beacon(Beacon otherBeacon)
        {
            this.identifiers = new List<Identifier>(otherBeacon.identifiers);
            this.dataFields = new List<long>(otherBeacon.dataFields);
            this.distance = otherBeacon.distance;
            this.runningAverageRssi = otherBeacon.runningAverageRssi;
            this.Rssi = otherBeacon.Rssi;
            this.TxPower = otherBeacon.TxPower;
            this.BluetoothAddress = otherBeacon.BluetoothAddress;
            this.BeaconTypeCode = otherBeacon.BeaconTypeCode;
            this.ServiceUuid = otherBeacon.ServiceUuid;
            this.BluetoothName = otherBeacon.BluetoothName;
        }

        /// <summary>
        /// Gets the list of identifiers transmitted with the advertisement.
        /// </summary>
        public ReadOnlyCollection<Identifier> Identifiers
        {
            get
            {
                return new ReadOnlyCollection<Identifier>(this.identifiers);
            }
        }

        /// <summary>
        /// Gets the first identifier (Convenience method).
        /// </summary>
        public Identifier Id1
        {
            get
            {
                return this.identifiers[0];
            }
        }

        /// <summary>
        /// Gets the second identifier (Convenience method).
        /// </summary>
        public Identifier Id2
        {
            get
            {
                return this.identifiers[1];
            }
        }

        /// <summary>
        /// Gets the third identifier (Convenience method).
        /// </summary>
        public Identifier Id3
        {
            get
            {
                return this.identifiers[2];
            }
        }

        /// <summary>
        /// Gets the list of data fields transmitted with the advertisement.
        /// </summary>
        public ReadOnlyCollection<long> DataFields
        {
            get
            {
                return new ReadOnlyCollection<long>(this.dataFields);
            }
        }

        /// <summary>
        /// Gets or sets the calibrated measured Tx power of the Beacon in RSSI
        /// This value is baked into an Beacon when it is manufactured, and
        /// it is transmitted with each packet to aid in the mDistance estimate
        /// </summary>
        public int TxPower { get; protected set; }

        /// <summary>
        /// Gets or sets the measured signal strength of the Bluetooth packet that led 
        /// do this Beacon detection.
        /// </summary>
        public int Rssi { get; set; }

        /// <summary>
        /// Gets or sets the running average rssi for use in distance calculations.
        /// </summary>
        public double? RunningAverageRssi
        {
            get
            {
                return this.runningAverageRssi;
            }

            set
            {
                this.runningAverageRssi = value;

                // force calculation of accuracy and proximity next time they are requested
                this.distance = null;
            }
        }

        /// <summary>
        /// Gets a calculated estimate of the distance to the beacon based on
        /// a running average of the RSSI and the transmitted power calibration
        /// value included in the beacon advertisement. This value is specific
        /// to the type of Android device receiving the transmission.
        /// </summary>
        public double Distance
        {
            get
            {
                if (this.distance == null)
                {
                    double bestRssiAvailable = this.Rssi;
                    if (this.RunningAverageRssi != null)
                    {
                        bestRssiAvailable = this.RunningAverageRssi.Value;
                    }
                    ////TODO LogManager
                    ////else
                    ////{
                    ////    LogManager.d(Tag, "Not using running average RSSI because it is null");
                    ////}

                    this.distance = CalculateDistance(this.TxPower, bestRssiAvailable);
                }

                return this.distance.Value;
            }
        }

        /// <summary>
        /// Used to attach data to individual Beacons, either locally or in the cloud
        /// </summary>
        /// TODO BeaconDataFactory
        /*protected static BeaconDataFactory beaconDataFactory = new NullBeaconDataFactory();*/

        /// <summary>
        /// Gets or sets the two byte value indicating the type of beacon that this is, 
        /// which is used for figuring out the byte layout of the beacon advertisement.
        /// </summary>
        public int BeaconTypeCode { get; protected set; }

        /// <summary>
        /// Gets or sets the bluetooth mac address.
        /// </summary>
        public string BluetoothAddress { get; protected set; }

        /// <summary>
        /// Gets or sets the bluetooth device name. This is a field transmitted by 
        /// the remote beacon device separate from the advertisement data
        /// </summary>
        public string BluetoothName { get; protected set; }

        /// <summary>
        /// <para>
        /// Gets or sets a two byte code indicating the beacon manufacturer. 
        /// A list of registered manufacturer codes may be found here:
        /// https://www.bluetooth.org/en-us/specification/assigned-numbers/company-identifiers
        /// </para>
        /// <para>
        /// If the beacon is a GATT-based beacon, this field will be set to -1
        /// </para>
        /// </summary>
        public int Manufacturer { get; protected set; }

        /// <summary>
        /// <para>
        /// Gets or sets a 32 bit service uuid for the beacon
        /// </para>
        /// <para>
        /// This is valid only for GATT-based beacons. If the beacon is a
        /// manufacturer data-based beacon, this field will be -1
        /// </para>
        /// </summary>
        public int ServiceUuid { get; protected set; }


        /// <summary>
        /// Gets and sets the DistanceCalculator to use with this beacon.
        /// </summary>
        /// TODO DistanceCalculator
        ////public static DistanceCalculator DistanceCalculator { get; set; }

        /// <summary>
        /// Requests server-side data for this beacon. Requires that a
        /// BeaconDataFactory be set up with a backend service.
        /// </summary>
        /// <param name="notifier">
        /// Interface providing a callback when data are available.
        /// </param>
        /// TODO BeaconDataNotifier
        /*public void requestData(BeaconDataNotifier notifier)
        {
            beaconDataFactory.requestBeaconData(this, notifier);
        }*/

        /// <summary>
        /// Two detected beacons are considered equal if they share the same three identifiers,
        /// regardless of their mDistance or RSSI.
        /// </summary>
        /// <param name="other">
        /// Other Beacon.
        /// </param>
        /// <returns>
        /// <code>true</code> if two beacons are equal, <code>false</code> otherwise.
        /// </returns>
        public override bool Equals(object other)
        {
            if (!(other is Beacon))
            {
                return false;
            }

            Beacon thatBeacon = (Beacon)other;
            if (this.identifiers.Count != thatBeacon.identifiers.Count)
            {
                return false;
            }

            // all identifiers must match
            for (int i = 0; i < this.identifiers.Count; i++)
            {
                if (!this.identifiers[i].Equals(thatBeacon.identifiers[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculate a hashCode for this beacon.
        /// </summary>
        /// <returns>
        /// Calculated Hash Code.
        /// </returns>
        public override int GetHashCode()
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (Identifier identifier in this.identifiers)
            {
                sb.Append("id");
                sb.Append(i);
                sb.Append(": ");
                sb.Append(identifier.ToString());
                sb.Append(" ");
                i++;
            }

            return sb.ToString().GetHashCode();
        }
        
        /// <summary>
        /// Formats a beacon as a string showing only its unique identifiers.
        /// </summary>
        /// <returns>
        /// Formatted String.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (Identifier identifier in this.identifiers)
            {
                if (i > 1)
                {
                    sb.Append(" ");
                }

                sb.Append("id");
                sb.Append(i);
                sb.Append(": ");
                sb.Append(identifier == null ? "null" : identifier.ToString());
                i++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Estimate the distance to the beacon using the DistanceCalculator set on this class.
        /// If no DistanceCalculator has been set, return -1 as the distance.
        /// </summary>
        /// <see cref="Altbeacon.Beacon.Distance.DistanceCalculator"/>
        /// <param name="txPower">
        /// Beacon TxPower.
        /// </param>
        /// <param name="bestRssiAvailable">
        /// Best Rssi Available (average rssi or current rssi).
        /// </param>
        /// <returns>
        /// Calculated distance.
        /// </returns>
        protected static double CalculateDistance(int txPower, double bestRssiAvailable)
        {
            ////if (Beacon.DistanceCalculator != null)
            ////{
            ////    return Beacon.DistanceCalculator.CalculateDistance(txPower, bestRssiAvailable);
            ////}
            ////else
            ////{
                ////TODO LogManager
                ////LogManager.e(Tag, "Distance calculator not set.  Distance will bet set to -1");
                return -1.0;
            ////}
        }

        /// <summary>
        /// <para>
        /// Builder class for Beacon objects. Provides a convenient way to set the various fields of a
        /// Beacon
        /// </para>
        /// <para>
        /// Example:
        /// <code>
        /// Beacon beacon = new Beacon.Builder()
        ///         .setId1(&quot;2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6&quot;)
        ///         .setId2("1")
        ///         .setId3("2")
        ///         .build();
        /// </code>
        /// </para>
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// Beacon object.
            /// </summary>
            private Beacon beacon;

            /// <summary>
            /// Identifiers first, second and the third.
            /// </summary>
            private Identifier id1, id2, id3;

            /// <summary>
            /// Initializes a new instance of the <see cref="Beacon.Builder"/> class.
            /// Creates a builder instance.
            /// </summary>
            public Builder()
            {
                this.beacon = new Beacon();
            }

            /// <summary>
            /// Builds an instance of this beacon based on parameters set in the Builder.
            /// </summary>
            /// <returns>
            /// Returns built Beacon.
            /// </returns>
            public Beacon Build()
            {
                if (this.id1 != null)
                {
                    this.beacon.identifiers.Add(this.id1);
                    if (this.id2 != null)
                    {
                        this.beacon.identifiers.Add(this.id2);
                        if (this.id3 != null)
                        {
                            this.beacon.identifiers.Add(this.id3);
                        }
                    }
                }

                return this.beacon;
            }

            /// <summary>
            /// Sets Identifiers.
            /// <see cref="Beacon#Identifiers"/>
            /// </summary>
            /// <param name="identifiers">
            /// Identifiers to set
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetIdentifiers(List<Identifier> identifiers)
            {
                this.id1 = null;
                this.id2 = null;
                this.id3 = null;
                this.beacon.identifiers = new List<Identifier>(identifiers);

                return this;
            }

            /// <summary>
            /// Convenience method allowing the first beacon identifier to be set as a String.
            /// It will be parsed into an Identifier object.
            /// </summary>
            /// <param name="id1String">
            /// String to parse into an identifier
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetId1(string id1String)
            {
                this.id1 = Identifier.Parse(id1String);
                return this;
            }

            /// <summary>
            /// Convenience method allowing the second beacon identifier to be set as a String.
            /// It will be parsed into an Identifier object.
            /// </summary>
            /// <param name="id2String">
            /// String to parse into an identifier
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetId2(string id2String)
            {
                this.id2 = Identifier.Parse(id2String);
                return this;
            }

            /// <summary>
            /// Convenience method allowing the third beacon identifier to be set as a String.
            /// It will be parsed into an Identifier object.
            /// </summary>
            /// <param name="id3String">
            /// String to parse into an identifier
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetId3(string id3String)
            {
                this.id3 = Identifier.Parse(id3String);
                return this;
            }

            /// <summary>
            /// Sets Data Fields.
            /// <see cref="Beacon#DataFields"/>
            /// </summary>
            /// <param name="dataFields">
            /// Data field list.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetDataFields(List<long> dataFields)
            {
                this.beacon.dataFields = new List<long>(dataFields);
                return this;
            }

            /// <summary>
            /// Sets TxPower.
            /// <see cref="Beacon#TxPower"/>
            /// </summary>
            /// <param name="txPower">
            /// TxPower value.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetTxPower(int txPower)
            {
                this.beacon.TxPower = txPower;
                return this;
            }

            /// <summary>
            /// Sets Rssi.
            /// <see cref="Beacon#Rssi"/>
            /// </summary>
            /// <param name="rssi">
            /// Rssi value.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetRssi(int rssi)
            {
                this.beacon.Rssi = rssi;
                return this;
            }

            /// <summary>
            /// Sets Manufacturer code.
            /// <see cref="Beacon#Manufacturer"/>
            /// </summary>
            /// <param name="manufacturer">
            /// Manufacturer Code.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetManufacturer(int manufacturer)
            {
                this.beacon.Manufacturer = manufacturer;
                return this;
            }

            /// <summary>
            /// Sets Bluetooth Address.
            /// <see cref="Beacon#BluetoothAddress"/>
            /// </summary>
            /// <param name="bluetoothAddress">
            /// Bluetooth Address.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetBluetoothAddress(string bluetoothAddress)
            {
                this.beacon.BluetoothAddress = bluetoothAddress;
                return this;
            }

            /// <summary>
            /// Sets Bluetooth Name.
            /// <see cref="Beacon#BluetoothName"/>
            /// </summary>
            /// <param name="name">
            /// Bluetooth Name.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetBluetoothName(string name)
            {
                this.beacon.BluetoothName = name;
                return this;
            }

            /// <summary>
            /// Sets Beacon Type Code.
            /// <see cref="Beacon#BeaconTypeCode"/>
            /// </summary>
            /// <param name="beaconTypeCode">
            /// Beacon Type Code value.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetBeaconTypeCode(int beaconTypeCode)
            {
                this.beacon.BeaconTypeCode = beaconTypeCode;
                return this;
            }

            /// <summary>
            /// Sets Service Uuid.
            /// <see cref="Beacon#ServiceUuid"/>
            /// </summary>
            /// <param name="serviceUuid">
            /// Service Uuid value.
            /// </param>
            /// <returns>
            /// Builder reference
            /// </returns>
            public Builder SetServiceUuid(int serviceUuid)
            {
                this.beacon.ServiceUuid = serviceUuid;
                return this;
            }
        }
    }
}
