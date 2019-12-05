# Windows Beacon Library

This project provides tools for working with bluetooth proximity beacons on Windows 10.  The code is ported from the Android Beacon Library, and while far less complete it
does work for basic beacon transmission and detection use cases.

There is currently no binary distribution so you have to compile this yourself and link it in with your project, or copy the code manually.

Much work remains to be done to make this as complete at the Android Beacon Library.  If you'd like to help, please open an issue on the project or dive right in with a pull request.

## Sample Code:

### Transmit Eddystone-UID

```
using Altbeacon;
using Altbeacon.Beacon;

BeaconTransmitter = new BeaconTransmitter(new BeaconParser().SetBeaconLayout(BeaconParser.EddystoneUidLayout));
List<long> dataFields = new List<long>();
dataFields.Add(0);
Beacon beacon = new Beacon.Builder()
	.SetTxPower(-59).SetDataFields(dataFields)
	.SetId1("0x00112233445566778899")
	.SetId2("0xAABBCCDDEEFF").Build();
BeaconTransmitter.StartAdvertising(beacon);
```

### Transmit iBeacon

```
using Altbeacon;
using Altbeacon.Beacon;

BeaconTransmitter = new BeaconTransmitter(new BeaconParser().SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24"));
Beacon beacon = new Beacon.Builder()
	.SetTxPower(-59)
	.SetManufacturer(0x004c)
	.SetId1("2F234454-CF6D-4A0F-ADF2-F4911BA9FFA6" /* proximity uuid */)
	.SetId2("1" /* major */)
	.SetId3("1" /* minor */).Build();
BeaconTransmitter.StartAdvertising(beacon);
```

### Basic Beacon Scanning and Detection

```
using Altbeacon;
using Altbeacon.Beacon;

private static BluetoothLEAdvertisementWatcher Watcher = new BluetoothLEAdvertisementWatcher();

private startLookingForBeacons()
{
  Watcher.Received += Watcher_Received;
  OLogger.Debug("Starting scan");
  Watcher.Start();
}

private static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
{
	if (args.Advertisement.ManufacturerData.Count() > 0) {
		// Parse iBeacon
		var buffer = args.Advertisement.ManufacturerData.ElementAt(0).Data;
		DataReader dataReader = DataReader.FromBuffer(buffer);
		byte[] bytes = new byte[buffer.Length];
		dataReader.ReadBytes(bytes);
		var beaconParser = new BeaconParser();
		beaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");
		Beacon beacon = beaconParser.FromAdvertisement(args.Advertisement, args.RawSignalStrengthInDBm, args.BluetoothAddress);
		if (beacon != null)
		{
			OLogger.Debug("Found iBeacon: UUID=" + beacon.Id1 + " major=" + beacon.Id2 + " minor=" + beacon.Id3 + " rssi: " + beacon.Rssi);
		}

	}
}
```

###  Bigger Example with Distance Estimates and Eddystone.  (Getting distance estimates requires using RssiTracker)

```
using Altbeacon;
using Altbeacon.Beacon;

private static BluetoothLEAdvertisementWatcher Watcher = new BluetoothLEAdvertisementWatcher();

private startLookingForBeacons() {
  Watcher.Received += Watcher_Received;
  OLogger.Debug("Starting scan");
  Watcher.Start();
}

private static String GetMac(ulong address)
{
	String mac = "";
		for (int i = 5; i >= 0; i--)
		{
			if (i < 5)
			{
				mac += ":";
			}
			mac += String.Format("{0:X2}",(address >> i & 0xff));
		}
	return mac;

}

private static void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
{
	RssiTracker.Add(GetMac(args.BluetoothAddress), args.RawSignalStrengthInDBm);

	Beacon beacon = null;
	if (args.Advertisement.ManufacturerData.Count() > 0) {
		// Parse iBeacon
		var buffer = args.Advertisement.ManufacturerData.ElementAt(0).Data;
		DataReader dataReader = DataReader.FromBuffer(buffer);
		byte[] bytes = new byte[buffer.Length];
		dataReader.ReadBytes(bytes);
		var beaconParser = new BeaconParser();
		beaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");
		beacon = beaconParser.FromAdvertisement(args.Advertisement, args.RawSignalStrengthInDBm, args.BluetoothAddress);
			if (beacon != null)
			{
				OLogger.Debug("Found iBeacon: UUID=" + beacon.Id1 + " major=" + beacon.Id2 + " minor=" + beacon.Id3 + " distance: " + beacon.Distance);
			}

	}
	else
	{
		// Parse Eddystone UID
		if (args.Advertisement.ServiceUuids.Count > 0)
		{
			var beaconParser = new BeaconParser();
			beaconParser.SetBeaconLayout(BeaconParser.EddystoneUidLayout);
			beacon = beaconParser.FromAdvertisement(args.Advertisement, args.RawSignalStrengthInDBm, args.BluetoothAddress);
			if (beacon != null)
			{
				OLogger.Debug("Found an eddystone beacon: " + beacon.Id1 + " " + beacon.Id2 + " distance: " + beacon.Distance);
			}
		}
	}
}
````

## License

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

This software is available under the Apache License 2.0, allowing you to use the library in your applications.
