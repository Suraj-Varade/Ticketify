namespace Core.RequestHelpers;

public class TicketParams
{
    private int _pageNumber = 1;
    private int _maxPageSize = 10;
    private int _pageSize = 5;

    public int PageSize
    {
        set
        {
            _pageSize = (value > _maxPageSize || value < 1) ? _maxPageSize : value;
        }
        get
        {
            return _pageSize;
        }
    }

    public int PageNumber
    {
        set
        {
            _pageNumber = (value < 1) ? 1 : value;
        }
        get
        {
            return _pageNumber;
        }
    }
    
    public string OrderBy { get; set; } = "CreatedAt";
    
    //filtering
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? AssignTo { get; set; }
    public int? CreatedBy { get; set; }
}