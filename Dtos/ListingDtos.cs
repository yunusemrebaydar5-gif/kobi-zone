namespace Pazaryeri.Api.Dtos;

public record LaborDto(string SkillTitle, int HeadCount, int? ExperienceYears, string? Certifications);

public record CreateListingDto(
    int CategoryId, string ListingType, string Title, string? About,
    decimal? Price, string? PriceUnit, decimal Quantity, string? Condition,
    string? SwapWanted, DateOnly? AvailableFrom, DateOnly? AvailableTo,
    string? City, LaborDto? Labor);