using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.Tests.Tests;

internal class RebuildItemSelectorTests
{
    private static readonly string[] CollectionLanguages = ["nl-be", "fr-fr"];

    private static CollectionEventReusableItemModel Item(Guid guid, string language) =>
        new(1, guid, language, "Pierret.DealerDetails", "dealer", false, 1, 1);

    [Test]
    public void DropsFallbackCopy_WhenItemWasFoundInACollectionLanguage()
    {
        var guid = Guid.NewGuid();
        var nlBe = Item(guid, "nl-be");
        // fr-fr pass: no fr-fr version, falls back to fr-be.
        var frBeFallback = Item(guid, "fr-be");

        var result = RebuildItemSelector.KeepBestLanguagePerItem([nlBe, frBeFallback], CollectionLanguages);

        Assert.That(result, Is.EqualTo(new[] { nlBe }));
    }

    [Test]
    public void KeepsFallbackItemOnce_WhenItemExistsOnlyThroughFallback()
    {
        var guid = Guid.NewGuid();

        // Both the nl-be and the fr-fr pass fall back to the same fr-be version.
        var result = RebuildItemSelector.KeepBestLanguagePerItem([Item(guid, "fr-be"), Item(guid, "fr-be")], CollectionLanguages);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].LanguageName, Is.EqualTo("fr-be"));
    }

    [Test]
    public void KeepsEveryCollectionLanguageVersion()
    {
        var guid = Guid.NewGuid();

        var result = RebuildItemSelector.KeepBestLanguagePerItem([Item(guid, "nl-be"), Item(guid, "fr-fr"), Item(guid, "fr-be")], CollectionLanguages);

        Assert.That(result.Select(item => item.LanguageName), Is.EquivalentTo(new[] { "nl-be", "fr-fr" }));
    }

    [Test]
    public void HandlesEachItemIndependently()
    {
        var withNlBe = Guid.NewGuid();
        var frBeOnly = Guid.NewGuid();

        var result = RebuildItemSelector.KeepBestLanguagePerItem(
            [Item(withNlBe, "nl-be"), Item(withNlBe, "fr-be"), Item(frBeOnly, "fr-be")],
            CollectionLanguages);

        Assert.That(result.Select(item => (item.ItemGuid, item.LanguageName)), Is.EquivalentTo(new[] { (withNlBe, "nl-be"), (frBeOnly, "fr-be") }));
    }
}
