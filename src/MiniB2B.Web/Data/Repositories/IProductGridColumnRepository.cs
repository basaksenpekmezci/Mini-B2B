using MiniB2B.Web.Domain;

namespace MiniB2B.Web.Data.Repositories;

public interface IProductGridColumnRepository
{
    Task<IEnumerable<ProductGridColumn>> GetVisibleColumnsAsync();
}
