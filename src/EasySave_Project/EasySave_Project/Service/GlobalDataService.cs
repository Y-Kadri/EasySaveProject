using EasySave_Project.Server;

namespace EasySave_Project.Service;

public class GlobalDataService
{
    
    private static GlobalDataService _instance;
    
    private static readonly object _lock = new object();

    public bool isConnecte { get; set; } = false;

    public Client client { get; set; }
    
    public (string?,string?) connecteTo { get; set; } = (null,null);
    

    private GlobalDataService()
    {
    }

    /// <summary>
    /// Retrieves the singleton instance of GlobalDataService, ensuring thread safety.
    /// </summary>
    /// <returns>The single instance of GlobalDataService.</returns>
    public static GlobalDataService GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new GlobalDataService();
                }
            }
        }
        return _instance;
    }
    
    
}