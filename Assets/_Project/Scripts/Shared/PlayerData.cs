using System;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;

namespace _Project.Scripts.Shared
{
    public class PlayerData
    {
        private string _name;
        private string _id;

        public string Name => _name;

        public string ID => _id;


        public PlayerData(string name, string id)
        {
            _name = name;
            _id = id;
        }
    }


    public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData>
    {
        private FixedString64Bytes _playerId;
        private ulong _clientId;
        private int _teamId;
        private FixedString64Bytes _name;
        

        public int TeamId => _teamId;

        public FixedString64Bytes Name => _name;

        public FixedString64Bytes PlayerId => _playerId;

        public ulong ClientId => _clientId;

        public PlayerNetworkData(FixedString64Bytes playerId, ulong clientId, int teamId, FixedString64Bytes name)
        {
            _teamId = teamId;
            _name = name;
            _playerId = playerId;
            _clientId = clientId;
        }

        public bool Equals(PlayerNetworkData other)
        {
            return _playerId == other._playerId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _playerId);
            serializer.SerializeValue(ref _clientId);
            serializer.SerializeValue(ref _teamId);
            serializer.SerializeValue(ref _name);
        }
        
    }
    
    public class FixedString64BytesConverter : JsonConverter<FixedString64Bytes>
    {
        public override void WriteJson(JsonWriter writer, FixedString64Bytes value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override FixedString64Bytes ReadJson(JsonReader reader, Type objectType, FixedString64Bytes existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string str = (string)reader.Value;
            return new FixedString64Bytes(str);
        }
    }
    
}
