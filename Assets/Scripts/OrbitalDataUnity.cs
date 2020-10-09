namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    //using Newtonsoft.Json;
    //using Newtonsoft.Json.Converters;

    [System.Serializable]
    public partial class OrbitalDataUnity
    {
        public Info Info;

        public List<OrbitUnity> Orbits;

        public OrbitalDataUnity(OrbitalData data)
        {
            this.Info = data.Info;
            this.Orbits = new List<OrbitUnity>();
            foreach (Orbit orbit in data.Orbits)
            {
                this.Orbits.Add(new OrbitUnity(orbit));
            }
        }
    }

    [System.Serializable]
    public partial class RawEphData
    {
        public double xPos;
        public double zPos;
        public double yPos;
        public double xVel;
        public double zVel;
        public double yVel;
        public RawEphData(double xPos, double zPos, double yPos, double xVel, double zVel, double yVel)
        {
            this.xPos = xPos;
            this.zPos = zPos;
            this.yPos = yPos;
            this.xVel = xVel;
            this.zVel = zVel;
            this.yVel = yVel;
        }
    }

    [System.Serializable]
    public partial class RawAttData
    {
        public double X; // Unity format 
        public double Y;
        public double Z;
        public double W;
        public RawAttData(double Im1, double Im2, double Im3, double Rl)   // GMAT format
        {
            // just propagating raw data, mapping occurs in JsonDataImport
            this.X = Im1;
            this.Y = Im2;
            this.Z = Im3;
            this.W = Rl;
        }
    }

    [System.Serializable]
    public partial class OrbitUnity
    {
        public string Name;
        public Display Display;
        public double Radius;

        public List<RawEphData> Eph;
        public List<RawAttData> Att;

        public double[] Time;
        public string Color;

        public OrbitUnity(Orbit data)
        {
            this.Name = data.Name;
            this.Display = data.Display;
            this.Radius = data.Radius;
            this.Time = data.Time;
            this.Color = data.Color;

            this.Eph = new List<RawEphData>();
            foreach (double[] Value in data.Eph)
            {
                this.Eph.Add(new RawEphData(Value[0], Value[1], Value[2], Value[3], Value[4], Value[5]));
            }

            if (data.Att != null)   // ensure null attitude data is handled
            {
                this.Att = new List<RawAttData>();
                foreach (double[] Value in data.Att)
                {
                    this.Att.Add(new RawAttData(Value[0], Value[1], Value[2], Value[3]));
                }
            }
        }
    }

}