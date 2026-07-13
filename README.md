# Gestion Clients Avalonia

Application desktop en C#/.NET permettant de gérer des clients avec Avalonia UI et une base de données locale SQLite.

## État du projet

Projet en cours de développement.

## Fonctionnalités actuelles

- Saisie du nom et de l'email d'un client
- Validation des champs obligatoires
- Enregistrement des clients dans une base SQLite
- Chargement automatique des clients au démarrage
- Affichage de l'identifiant, du nom et de l'email
- Sélection d'un client dans la liste
- Préremplissage des champs avec les données du client sélectionné
- Modification persistante d'un client
- Suppression persistante d'un client
- Fenêtre de confirmation avant la suppression
- Création automatique de la base et de la table Clients
- Recherche de clients par nom ou adresse email
- Export des clients au format CSV
- Import de clients depuis un fichier CSV
- Détection et exclusion des doublons par adresse email lors de l'import
- Validation du format des adresses email lors de l'ajout, de la modification et de l'import CSV

## Technologies utilisées

- C#
- .NET
- Avalonia UI
- SQLite
- Microsoft.Data.Sqlite

## Objectif pédagogique

Ce projet me permet de renforcer mes bases en C#/.NET, en architecture d'application, en logique métier et en gestion de données avec SQLite.

Il constitue également une étape vers l'apprentissage du développement backend .NET et des API REST.

## Objectif du projet

Créer un petit logiciel de gestion de clients simple, local et utilisable par une petite entreprise ou un indépendant.