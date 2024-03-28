using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{

  [ApiController]
  [Route("items")]
  public class ItemsController : ControllerBase
  {
    private readonly IRepository<InventoryItem> itemsRepository;

    private readonly CatalogClinet catalogClinet;    

    public ItemsController(IRepository<InventoryItem> repository, CatalogClinet catalogClinet)
    {
      this.itemsRepository = repository;
      this.catalogClinet = catalogClinet;
    }

    // [HttpGet("/allItems")]
    // public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAllAsync()
    // {
    //   var items = (await itemsRepository.GetAllAsync())
    //   .Select(item => item.AsDto());
    //   Console.WriteLine(items);

    //   return Ok(items);
    // }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
      if (userId == Guid.Empty)
      {
        return BadRequest();
      }
      
      var catalogItems = await catalogClinet.GetCatalogItemsAsync();
      var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.UserId == userId);
      
      var inventoryItemsDtos = inventoryItemEntities.Select(inventoryItem => {
        var catalogItem = catalogItems.Single(item => item.Id == inventoryItem.CatalogItemId);
        return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
      });

      return Ok(inventoryItemsDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
      var inventoryItem = await itemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId
      && item.CatalogItemId == grantItemsDto.CatalogItemId);

      if (inventoryItem == null)
      {
        inventoryItem = new InventoryItem
        {
          UserId = grantItemsDto.UserId,
          CatalogItemId = grantItemsDto.CatalogItemId,
          Quantity = grantItemsDto.Quantity,
          AcquiredDate = DateTimeOffset.UtcNow,
        };
        await itemsRepository.CreateAsync(inventoryItem);
      }
      else
      {
        inventoryItem.Quantity += grantItemsDto.Quantity;
        await itemsRepository.UpdateAsync(inventoryItem);
      }
      return Ok();
    }
  }
}