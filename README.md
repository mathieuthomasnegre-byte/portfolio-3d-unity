# Portfolio 3D (Unity)

Portfolio interactif de Mathieu Negre : trois chambres, trois âges (enfant, ado, jeune adulte), un parcours à assembler brique par brique. Réalisé avec Unity et exporté pour le web (WebGL).

## Prérequis

- Unity **6000.6.9f1** (voir `ProjectSettings/ProjectVersion.txt`), avec le module WebGL.
- Node.js 22 ou plus récent, pour le serveur local.
- Le pack gratuit **Cute Furniture Free** d'ithappy, à importer depuis l'Asset Store dans `Assets/ithappy/`. Il n'est pas inclus dans le dépôt : sa licence (Unity Asset Store EULA) interdit de redistribuer ses fichiers.

Le mobilier Kenney Furniture Kit est inclus (licence CC0, voir `Assets/Kenney Furniture Kit/License.txt`).

## Construire

Dans Unity, menu **Portfolio** :

- **Reconstruire la scène** : régénère `Assets/Portfolio/Scenes/Portfolio.unity` à partir des scripts de `Scripts/Editor`.
- **Exporter pour le web (WebGL)** : produit `Build/WebGL/` et y copie l'appli Ahla (dépôt `ahla-apprendre-libanais`, attendu dans `../Projets/apprendre-libanais`).

## Tester l'export web

```bash
npm start
```

Puis ouvrir http://127.0.0.1:8123. Le serveur n'écoute que sur cette machine.

## Projets liés

- Hack the Feed : https://data-to-delightful-discovery.lovable.app
- Ahla, appli pour apprendre le libanais : servie sous `libanais/` dans l'export web.
