using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace InteractableExfilsAPI.Helpers
{
    public class ObjectData
    {
        [JsonProperty("Name")]
        public string Name;
        [JsonProperty("Position")]
        public UnityEngine.Vector3 Position;
        [JsonProperty("Rotation")]
        public UnityEngine.Quaternion Rotation;
        [JsonProperty("Scale")]
        public UnityEngine.Vector3 Scale;
    }

    public class MapData
    {
        [JsonProperty("MapID")]
        public string MapID;
        [JsonProperty("Objects")]
        public List<ObjectData> Objects;

        public static MapData Get(string mapId)
        {
            string path = GetPathByMapID(mapId);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return GetDataFromJson(json);
        }

        private static MapData GetDataFromJson(string json)
        {
            return JsonConvert.DeserializeObject<MapData>(json);
        }

        private static string GetPathByMapID(string mapId)
        {
            string locationsPath = Path.Combine(Path.GetDirectoryName(Plugin.AssemblyPath), "Locations");
            string filePath = Path.Combine(locationsPath, $"{mapId}.json");
            return filePath;
        }
    }
}
