using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altbeacon.Beacon
{
    public class Region
    {
        private String uniqueId = null;
        /// <summary>
        /// The a list of the multi-part identifiers of the beacon. Together, 
        /// these identifiers signify a unique beacon. The identifiers are ordered 
        /// by significance for the purpose of grouping beacons.
        /// </summary>
        private List<Identifier> identifiers;

        public String BluetoothAddress
        {
            get;
        }

        private Region()
        {

        }

        public Region(String uniqueId, Identifier id1, Identifier id2, Identifier id3)
        {
            this.uniqueId = uniqueId;
            this.identifiers = new List<Identifier>();
            identifiers.Add(id1);
            identifiers.Add(id2);
            identifiers.Add(id3);
        }

        public Region(String uniqueId, List<Identifier> identifiers)
        {
            this.uniqueId = uniqueId;
            this.identifiers = identifiers;
        }

        public Region(String uniqueId, String bluetoothAddress)
        {
            this.uniqueId = uniqueId;
            this.identifiers = new List<Identifier>();
            this.BluetoothAddress = bluetoothAddress;
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

        public String UniqueId
        {
            get
            {
                return uniqueId;
            }
        }

        /// <summary>
        /// Two regionsare considered equal if their identifier is equal
        /// </summary>
        override public bool Equals(Object other)
        {
            if (other == null || !(other is Region))
            {
                return false;
            }

            return ((Region)other).uniqueId == uniqueId;
        }

        /// <summary>
        /// Calculate a hashCode for this beacon.
        /// </summary>
        /// <returns>
        /// Calculated Hash Code.
        /// </returns>
        public override int GetHashCode()
        {
            return this.uniqueId.GetHashCode();
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


        ///
        /// Checks to see if an Beacon object is included in the matching criteria of this Region
        /// 

        public bool MatchesBeacon(Beacon beacon)
        {
            // All identifiers must match, or the corresponding region identifier must be null.
            for (int i = identifiers.Count(); --i >= 0;)
            {
                Identifier identifier = identifiers[i];
                Identifier beaconIdentifier = null;
                if (i < beacon.Identifiers.Count())
                {
                    beaconIdentifier = beacon.Identifiers[i];
                }
                if ((beaconIdentifier == null && identifier != null) ||
                        (beaconIdentifier != null && identifier != null && !identifier.Equals(beaconIdentifier)))
                {
                    return false;
                }
            }
            if (BluetoothAddress != null && BluetoothAddress != beacon.BluetoothAddress)
            {
                return false;
            }
            return true;
        }
    }
}
