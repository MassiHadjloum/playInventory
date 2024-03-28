

namespace Play.Inventory.Service.Clients
{
    public class CatalogClinet {
      private readonly HttpClient httpClient;

      public CatalogClinet(HttpClient httpClient) {
        this.httpClient = httpClient;
      }

      // connect to catalog service on the /items url
      public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync(){
        var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/entities");
        return items!;
      }
    }
}
