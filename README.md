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
- Affichage de messages simples et compréhensibles lors des erreurs techniques
- Enregistrement des détails techniques des exceptions dans un fichier de logs local

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
- Gestion d’une base de données corrompue ou incompatible
- Maintien de l’application ouverte lorsqu’une erreur de démarrage survient
- Désactivation automatique des champs et des actions lorsque la base n’est pas disponible
- Vérification de la disponibilité de la base avant les opérations qui l’utilisent
- Protection des accès SQLite pendant :
  - le démarrage de l’application
  - l’ajout d’un client
  - la modification d’un client
  - la suppression d’un client
  - la recherche de clients
  - le rechargement automatique de la liste
  - l’import CSV
  - l’export CSV
- Conservation du dernier état valide de la liste et du compteur lorsqu’une lecture SQLite échoue
- Journalisation centralisée des erreurs techniques
- Une erreur d’écriture du fichier de logs ne provoque pas la fermeture de l’application

Le fichier de logs est enregistré dans :

`%LocalAppData%\GestionClientsAvalonia\Logs\application.log`

Il contient notamment :

- la date et l’heure de l’erreur
- le contexte dans lequel l’erreur est survenue
- le type de l’exception
- le message technique
- la pile d’appels complète

Les erreurs métier prévues, comme une adresse email déjà utilisée, ne sont pas enregistrées inutilement dans le journal.

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
- Messages techniques simplifiés pour rester compréhensibles par l’utilisateur
- Réaffichage automatique de tous les clients lorsque le champ de recherche est vidé
- Conservation de la liste actuellement affichée lorsqu’une recherche ou un rechargement échoue

## Technologies utilisées

- C#
- .NET 10
- Avalonia UI
- SQLite
- Microsoft.Data.Sqlite

## Organisation actuelle

- `AppLogger.cs` : journalisation locale des erreurs techniques
- `Client.cs` : modèle représentant un client
- `ClientRules.cs` : règles communes de validation
- `ClientRepository.cs` : opérations de lecture et d’écriture dans SQLite
- `Database.cs` : connexion, création et migrations de la base
- `CsvService.cs` : import et export des fichiers CSV
- `EmailValidator.cs` : validation du format des adresses email
- `DeleteConfirmationWindow.axaml` : fenêtre de confirmation avant suppression
- `DeleteConfirmationWindow.axaml.cs` : gestion de la réponse de confirmation
- `MainWindow.axaml` : interface principale
- `MainWindow.axaml.cs` : gestion actuelle des interactions, de l’état de l’interface et des erreurs

## Objectif pédagogique

Ce projet me permet de renforcer mes compétences en C#/.NET, en architecture d’application, en logique métier et en gestion de données avec SQLite.

Il me permet également d’apprendre à rendre une application plus fiable grâce à :

- la validation des données
- la gestion structurée des exceptions
- la journalisation des erreurs techniques
- les transactions SQLite
- les contraintes de base de données
- les migrations de schéma
- la conservation d’un état cohérent de l’interface en cas d’échec

Le développement de l’interface me permet aussi de travailler l’ergonomie, le redimensionnement et l’adaptation des actions à l’état de l’application.

Ce projet constitue une étape vers l’apprentissage du développement backend .NET et des API REST.

## Objectif du projet

Créer un logiciel local de gestion de clients simple, fiable et utilisable par une petite entreprise, une association ou un indépendant.