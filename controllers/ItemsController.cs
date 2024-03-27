using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{

  [ApiController]
  [Route("items")]
  public class ItemsController : ControllerBase
  {
    private readonly IRepository<InventoryItem> itemsRepository;

    public ItemsController(IRepository<InventoryItem> repository)
    {
      this.itemsRepository = repository;
    }

    [HttpGet("/allItems")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAllAsync()
    {
      var items = (await itemsRepository.GetAllAsync())
      .Select(item => item.AsDto());
      Console.WriteLine(items);

      return Ok(items);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
      if (userId == Guid.Empty)
      {
        return BadRequest();
      }
      var items = (await itemsRepository.GetAllAsync(item => item.UserId == userId))
      .Select(item => item.AsDto());
      Console.WriteLine(items);

      return Ok(items);
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