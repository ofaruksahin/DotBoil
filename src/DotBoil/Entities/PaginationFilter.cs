using DotBoil.Enums;

namespace DotBoil.Entities;

public class PaginationFilter
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public string SortColumn { get; private set; }
    public EnumSortDirection SortDirection { get; private set; }

    public PaginationFilter()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}