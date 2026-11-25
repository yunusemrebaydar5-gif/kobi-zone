namespace Pazaryeri.Api.Dtos;

public record VerifyDto(bool Approve, string? RejectReason);
public record ReviewDto(byte Rating, string? Comment);
public record SendMessageDto(int ReceiverId, int? ListingId, int? QuoteId, string Body);
public record CreateTopicDto(int ForumCategoryId, string Title, string Body);
public record CreatePostDto(string Body);