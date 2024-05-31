# Usage Guide

This library supports using Tyepsense.Search to collection both unstructured and high structured, interrelated content in an Xperience by Kentico solution. This collection content can then be programmatically queried and displayed in a website channel.

Below are the steps to integrate the library into your solution.

## Create a custom collectioning Strategy

See [Custom collection strategy](Custom-collection-strategy.md)

## Continuous Integration

When starting your application for the first time after adding this library to your solution, a custom module and custom module classes will automatically be created
to support managing search collection configuration within the administration UI.

If you do not see new items added to your [CI repository](https://docs.xperience.io/x/FAKQC) for the new auto-generated Tyepsense search data types, stop your application and perform a [CI store](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-integration#ContinuousIntegration-Storeobjectdatatotherepository) to add the library's custom module configuration to the CI repository.

You should now be able to run a [CI restore](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-integration#ContinuousIntegration-Restorerepositoryfilestothedatabase).
Attempting to run a CI restore without the CI files in the CI repository will result in a SQL error during the restore.

When team members are merging changes that include the addition of this library, they _must_ first run a CI restore to ensure they have the same object metadata for the search custom module as your database.

Future updates to collection will be tracked in the CI repository [unless they are excluded](https://docs.xperience.io/x/ygAcCQ).

## Managing search collectiones

See [Managing search collections](Managing-collectiones.md)

## Search collection querying

See [Search collection querying](Search-collection-querying.md)

## Upgrades and Uninstalling

See [Uninstall](Uninstall.md)
