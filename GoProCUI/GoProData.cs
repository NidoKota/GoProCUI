using System;
using System.Collections.Generic;

[Serializable]
public class GoProData
{
    public List<Device> Devices = new List<Device>();
    public bool AccessPoint;
    public float Buttery;

    [Serializable]
    public class Device
    {
        public string Name;
    }
}