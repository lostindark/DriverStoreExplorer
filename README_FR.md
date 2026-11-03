<table><tr>
<td><img src="./Rapr/icon.ico" alt="logo" width="96"></td>
<td>
<h1>Driver Store Explorer (RAPR)</h1>
🌏: <a href="/README.md">English</a> | <a href="/README_ES.md">Español</a> | <a href="/README_FR.md"><b>Français</b></a> | <a href="/README_TR.md">Türkçe</a> | <a href="/README_KO.md">한국어</a> | <a href="/README_ZH-CN.md">简体中文</a> | <a href="/README_ZH-TW.md">繁體中文</a>
</td>
</tr></table>

---

[![Build Status](https://ci.appveyor.com/api/projects/status/kqtvhfq23am2gq26/branch/master?svg=true)](https://ci.appveyor.com/project/lostindark/driverstoreexplorer/branch/master)
[![Build Status](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml/badge.svg)](https://github.com/lostindark/DriverStoreExplorer/actions/workflows/ci.yml)

## ⚠️ Avertissement
**Driver Store Explorer modifie le magasin de pilotes de Windows. Une mauvaise utilisation peut entraîner des dysfonctionnements du système, empêcher Windows de démarrer ou provoquer la perte de fonctionnalités de périphériques. Prenez connaissance des risques avant de continuer. Sauvegardez systématiquement vos pilotes avant de supprimer quoi que ce soit.**

---

## Qu'est-ce que Driver Store Explorer ?

Driver Store Explorer (RAPR) est un outil puissant permettant d'afficher, de gérer et de nettoyer le [magasin de pilotes](https://learn.microsoft.com/windows-hardware/drivers/install/driver-store) de Windows. Il est destiné aux utilisateurs avancés et aux administrateurs. Si vous n'êtes pas familiarisé avec les rouages internes de Windows, l'utilisation de cet outil est déconseillée.

---

## Fonctionnalités

### Opérations principales
* **Parcourir et lister** : Afficher tous les packages de pilotes tiers avec des métadonnées détaillées (taille, version du pilote, date du pilote, fabricant, etc.)
* **Ajouter / Installer** : Installer de nouveaux packages de pilotes avec option d'installation automatique du périphérique
* **Supprimer les pilotes** : Supprimer un ou plusieurs packages de pilotes, avec prise en charge de l'option « Forcer la suppression » pour les pilotes en cours d'utilisation  
	_Remarque : Les conséquences exactes de la suppression de pilotes dépendent de l'état du système et de son utilisation. Faites preuve de prudence, car la suppression peut affecter le fonctionnement d'un périphérique ou nécessiter la réinstallation des pilotes pour certains matériels._
* **Exporter** : Sauvegarder les pilotes sélectionnés ou tous les pilotes dans une arborescence de dossiers organisée
* **Nettoyage intelligent** : Identifier et sélectionner automatiquement les versions de pilotes obsolètes ou inutilisées

### Capacités avancées
* **API multiples** : Prise en charge des moteurs Windows API native, DISM ou PnPUtil avec détection automatique
* **En ligne et hors ligne** : Prise en charge de l'ordinateur local ou des magasins de pilotes hors ligne d'images Windows
* **Association de périphériques** : Afficher les périphériques connectés et leur état de présence
* **Opérations par lots** : Sélection multiple pour les actions en masse avec suivi de la progression
* **Recherche en temps réel** : Filtrage en direct et exportation au format CSV pour analyse

### Interface utilisateur
* **Multilingue** : Plus de 20 langues avec prise en charge de l'écriture de droite à gauche (RTL)
* **Personnalisable** : Colonnes triables, regroupement et disposition flexible
* **Retour visuel** : Codes couleur, mise en surbrillance de la sélection et journalisation détaillée

---

## Comprendre l'état des pilotes et les options de suppression

- **Pilotes obsolètes :** Les pilotes sont considérés comme « obsolètes » lorsque des versions plus récentes existent sur le système. Les supprimer peut aider à libérer de l'espace et à réduire l'encombrement, mais peut avoir un impact sur la compatibilité avec certains périphériques ou configurations. Pensez à sauvegarder vos pilotes avant toute suppression. L'option « Sélectionner les pilotes obsolètes » permet d'identifier automatiquement les pilotes obsolètes, bien que les résultats puissent varier.
- **Pilotes inutilisés :** « Sélectionner les pilotes inutilisés » sélectionne les pilotes sans périphérique associé, et non des pilotes dont la suppression est assurément sans risque. Des pilotes essentiels d'antivirus, de services réseau, d'imprimantes et d'autres logiciels peuvent également être sélectionnés. Le bouton ainsi que le menu contextuel affichent un avertissement avant de modifier la sélection ; « Annuler » est l'option par défaut. Cet avertissement est activé par défaut et peut être désactivé via **Options > Avertir avant de sélectionner les pilotes inutilisés**. Un rappel d'état jaune reste affiché à côté des résultats des opérations tant que l'un de ces pilotes est coché ; les erreurs restent en rouge sans masquer le rappel. L'exportation réussie de packages de pilotes efface le rappel pour les pilotes sauvegardés ; les exports CSV et les sauvegardes ayant échoué ne l'effacent pas. Vérifiez soigneusement les pilotes sélectionnés et sauvegardez-les avant suppression.
- **Pilotes critiques au démarrage :** Windows peut avoir besoin de ces pilotes pour démarrer. « Sélectionner les pilotes obsolètes » et « Sélectionner les pilotes inutilisés » les ignorent, à moins d'activer **Options > Inclure les pilotes critiques au démarrage dans les pilotes obsolètes et inutilisés**.
- **Sélection pendant la recherche :** Lorsque la zone de recherche filtre la liste, « Tout sélectionner », « Inverser la sélection » et « Sélectionner » ne modifient que les pilotes visibles. « Sélectionner les pilotes obsolètes » et « Sélectionner les pilotes inutilisés » analysent quant à eux l'ensemble du magasin de pilotes : ils réinitialisent donc d'abord la recherche, et tous les éléments sélectionnés restent visibles.
- **Noms de périphériques grisés :** Les pilotes dont le nom de périphérique apparaît en gris sont associés à des périphériques qui ne sont pas connectés actuellement (tels que des appareils photo, téléphones ou disques externes). Si vous supprimez ces pilotes, vous devrez les réinstaller si vous reconnectez le périphérique ultérieurement.
- **Nettoyage hors ligne :** La sélection automatique des pilotes obsolètes/inutilisés est désactivée pour les magasins hors ligne car l'utilisation des périphériques ne peut pas être établie de manière fiable. Les pilotes peuvent toujours être sélectionnés manuellement pour être supprimés.
- **Forcer la suppression :** Utilisez cette option si vous devez supprimer un pilote actuellement en cours d'utilisation. *Remarque : cette option peut ne pas fonctionner pour les pilotes d'impression.*

---

## Capture d'écran
![Capture d'écran de DriverStoreExplorer](https://github.com/user-attachments/assets/2d7df896-494d-4bcd-b064-5f05696cd0d3)

---

## Installation

### Prérequis
- Windows 7 ou version ultérieure
- .NET Framework 4.7.2 ou version ultérieure
- Privilèges d'administrateur

### Option 1 : Télécharger l'exécutable précompilé (recommandé)
1. Allez sur la [page de la plus récente version](https://github.com/lostindark/DriverStoreExplorer/releases/latest)
2. Téléchargez la plus récente archive ZIP
3. Décompressez l'archive dans le dossier de votre choix
4. Exécutez `Rapr.exe`

### Option 2 : Installer via Winget (recommandé)
```powershell
winget install lostindark.DriverStoreExplorer
```
Après l'installation, lancez l'outil en exécutant :
```powershell
rapr
```

### Option 3 : Installer via Scoop
```powershell
scoop bucket add extras
scoop install driverstoreexplorer
```
Après l'installation, lancez l'outil en exécutant :
```powershell
rapr
```

### Option 4 : Compiler depuis les sources
1. Clonez ou téléchargez ce dépôt
2. Ouvrez `Rapr.sln` dans Visual Studio 2022
3. Générez la solution (Générer → Générer la solution ou Ctrl+Maj+B)
4. Exécutez le fichier exécutable depuis le répertoire de sortie

---

## Historique du projet
Initialement hébergé sur [https://driverstoreexplorer.codeplex.com/](https://web.archive.org/web/20190417132137/https://archive.codeplex.com/?p=driverstoreexplorer).

## Crédits
- [ObjectListView](https://github.com/lostindark/ObjectListView)
- [Managed DismApi Wrapper](https://github.com/jeffkl/ManagedDism)
- [FlexibleMessageBox](https://www.codeproject.com/Articles/601900/FlexibleMessageBox-A-Flexible-Replacement-for-the)
- [Resource Embedder](https://github.com/0xced/resource-embedder)
- [PortableSettingsProvider](https://github.com/bluegrams/SettingsProviders)
- [Strong Namer](https://github.com/dsplaisted/strongnamer)

## Partenaires
Signature de code gratuite sous Windows fournie par [SignPath.io], certificat émis par la [SignPath Foundation].

[SignPath.io]: https://signpath.io
[SignPath Foundation]: https://signpath.org
