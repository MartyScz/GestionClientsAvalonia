# Gestion Clients Avalonia

Application desktop développée en C#/.NET avec Avalonia UI, permettant de gérer localement une liste de clients à l’aide d’une base de données SQLite.

## État du projet

Projet en cours de développement.

Le cœur fonctionnel de l’application est opérationnel. Le travail actuel porte principalement sur la fiabilité des données, l’architecture, les tests et la préparation d’une première version publiable.

## Fonctionnalités actuelles

- Saisie du nom et de l’adresse email d’un client
- Validation des champs obligatoires
- Validation du format des adresses email
- Limitation du nom à 100 caractères
- Limitation de l’adresse email à 254 caractères
- Détection des adresses email déjà utilisées
- Enregistrement des clients dans une base SQLite
- Chargement automatique des clients au démarrage
- Affichage de l’identifiant, du nom et de l’adresse email
- En-têtes pour les colonnes Id, Nom et Email
- Affichage du nombre de clients actuellement visibles
- Sélection d’un client dans la liste
- Préremplissage des champs avec les données du client sélectionné
- Bouton Nouveau / Vider pour réinitialiser le formulaire
- Activation automatique des boutons Modifier et Supprimer selon la sélection
- Modification persistante d’un client
- Suppression persistante d’un client
- Fenêtre de confirmation avant la suppression
- Recherche de clients par nom ou adresse email
- Réaffichage automatique de tous les clients lorsque le champ de recherche est vidé
- Export des clients au format CSV
- Import de clients depuis un fichier CSV
- Validation des données pendant l’import
- Détection et exclusion des doublons pendant l’import
- Import CSV transactionnel avec annulation complète en cas d’erreur technique
- Gestion des fichiers CSV verrouillés ou inaccessibles
- Gestion des principales erreurs SQLite

## Fiabilité des données

- Base SQLite stockée dans le dossier local de l’utilisateur :
  `%LocalAppData%\GestionClientsAvalonia\GestionClient.db`
- Création automatique de la base lors du premier lancement
- Adresse email unique sans distinction entre majuscules et minuscules
- Contraintes de longueur appliquées dans l’application et directement dans SQLite
- Transactions SQLite avec `Commit` et `Rollback`
- Système de migrations basé sur `PRAGMA user_version`
- Conservation des clients existants lors des évolutions de la structure de la base
- Gestion d’une base de données temporairement inaccessible au démarrage
- Maintien de l’application ouverte lorsqu’une erreur de démarrage survient
- Désactivation automatique des champs et des actions lorsque la base n’est pas disponible
- Vérification de la disponibilité de la base avant les opérations qui l’utilisent

## Expérience utilisateur

- Interface adaptable au redimensionnement de la fenêtre
- Taille minimale empêchant l’interface de devenir inutilisable
- Liste des clients adaptée automatiquement à l’espace disponible
- Barre de recherche toujours accessible
- Organisation horizontale des boutons pour une interface plus compacte
- Actions disponibles uniquement lorsqu’elles peuvent être utilisées
- Messages visuels différenciés selon leur nature :
  - vert pour les succès
  - rouge pour les erreurs
  - bleu pour les informations
- Réaffichage automatique de tous les clients lorsque le champ de recherche est vidé

## Technologies utilisées

- C#
- .NET 10
- Avalonia UI
- SQLite
- Microsoft.Data.Sqlite

## Organisation actuelle

- `Client.cs` : modèle représentant un client
- `ClientRules.cs` : règles communes de validation
- `ClientRepository.cs` : opérations de lecture et d’écriture dans SQLite
- `Database.cs` : connexion, création et migrations de la base
- `CsvService.cs` : import et export des fichiers CSV
- `EmailValidator.cs` : validation du format des adresses email
- `DeleteConfirmationWindow.axaml` : fenêtre de confirmation avant suppression
- `DeleteConfirmationWindow.axaml.cs` : gestion de la réponse de confirmation
- `MainWindow.axaml` : interface principale
- `MainWindow.axaml.cs` : gestion actuelle des interactions et de l’état de l’interface

## Objectif pédagogique

Ce projet me permet de renforcer mes compétences en C#/.NET, en architecture d’application, en logique métier et en gestion de données avec SQLite.

Il me permet également d’apprendre à rendre une application plus fiable grâce à la validation des données, à la gestion des erreurs, aux transactions et aux migrations de base de données.

Le développement de l’interface me permet aussi de travailler l’ergonomie, le redimensionnement et l’adaptation des actions à l’état de l’application.

Ce projet constitue une étape vers l’apprentissage du développement backend .NET et des API REST.

## Objectif du projet

Créer un logiciel local de gestion de clients simple, fiable et utilisable par une petite entreprise, une association ou un indépendant.