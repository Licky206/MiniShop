namespace MiniShop.Application.Dtos;

public record CategoryDto(int Id, string Name, string Slug);

public record CategoryCreateDto(string Name, string Slug);

public record CategoryUpdateDto(string Name, string Slug);