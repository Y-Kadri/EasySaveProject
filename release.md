## Nouveautés de cette version


- Fonctionnalités disponibles :
    - Nouvelle interface basée sur **Avalonia** pour une meilleure expérience utilisateur
    - Prise en charge du cryptage des fichiers via **CryptoSoft**  
    - Cryptage des fichiers selon les extensions définies par l'utilisateur dans les paramètres 
    - Blocage des sauvegardes si un logiciel métier est en cours d’exécution  
    - Terminaison propre du fichier en cours de sauvegarde dans le cas de travaux séquentiels  
    - Possibilité de définir un logiciel métier dans les paramètres  
    - L'arrêt des travaux est consigné dans le fichier log  

- Fonctionnalités modifié :
    - Abandon du mode Console  
    - Plus aucune restriction sur le nombre de travaux créés  
    - Ajout d’une nouvelle information dans les fichiers de logs : **le temps de cryptage des fichiers (en ms)**  
        - **0** : Pas de cryptage  
        - **>0** : Temps de cryptage (en ms)  
        - **<0** : Code erreur  

---