namespace Altbeacon.Beacon
{
    using System;
    using Windows.Devices.Bluetooth.Advertisement;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Devices.Bluetooth;

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
    public class BeaconTransmitter
    {
        public Beacon Beacon;
        private BeaconParser BeaconParser;
        private BluetoothLEAdvertisementPublisher Publisher = null;
        public Boolean Started = false;

        public BeaconTransmitter(BeaconParser parser) {
          this.BeaconParser = parser;
        }
        public void StartAdvertising(Beacon beacon) {
          this.Beacon = beacon;
          StartAdvertising();
        }
       public void StopAdvertising()
        {
            if (Publisher != null && Started)
            {
                Publisher.Stop();
                this.Started = false;
            }
        }
        private void StartAdvertising() {
            StopAdvertising();
            Publisher = new BluetoothLEAdvertisementPublisher();

            var advertisingBytes = this.BeaconParser.GetBeaconAdvertisementData(Beacon);

            if (this.BeaconParser.ServiceUuid != null)

            {
                var advertisingBytesWithoutServiceUuid = new byte[advertisingBytes.Length - 2];
                Array.Copy(advertisingBytes, 2, advertisingBytesWithoutServiceUuid, 0, advertisingBytes.Length - 2);
                //{0000feaa-0000-1000-8000-00805f9b34fb}
                var uuidString = String.Format("0000{0:X4}00001000800000805F9B34FB", this.BeaconParser.ServiceUuid);
                Guid guid = Guid.Parse(uuidString);
                //Publisher.Advertisement.ServiceUuids.Add(guid);
                var serviceUuidDataSection = new BluetoothLEAdvertisementDataSection();
                byte[] serviceUuidBytes = new byte[2];
                serviceUuidBytes[1] = (byte) (this.BeaconParser.ServiceUuid >> 8);
                serviceUuidBytes[0] = (byte)(this.BeaconParser.ServiceUuid & 0xff);
                serviceUuidDataSection.Data = serviceUuidBytes.AsBuffer();
                serviceUuidDataSection.DataType = 3; // Found this by scanning Eddystone-UID

                var dataSection = new BluetoothLEAdvertisementDataSection();
                dataSection.Data = advertisingBytes.AsBuffer();
                dataSection.DataType = 22; // Found this by scanning Eddystone-UID
                Publisher.Advertisement.DataSections.Add(dataSection);
            }
            else
            {
                // if manufacturer advertisement
                var manufacturerData = new BluetoothLEManufacturerData();
                manufacturerData.CompanyId = (ushort)Beacon.Manufacturer;
                manufacturerData.Data = advertisingBytes.AsBuffer();
                Publisher.Advertisement.ManufacturerData.Add(manufacturerData);
            }

            Publisher.StatusChanged += OnPublisherStatusChanged;
            /*
            Publisher.Advertisement.Flags;
            */

            Publisher.Start();
            this.Started = true;
        }

        private async void OnPublisherStatusChanged(
                    BluetoothLEAdvertisementPublisher publisher,
                    BluetoothLEAdvertisementPublisherStatusChangedEventArgs eventArgs)
        {
            BluetoothLEAdvertisementPublisherStatus status = eventArgs.Status;
            if (status == BluetoothLEAdvertisementPublisherStatus.Started)
            {
                Logger.Debug("Transmitter status changed to STARTED");

            }
            else if (status == BluetoothLEAdvertisementPublisherStatus.Stopped)
            {
                Logger.Debug("Transmitter status changed to STOPPED");
            }
            else if (status == BluetoothLEAdvertisementPublisherStatus.Aborted)
            {
                Logger.Error("Transmitter status changed to ABORTED");
            }
            else
            {
                Logger.Error("Transmitter status changed to intermediate value: "+status);
            }
            BluetoothError error = eventArgs.Error;
            if (error.ToString() != "Success")
            {
                Logger.Error("Transmitter start error: " + error);
            }
        }

    }
}
