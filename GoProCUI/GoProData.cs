using System;
using System.Collections.Generic;

[Serializable]
public class GoProData
{
    public List<Device> Devices = new List<Device>();
    public string Message;

    [Serializable]
    public class Device
    {
        public string Name;
        public string Id;
        public bool AccessPoint;
        public float Buttery;
    }
}