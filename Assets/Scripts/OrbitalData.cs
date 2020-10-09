namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [System.Serializable]
    public partial class OrbitalData
    {
        [JsonProperty("info")]
        public Info Info;

        [JsonProperty("orbits")]
        public Orbit[] Orbits;
    }

    [System.Serializable]
    public partial class Info
    {
        [JsonProperty("coordinates")]
        public string Coordinates;

        [JsonProperty("units")]
        public string Units;
    }

    [System.Serializable]
    public partial class EphData
    {        
        public List<double> ephdata;
    }

    [System.Serializable]
    public partial class AttData
    {
        public List<double> attdata;
    }

    [System.Serializable]
    public partial class Orbit
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("display")]
        public Display Display;

        [JsonProperty("radius")]
        public double Radius;

        [JsonProperty("eph")]        
        //public Eph[] eph;
        public List<double[]> Eph;

        [JsonProperty("att")]
        public List<double[]> Att;

        [JsonProperty("time")]
        public double[] Time;

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color;
    }

    public enum Display { LinePoint, Point };

    public partial class OrbitalData
    {
        public static OrbitalData FromJson(string json) => JsonConvert.DeserializeObject<OrbitalData>(json, QuickType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this OrbitalData self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                DisplayConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class DisplayConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Display) || t == typeof(Display?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {        // initialises instance of JsonReader
                 // ititialises deserialiser
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "line,point":
                    return Display.LinePoint;
                case "point":
                    return Display.Point;
            }
            throw new Exception("Cannot unmarshal type Display");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Display)untypedValue;
            switch (value)
            {
                case Display.LinePoint:
                    serializer.Serialize(writer, "line,point");
                    return;
                case Display.Point:
                    serializer.Serialize(writer, "point");
                    return;
            }
            throw new Exception("Cannot marshal type Display");
        }

        public static readonly DisplayConverter Singleton = new DisplayConverter();
    }
}