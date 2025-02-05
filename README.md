# EasySaveProject
[![GitHub release](https://img.shields.io/github/v/release/Y-Kadri/EasySaveProject?label=Release&style=flat)](https://github.com/Y-Kadri/EasySaveProject/releases)

EasySaveProject est une application de gestion et d'exécution de sauvegardes de fichiers, développée en .NET Core. Elle permet de créer, exécuter et suivre l'état des sauvegardes, tout en offrant une interface console simple et efficace.

## Fonctionnalités principales :
Création et gestion de jusqu'à 5 travaux de sauvegarde.
Sauvegarde complète ou différentielle.
Enregistrement des actions dans un fichier log JSON.
Suivi en temps réel de l'état des sauvegardes.
Compatibilité avec disques locaux, externes et lecteurs réseau.

## Installation
Assurez-vous d'avoir .NET Core installé sur votre machine.
Téléchargez l'exécutable ou clonez le repository.
Exécutez la commande suivante :
dotnet EasySave.exe
Cela lancera l'application en mode console.

## Utilisation
Créez un travail de sauvegarde en spécifiant un nom, un répertoire source, un répertoire cible et le type de sauvegarde (complète ou différentielle).
Exécutez les sauvegardes via la commande appropriée.

## Tests
Des tests automatisés sont inclus pour valider la création et l'exécution des sauvegardes, ainsi que la gestion des erreurs.