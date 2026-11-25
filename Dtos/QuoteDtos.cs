namespace Pazaryeri.Api.Dtos;

public record CreateQuoteDto(
    int ListingId, decimal? ProposedPrice, decimal Quantity, string? SwapOffer,
    DateOnly? StartDate, DateOnly? EndDate, string? Note);

public record CounterDto(decimal? ProposedPrice, decimal Quantity, string? SwapOffer, string? Note);