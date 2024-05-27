using CMS.Core;
using CMS.Tests;

using DancingGoat.Models;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;
using Kentico.Xperience.Typesense.Tests.Base;
namespace Kentico.Xperience.Typesense.Tests.Tests;

internal class CollectionedItemModelExtensionsTests : UnitTests
{
    [Test]
    public void IsCollectionedByCollection()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        Assert.That(model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }

    [Test]
    public void WildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();
        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Collection;
        var path = new TypesenseCollectionIncludedPath("/%") { ContentTypes = [new(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))] };

        index.IncludedPaths = new List<TypesenseCollectionIncludedPath>() { path };

        TypesenseCollectionStore.Instance.AddCollection(index);

        Assert.That(model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }

    [Test]
    public void WrongWildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Collection;
        var path = new TypesenseCollectionIncludedPath("/Collection/%") { ContentTypes = [new("contentType", "contentType")] };

        index.IncludedPaths = new List<TypesenseCollectionIncludedPath>() { path };

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(index);

        Assert.That(!model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }

    [Test]
    public void WrongPath()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Collection;
        var path = new TypesenseCollectionIncludedPath("/Collection") { ContentTypes = [new("contentType", "contentType")] };

        index.IncludedPaths = new List<TypesenseCollectionIncludedPath>() { path };

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(index);

        Assert.That(!model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }

    [Test]
    public void WrongContentType()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.ContentTypeName = "DancingGoat.HomePage";

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);

        Assert.That(!model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }

    [Test]
    public void WrongCollection()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        Assert.That(!model.IsCollectionedByCollection(log, "NewCollection", MockDataProvider.EventName));
    }

    [Test]
    public void WrongLanguage()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.LanguageName = "sk";

        TypesenseCollectionStore.Instance.SetIndicies(new List<TypesenseConfigurationModel>());
        TypesenseCollectionStore.Instance.AddCollection(MockDataProvider.Collection);

        Assert.That(!model.IsCollectionedByCollection(log, MockDataProvider.DefaultCollection, MockDataProvider.EventName));
    }
}
