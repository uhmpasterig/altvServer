using server.Core;
using server.Events;
using _logger = server.Logger.Logger;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using server.Models;
using server.Handlers.Storage;
using server.Handlers.Event;

namespace server.Modules.Items;

public class xItem : Models.Item
{
  public xItem(Models.Item item)
  {
    this.id = item.id;
    this.name = item.name;
    this.stackSize = item.stackSize;
    this.weight = item.weight;
    this.job = item.job;
    this.data = item.data;
  }
}

public class InventoryItem
{
  public int id { get; set; }
  public string name { get; set; }
  public int stackSize { get; set; }
  public float weight { get; set; }
  public string job { get; set; }
  public string data { get; set; }
  public int slot { get; set; }
  public int count { get; set; }
  
  public InventoryItem()
  {
  }

  public InventoryItem(int id, string name, int stackSize, float weight, string job, string data, int slot, int count)
  {
    this.id = id;
    this.name = name;
    this.stackSize = stackSize;
    this.weight = weight;
    this.job = job;
    this.data = data;

    this.slot = slot;
    this.count = count;
  }

  public InventoryItem(xItem item, int slot, int count)
  {
    this.id = item.id;
    this.name = item.name;
    this.stackSize = item.stackSize;
    this.weight = item.weight;
    this.job = item.job;
    this.data = item.data;

    this.slot = slot;
    this.count = count;
  }
}

public class Items : ILoadEvent
{
  public static Dictionary<string, xItem> _items = new Dictionary<string, xItem>();
  public static Dictionary<string, Action<xPlayer>> _usableItems = new Dictionary<string, Action<xPlayer>>();

  public static void RegisterUsableItem(string itemname, Action<xPlayer> action)
  {
    _logger.Startup($"Registering usable item {itemname}");
    if (!_items.ContainsKey(itemname))
    {
      _logger.Error($"Item {itemname} does not exist");
      return;
    }

    _usableItems.Add(itemname, action);
  }

  public static void UseItem(xPlayer player, string itemname)
  {
    if (!_items.ContainsKey(itemname))
    {
      _logger.Error($"Item {itemname} does not exist");
      return;
    }

    foreach (var action in _usableItems.Where(x => x.Key == itemname))
    {
      action.Value(player);
    }
  }

  public async static void UseItemFromSlot(xPlayer player, int slot, int storageId)
  {
    IStorageHandler handler = new StorageHandler();
    xStorage storage = await handler.GetStorage(storageId);
    if (storage == null)
    {
      _logger.Error($"Storage with id {storageId} does not exist");
      return;
    }
    InventoryItem item = storage.items.Find(x => x.slot == slot)!;
    if (item == null)
    {
      _logger.Error($"Item with slot {slot} does not exist");
      return;
    }
    if (!_items.ContainsKey(item.name))
    {
      _logger.Error($"Item {item.name} does not exist");
      return;
    }
    if(!storage.RemoveItem(slot)) {
      _logger.Error($"Could not remove item {item.name} from storage {storageId}");
      return;
    }
    _logger.Debug($"Item {item.name} used");
    foreach (var action in _usableItems.Where(x => x.Key == item.name))
    {
      action.Value(player);
    }
  }

  public async static void RemoveItemFromSlot(int slot, int storageId, int count)
  {
    IStorageHandler handler = new StorageHandler();
    xStorage storage = await handler.GetStorage(storageId);
    if (storage == null)
    {
      _logger.Error($"Storage with id {storageId} does not exist");
      return;
    }
    InventoryItem item = storage.items.Find(x => x.slot == slot)!;
    if (item == null)
    {
      _logger.Error($"Item with slot {slot} does not exist");
      return;
    }
    if (!_items.ContainsKey(item.name))
    {
      _logger.Error($"Item {item.name} does not exist");
      return;
    }
    if(!storage.RemoveItem(slot, count)) {
      _logger.Error($"Could not remove item {item.name} from storage {storageId}");
      return;
    }
  }

  

  public static xItem GetItem(string itemname)
  {
    if (!_items.ContainsKey(itemname))
    {
      _logger.Error($"Item {itemname} does not exist");
      return null;
    }

    return _items[itemname];
  }

  public static xItem GetItem(int id)
  {
    foreach (var item in _items)
    {
      if (item.Value.id == id)
      {
        return item.Value;
      }
    }

    _logger.Error($"Item with id {id} does not exist");
    return null;
  }

  public async void OnLoad()
  {
    await using ServerContext serverContext = new ServerContext();
    
    _logger.Startup("Lade Items!");
    foreach (Models.Item item in serverContext.Items)
    {
      xItem iitem = new xItem(item);
      _items.Add(iitem.name, iitem);
      _logger.Debug($"Item {iitem.name} geladen");
    }
    
    _logger.Startup($"x{_items.Count } items geladen");
    Alt.Emit("ItemsLoaded");
  }
}