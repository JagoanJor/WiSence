using System.Collections.Generic;

namespace API.Responses
{
    public class ListResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }

        public ListResponse(IEnumerable<T> data, int total, int page)
        {
            Data = data;
            Total = total;
            Page = page;
        }
    }
}
