namespace DotBoil.Entities;

public class PaginatedModel<TModel>
    where TModel : class
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalRecords { get; private set; }
    public IEnumerable<TModel> Items { get; private set; }

    public PaginatedModel()
    {
    }

    public PaginatedModel(int pageNumber, int pageSize, int totalPages, int totalRecords, IEnumerable<TModel> items)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        TotalRecords = totalRecords;
        Items = items;
    }
}