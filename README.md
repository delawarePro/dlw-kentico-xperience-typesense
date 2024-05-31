# Xperience by Kentico Typesense integration by delaware consulting

## Description

This integration enables you to create [Typesense](https://typesense.org/) search collections to collection content of pages ([content types](https://docs.xperience.io/x/gYHWCQ) with the 'Page' feature enabled) from the Xperience content tree using a code-first approach. To provide a search interface for the collectioned content, developers can use the [.NET API](https://github.com/DAXGRID/typesense-dotnet), [JavaScript API](https://github.com/typesense/typesense-js), or the [InstantSearch.js](https://typesense.org/docs/guide/search-ui-components.html) library.

**Be aware that this integration has been developed by delaware consulting and is not an official Kentico product.**

## Library Version Matrix

| Xperience Version   | Library Version |
| ------------------- | --------------- |
| >= 29.0.0           | >= 4.1.0        |
| 28.x                | >= 3.x          |
| >= 26.2.0, < 27.0.0 | 2.x             |

## Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)
- [Typesense](https://typesense.org/docs/guide/)
-

## Package Installation

Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.Typesense
```

## Quick Start

1. Add configuration from your Typesense account to the ASP.NET Core `appsettings.json` file:

   ```json
    "CMSTypesenseSearch": {
    "ApiKey": "<your collectioning API key>",
    "Node": {
      "Host": "<your typesense host (localhost)>",
      "Port": <your typesense port (8108)>,
      "Protocol": "<your typesense protocol (http)>"
    }
   },
   ```

2. Define a custom `DefaultTypesenseCollectionStrategy` implementation to customize how content pages/content items are processed for the collection. See [`Custom-collection-strategy.md`](docs/Custom-collection-strategy.md)
3. Add this library to the application services, registering your custom `DefaultTypesenseCollectionStrategy` and Typesense services

   ```csharp
   // Program.cs
   services.AddKenticoTypesense(configuration);
   services.RegisterStrategy<GlobalTypesenseCollectionStrategy>("DefaultStrategy");
   ```

4. Create an collection in Xperience's Administration within the Search application added by this library.
   ![Administration search edit form](/images/xperience-administration-search-collection-edit-form.jpg)
5. Rebuild the collection in Xperience's Administration within the Search application added by this library.
   ![Administration search edit form](/images/xperience-administration-search-collection-list.jpg)
6. Display the results on your site with a Razor View 👍.

## Full Instructions

View the [Usage Guide](docs/Usage-Guide.md) for more detailed instructions.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](LICENSE.md) for more information.

## Support

We didn't provide support for this project, but you can submit an issue or a pull request. The feedbacks are welcome.
