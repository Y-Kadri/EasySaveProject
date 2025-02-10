# EasySaveProject
[![GitHub release](https://img.shields.io/github/v/release/Y-Kadri/EasySaveProject?label=Release&style=flat)](https://github.com/Y-Kadri/EasySaveProject/releases)

## Conventions de Nommage & Lignes Directrices

## 1. Principes Généraux

- Tout le code **DOIT** être écrit en **anglais**.
- Utiliser **PascalCase** pour les noms de classes et de méthodes.
- Utiliser **camelCase** pour les variables locales et les paramètres de méthode.
- Utiliser **SCREAMING_SNAKE_CASE** pour les constantes.
- Utiliser des **noms significatifs et descriptifs**.
- Éviter les abréviations sauf si elles sont largement connues (ex: "id" au lieu de "identifier").
- Les noms des interfaces **DOIVENT** commencer par `I` (ex: `IJobObserver`).
- Les noms des classes abstraite **DOIVENT** commencer par `A` (ex: `AJob`).

## 2. Convention des Commits Git

Nous suivons le format **Conventional Commits** :

```bash
<type>[<scope>]: <message>
```

### Types de Commit :

| Type | Objectif |
| --- | --- |
| feature | Ajout d'une nouvelle fonctionnalité |
| **fix** | Correction d'un bug |
| **docs** | Modifications de la documentation |
| **style** | Changements de style de code (indentation, espaces, etc.) |
| **refactor** | Réorganisation du code sans changement de comportement |
| **perf** | Amélioration des performances |
| **test** | Ajout ou modification de tests |
| **chore** | Tâches de maintenance (dépendances, CI/CD, etc.) |

### Exemples de Commits :

```bash
feature[job]: ajout de la fonctionnalité d'exécution des jobs
```

## 3. Stratégie de Branching GitFlow

Nous utilisons **GitFlow** avec les branches suivantes :

- `main` → Code stable prêt pour la production.
- `development` → Branche de développement active.
- `feature/*` → Nouvelles fonctionnalités (dérivées de `development`).
- `release/*` → Préparation d'une nouvelle version (dérivée de `development`).
- `hotfix/*` → Corrections de bugs en production (dérivées de `main`).

## 4. Structure du Projet & Règles de Nommage

### **Répertoires & Noms de Fichiers**

- **Controller** → Gèrent la logique métier et orchestrent les actions.
- **Model** → Contiennent les structures de données et les entités métier.
- **View** → Couche d'interface utilisateur (CLI, GUI, etc.).
- **Manager** → Gèrent les opérations complexes et le cycle de vie.
- **Command** → Implémentent le pattern Command.
- **Service** → Logique métier principale et services.
- **Util** → Fonctions utilitaires/aides.
- **Ressource** → Ressources comme les fichiers de configuration.

### **Règles de Nommage par Type**

### **Classes & Interfaces**

- Utiliser **PascalCase** pour les noms de classes.
- Les interfaces **DOIVENT** commencer par `I`.
- Exemple :
    
    ```csharp
    public class JobManager { }
    public interface IJobObserver { }
    
    ```
    

### **Méthodes**

- Utiliser **PascalCase**.
- Commencer par un verbe pour plus de clarté (ex: `GetJobStatus`, `ExecuteCommand`).
- Exemple :
    
    ```csharp
    public void ExecuteJob() { }
    
    ```
    

### **Variables & Paramètres**

- Utiliser **camelCase** pour les variables et les paramètres de méthode.
- Exemple :
    
    ```csharp
    string jobName;
    int jobCount;
    ```
    

### **Constantes**

- Utiliser **SCREAMING_SNAKE_CASE**.
- Exemple :
    
    ```csharp
    public const int MAX_RETRY_COUNT = 5;
    ```
    

## 5. Bonnes Pratiques de Codage

- Garder les méthodes **courtes** et **avec une seule responsabilité**.
- Utiliser **l'injection de dépendance** autant que possible.
- Éviter les nombres magiques (utiliser des constantes ou des enums à la place).
- Commenter le code
- Suivre les principes SOLID.

## 6. Exemple de Code

```csharp
public class JobManager
{
    private readonly IJobObserver jobObserver;
    private List<JobModel> jobs;

    public JobManager(IJobObserver observer)
    {
        jobObserver = observer;
        jobs = new List<JobModel>();
    }

    public void ExecuteJob(JobModel job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        job.Run();
        jobObserver.Notify(job);
    }
}

```