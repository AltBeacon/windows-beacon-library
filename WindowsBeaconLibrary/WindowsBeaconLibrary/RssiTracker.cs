using System;
using System.Collections.Generic;

namespace WindowsBeaconLibrary
{
    public class RssiTracker
    {
        class Sample
        {
            public DateTime Timestamp;
            public Int32 Rssi;
        }
        public Int32 AverageTimeSecs = 20;
        private Dictionary<String,List<Sample>> rssiSamples = new Dictionary<String, List<Sample>>();
        private void Reset(String key)
        {
            rssiSamples[key] = new List<Sample>();
        }
        public double Add(String key, Int32 value)
        {
            if (!rssiSamples.ContainsKey(key))
            {
                rssiSamples[key] = new List<Sample>();
            }
            Sample sample = new Sample();
            sample.Rssi = value;
            sample.Timestamp = DateTime.Now;
            rssiSamples[key].Add(sample); 
            return RunningAverage(key);
        }
        public Double Count(String key)
        {
            if (rssiSamples.ContainsKey(key))
            {
                return rssiSamples[key].Count;
            }
            else
            {
                return 0;
            }
        }
        public Double RunningAverage(String key)
        {
            if (!rssiSamples.ContainsKey(key))
            {
                return 0.0;
            }
            double sum = 0.0;
            int count = 0;
            List<Sample> newRssiSamples = new List<Sample>();            
            for (int i = 0; i < rssiSamples[key].Count; i++)
            {
                Double age = (DateTime.Now - rssiSamples[key][i].Timestamp).TotalSeconds;
                if (age < AverageTimeSecs)
                {
                    newRssiSamples.Add(rssiSamples[key][i]);
                    sum += rssiSamples[key][i].Rssi;
                    count += 1;
                }
            }
            rssiSamples[key] = newRssiSamples;
            if (count > 0)
            {
                return sum / count;
            }
            else
            {
                return 0;
            }

        }
    }


}
