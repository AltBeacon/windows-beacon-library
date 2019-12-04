using System;
using System.Collections.Generic;


namespace Altbeacon.Beacon
{
    //
    // Tracks Beacons or Regions
    //
    public class BeaconTracker
    {
        private HashSet<Region> TrackedRegions = new HashSet<Region>();
        private HashSet<Object> added = new HashSet<Object>();
        private HashSet<Object> removed = new HashSet<Object>();
        private bool trackOnlyRegions = false;
        public long ExpirationSeconds = 5;

        private class TrackedObject
        {
            public Object beaconOrRegion;
            public DateTime lastSeen;
            public override bool Equals(Object obj)
            {
                if (obj == null || !(obj is TrackedObject))
                    return false;
                else
                    return beaconOrRegion == ((TrackedObject)obj).beaconOrRegion;
            }

            public override int GetHashCode()
            {
                return beaconOrRegion.GetHashCode();
            }
        }
        private HashSet<TrackedObject> trackedObjects = new HashSet<TrackedObject>();
             
        private BeaconTracker()
        {

        }
        public static BeaconTracker GetInstanceForMonitoring()
        {
            BeaconTracker tracker = new BeaconTracker();
            tracker.trackOnlyRegions = true;
            tracker.ExpirationSeconds = 30;
            return tracker;
        }
        public static BeaconTracker GetInstanceForRanging()
        {
            BeaconTracker tracker = new BeaconTracker();
            return tracker;
        }
        public static BeaconTracker GetInstanceForWildcardRanging()
        {
            BeaconTracker tracker = new BeaconTracker();
            tracker.TrackedRegions.Add(new Region("wildcard", null, null, null));
            return tracker;
        }

        public void Track(Beacon beacon)
        {
            foreach (Region region in TrackedRegions)
            {
                if (region.MatchesBeacon(beacon))
                {
                    if (trackOnlyRegions)
                    {
                        TrackInternal(region);
                    }
                    else
                    {
                        TrackInternal(beacon);
                    }
                }
            }

        }
        private void TrackInternal(Object beaconOrRegion)
        {
            var tracked = new TrackedObject();
            tracked.beaconOrRegion = beaconOrRegion;
            tracked.lastSeen = DateTime.Now;
            trackedObjects.Add(tracked);
        }
        public void PurgeExpired()
        {
            HashSet<TrackedObject> newlyTracked = new HashSet<TrackedObject>();
            foreach (TrackedObject o in this.trackedObjects)
            {
                if ((DateTime.Now - o.lastSeen).TotalSeconds > ExpirationSeconds)
                {
                    removed.Add(o.beaconOrRegion);
                }
                else
                {
                    newlyTracked.Add(o);
                }
            }
            trackedObjects = newlyTracked;
        }
        public void ClearAddedAndRemoved()
        {
            added = new HashSet<Object>();
            removed = new HashSet<Object>();        
        }

        private HashSet<Region> GetRegions(HashSet<Object> objects)
        {
            HashSet<Region> regions = new HashSet<Region>();
            foreach (Object o in objects)
            {
                if (o is Region)
                {
                    regions.Add((Region)o);
                }
            }
            return regions;
        }
        public HashSet<Beacon> GetVisibleBeacons()
        {
            HashSet<Beacon> beacons = new HashSet<Beacon>();
            foreach (TrackedObject o in trackedObjects)
            {
                if (o.beaconOrRegion is Beacon)
                {
                    beacons.Add((Beacon)o.beaconOrRegion);
                }
            }
            return beacons;
        }
        public HashSet<Region> GetEnteredRegions()
        {
            return GetRegions(added);
        }
        public HashSet<Region> GetExitedRegions()
        {
            return GetRegions(removed);
        }
    }
}
