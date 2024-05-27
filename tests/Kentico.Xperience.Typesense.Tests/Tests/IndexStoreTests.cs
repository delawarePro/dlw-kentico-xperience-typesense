using CMS.Tests;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;
using Kentico.Xperience.Typesense.Tests.Base;

namespace Kentico.Xperience.Typesense.Tests.Tests;
internal class CollectionStoreTests : UnitTests
{

    [Test]
    public void AddAndGetCollection()
    {
        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());

        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.GetCollection("TestCollection", 1));

        Assert.Multiple(() =>
        {
            Assert.That(TypesenseCollectionStore.Instance.GetCollection("TestCollection") is not null);
            Assert.That(TypesenseCollectionStore.Instance.GetCollection(MockDataProvider.DefaultCollection) is not null);
        });
    }

    [Test]
    public void AddCollection_AlreadyExists()
    {
        var fixture = new Fixture();
        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);

        bool hasThrown = false;

        try
        {
            TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);
        }
        catch
        {
            hasThrown = true;
        }

        Assert.That(hasThrown);
    }

    [Test]
    public void SetIndicies()
    {
        var defaultCollection = new TypesenseConfigurationModel { collectionName = "DefaultCollection", Id = 0 };
        var simpleCollection = new TypesenseConfigurationModel { collectionName = "SimpleCollection", Id = 1 };

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>() { defaultCollection, simpleCollection });

        Assert.Multiple(() =>
        {
            Assert.That(TypesenseCollectionStore.Instance.GetCollection(defaultCollection.collectionName) is not null);
            Assert.That(TypesenseCollectionStore.Instance.GetCollection(simpleCollection.collectionName) is not null);
        });
    }

    [TearDown]
    public void TearDown() => TypesenseCollectionStore.Instance.SetIndicies([]);
}
