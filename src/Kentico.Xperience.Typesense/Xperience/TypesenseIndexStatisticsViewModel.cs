namespace Kentico.Xperience.Typesense.Xperience;

public class TypesenseCollectionStatisticsViewModel
{
    //
    // Summary:
    //     Collection name.
    public string? Name { get; set; }

    //
    // Summary:
    //     Date of last update.
    public DateTime UpdatedAt { get; set; }

    //
    // Summary:
    //     Number of records contained in the index
    public long NumberOfDocuments { get; set; }

}
