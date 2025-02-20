using EasySave_Project.Server;

namespace EasySave_Project.Service;

public class GlobalDataService
{
    
    private static GlobalDataService _instance;
    private static readonly object _lock = new object();

    public bool isConnecte { get; set; } = false;

    public Client client { get; set; }
    
    public (string?,string?) connecteTo { get; set; } = (null,null);
    

    // Constructeur privé
    private GlobalDataService()
    {
    }

    // Méthode pour obtenir l'instance du singleton
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