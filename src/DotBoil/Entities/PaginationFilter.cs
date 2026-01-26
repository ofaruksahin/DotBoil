using DotBoil.Enums;

namespace DotBoil.Entities;

public class PaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; }
    public EnumSortDirection SortDirection { get; set; }

    public PaginationFilter()
    {
    }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}