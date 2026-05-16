# Nexus404

Un middleware modulaire basé sur l'IA qui intercepte les erreurs 404, analyse l'intention de l'utilisateur et la structure du site, et génère dynamiquement des redirections sémantiques ou du contenu de secours personnalisé.

## Fonctionnalités

- **Interception 404** : Détecte les ressources manquantes en temps réel.
- **Analyse Sémantique** : Utilise l'IA pour comprendre ce que l'utilisateur cherchait.
- **Redirection Dynamique** : Suggère ou redirige automatiquement vers la page la plus proche.
- **Contenu de Secours** : Génère une page 404 personnalisée et utile si aucune redirection n'est possible.

## Architecture

- **Nexus404.Middleware** : Le composant .NET à intégrer dans votre pipeline ASP.NET Core.
- **Nexus404.AiService** : Service Python gérant l'intelligence et l'analyse de structure.
- **Nexus404.DemoApp** : Exemple d'intégration.

## Installation (.NET)

Ajoutez le middleware à votre projet :

```csharp
app.UseMiddleware<Nexus404Middleware>();
```

## Configuration (Python AI Service)

```bash
cd Nexus404.AiService
pip install -r requirements.txt
python main.py
```

## Contributing

Merci de contribuer ! Veuillez suivre les standards habituels.
