namespace Server;

public class CacheManager
{
    private static CacheManager _instance;
    private Dictionary<string, bool> _configFlags;

    private CacheManager()
    {
        _configFlags = new();
    }
    
    
    public static CacheManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CacheManager();
            }
            return _instance;
        }
    }

    public bool RetrieveConfigFlag(string key) => _configFlags.ContainsKey(key) && _configFlags[key];

    public void StoreConfig(string key, bool flag) => _configFlags[key] = flag;
}