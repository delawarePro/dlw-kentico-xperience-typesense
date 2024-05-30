using CMS.Core;
using CMS.EventLog;
using CMS.Tests;

using FluentAssertions;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;

namespace Kentico.Xperience.Typesense.Tests.Collectioning;

public class Tests : UnitTests
{
    private const string DEFAULT_LANGUAGE_NAME = "en-US";

    [TestCase("")]
    [TestCase(null)]
    [TestCase("   ")]
    public void IsCollectionedByCollection_Will_Throw_When_collectionName_Is_Is_Invalid(
        string? collectionName
    )
    {
        var fixture = new Fixture();

        var log = Substitute.For<IEventLogService>();

        var item = fixture.Create<CollectionEventWebPageItemModel>();

        var sut = () => item.IsCollectionedByCollection(log, collectionName!, "event");
        sut.Should().ThrowExactly<ArgumentNullException>();
    }

    [Test]
    public void IsCollectionedByCollection_Will_Throw_When_Item_Is_Is_Invalid()
    {
        var log = Substitute.For<IEventLogService>();

        CollectionEventWebPageItemModel item = null!;

        var sut = () => item.IsCollectionedByCollection(log, "index", "event");
        sut.Should().ThrowExactly<ArgumentNullException>();
    }

    [Test]
    public void IsCollectionedByCollection_Will_Return_False_When_The_Collection_Doesnt_Exist()
    {
        var log = Substitute.For<EventLogService>();

        var sut = GetDefaultCollectionEventWebPageItemModel();

        sut.IsCollectionedByCollection(log, "index", "event").Should().BeFalse();
    }

    [Test]
    public void IsCollectionedByCollection_Will_Return_False_When_The_Matching_Collection_Has_No_Matching_ContentTypes()
    {
        var log = Substitute.For<EventLogService>();

        IEnumerable<TypesenseCollectionIncludedPath> paths = [new("/path") { ContentTypes = [new("contentType", "contentType")], Identifier = "1" }];

        var index = new TypesenseCollection(new TypesenseConfigurationModel
        {
            ChannelName = "channel",
            Id = 2,
            CollectionName = "index2",
            LanguageNames = [DEFAULT_LANGUAGE_NAME],
            Paths = paths,
            RebuildHook = "/rebuild",
            StrategyName = "strategy"
        }, new() { { "strategy", typeof(DefaultTypesenseCollectionStrategy) } });
        TypesenseCollectionStore.Instance.AddCollection(index);

        var sut = GetDefaultCollectionEventWebPageItemModel();
        sut.ContentTypeName = paths.First().ContentTypes[0] + "-abc";

        sut.IsCollectionedByCollection(log, index.CollectionName, "event").Should().BeFalse();
    }

    [Test]
    public void IsCollectionedByCollection_Will_Return_False_When_The_Matching_Collection_Has_No_Matching_Paths()
    {
        var log = Substitute.For<EventLogService>();
        List<string> contentTypes = ["contentType"];

        IEnumerable<TypesenseCollectionIncludedPath> exactPaths = [new("/path") { ContentTypes = [new("contentType", "contentType")], Identifier = "1" }];

        var index1 = new TypesenseCollection(new TypesenseConfigurationModel
        {
            ChannelName = "channel",
            Id = 1,
            CollectionName = "index1",
            LanguageNames = [DEFAULT_LANGUAGE_NAME],
            Paths = exactPaths,
            RebuildHook = "/rebuild",
            StrategyName = "strategy"
        }, new() { { "strategy", typeof(DefaultTypesenseCollectionStrategy) } });
        TypesenseCollectionStore.Instance.AddCollection(index1);

        IEnumerable<TypesenseCollectionIncludedPath> wildcardPaths = [new("/home/%") { ContentTypes = [new("contentType", "contentType")], Identifier = "1" }];
        var index2 = new TypesenseCollection(new TypesenseConfigurationModel
        {
            ChannelName = "channel",
            Id = 2,
            CollectionName = "index2",
            LanguageNames = [DEFAULT_LANGUAGE_NAME],
            Paths = wildcardPaths,
            RebuildHook = "/rebuild",
            StrategyName = "strategy"
        }, new() { { "strategy", typeof(DefaultTypesenseCollectionStrategy) } });
        TypesenseCollectionStore.Instance.AddCollection(index2);

        var sut = GetDefaultCollectionEventWebPageItemModel();
        sut.ContentTypeName = contentTypes[0];
        sut.WebPageItemTreePath = exactPaths.First().AliasPath + "/abc";

        sut.IsCollectionedByCollection(log, index1.CollectionName, "event").Should().BeFalse();
        sut.IsCollectionedByCollection(log, index2.CollectionName, "event").Should().BeFalse();
    }

    [Test]
    public void IsCollectionedByCollection_Will_Return_True_When_The_Matching_Collection_Has_An_Exact_Path_Match()
    {
        var log = Substitute.For<EventLogService>();
        List<TypesenseCollectionContentType> contentTypes = [new("contentType", "contentType")];

        IEnumerable<TypesenseCollectionIncludedPath> exactPaths = [new("/path/abc/def") { ContentTypes = contentTypes, Identifier = "1" }];

        var index1 = new TypesenseCollection(new TypesenseConfigurationModel
        {
            ChannelName = "channel",
            Id = 1,
            CollectionName = "index1",
            LanguageNames = [DEFAULT_LANGUAGE_NAME],
            Paths = exactPaths,
            RebuildHook = "/rebuild",
            StrategyName = "strategy"
        }, new() { { "strategy", typeof(DefaultTypesenseCollectionStrategy) } });
        TypesenseCollectionStore.Instance.AddCollection(index1);

        IEnumerable<TypesenseCollectionIncludedPath> wildcardPaths = [new("/path/%") { ContentTypes = [new("contentType", "contentType")], Identifier = "1" }];

        var index2 = new TypesenseCollection(new TypesenseConfigurationModel
        {
            ChannelName = "channel",
            Id = 2,
            CollectionName = "index2",
            LanguageNames = [DEFAULT_LANGUAGE_NAME],
            Paths = wildcardPaths,
            RebuildHook = "/rebuild",
            StrategyName = "strategy"
        }, new() { { "strategy", typeof(DefaultTypesenseCollectionStrategy) } });
        TypesenseCollectionStore.Instance.AddCollection(index2);

        var sut = GetDefaultCollectionEventWebPageItemModel();
        sut.ContentTypeName = contentTypes[0].ContentTypeName;
        sut.WebPageItemTreePath = exactPaths.First().AliasPath;

        sut.IsCollectionedByCollection(log, index1.CollectionName, "event").Should().BeTrue();
        sut.IsCollectionedByCollection(log, index2.CollectionName, "event").Should().BeTrue();
    }

    [TearDown]
    public void TearDown() => TypesenseCollectionStore.Instance.SetIndicies([]);

    private CollectionEventWebPageItemModel GetDefaultCollectionEventWebPageItemModel()
    {
        var fixture = new Fixture();
        var sut = fixture.Create<CollectionEventWebPageItemModel>();

        sut.LanguageName = DEFAULT_LANGUAGE_NAME;

        return sut;
    }
}
