namespace Kg.Velocity.Contracts.Nasa;

public sealed record NasaImageDto(
    string ImageUrl,
    string Title,
    string? Description
);
