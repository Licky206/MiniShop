namespace MiniShop.Application.Mapping;

public static class CategoryMappings
{
    // za IQueryable.Select u EfCategoryRepository
    public static readonly Expression<Func<Category, CategoryDto>> Projection = c =>
        new CategoryDto(c.Id, c.Name, c.Slug);

    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name, c.Slug);

    public static Category ToEntity(this CategoryCreateDto d) => new()
    {
        Name = d.Name.Trim(),
        Slug = d.Slug
    };
}