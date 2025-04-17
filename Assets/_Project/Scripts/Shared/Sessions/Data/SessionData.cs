namespace _Project.Scripts.Shared.Sessions.Data
{
    public struct SessionData
    {
        private string _id;
        private string _name;
        private int _numberOfPlayers;
        private bool _isLocked;
        private int _maxPlayers;
        private bool _canJoin;

        public string ID => _id;
        public string Name => _name;
        public int NumberOfPlayers => _numberOfPlayers;

        public bool IsLocked => _isLocked;
        
        public int MaxPlayers => _maxPlayers;

        public bool CanJoin => _canJoin;

        public SessionData(string id, string name, int numberOfPlayers, bool isLocked, int maxPlayers)
        {
            _id = id;
            _name = name;
            _numberOfPlayers = numberOfPlayers;
            _isLocked = isLocked;
            _maxPlayers = maxPlayers;
            _canJoin = !isLocked && _numberOfPlayers < maxPlayers;
        }
    }
}