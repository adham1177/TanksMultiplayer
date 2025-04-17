using System;
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
        private int _teamId;
        private FixedString64Bytes _name;
        

        public int TeamId => _teamId;

        public FixedString64Bytes Name => _name;

        public PlayerNetworkData(int teamId, FixedString64Bytes name)
        {
            _teamId = teamId;
            _name = name;
        }

        public bool Equals(PlayerNetworkData other)
        {
            return _teamId == other._teamId && _name == other._name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // serializer.SerializeValue(ref _playerId);
            serializer.SerializeValue(ref _teamId);
            serializer.SerializeValue(ref _name);
        }
        
    }
}
