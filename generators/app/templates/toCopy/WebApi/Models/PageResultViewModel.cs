using System.Collections.Generic;

namespace WebApi.Models
{
    public class PageResultViewModel<TModel>
    {
        public PageResultViewModel()
        {
            PageNumber = 0;
            PageSize = 10;
            SearchTerm = string.Empty;

        }

        public IEnumerable<TModel> Results { get; set; }
        public int Total { get; set; }
        public uint PageNumber { get; set; }
        public uint PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
}
