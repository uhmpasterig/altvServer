using server.Models;
using server.Handlers.Storage;
using server.Core;
using server.Events;
using Newtonsoft.Json;
using server.Handlers.Vehicle;
using _items = server.Modules.Items.Items;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using server.Modules.Items;
using server.Handlers.Player;
using _logger = server.Logger.Logger;

namespace server.Modules.Inventory;

public class InventoryModule : IPressedIEvent, ILoadEvent
{
  internal static IPlayerHandler playerHandler = new PlayerHandler();
  internal Dictionary<xPlayer, List<int>> userOpenInventorys = new Dictionary<xPlayer, List<int>>();

  public async Task<bool> OnKeyPressI(xPlayer player)
  {
    IVehicleHandler vehicleHandler = new VehicleHandler();
    IStorageHandler storageHandler = new StorageHandler();
    List<object> uiStorages = new List<object>();
    List<int> openInventorys = new List<int>();

    xStorage playerStorage = await storageHandler.GetStorage(player.playerInventorys["inventory"]);
    playerStorage.AddItem("weste", 2);
    uiStorages.Add(playerStorage.GetData());
    openInventorys.Add(playerStorage.id);

    if (player.IsInVehicle)
    {
      xVehicle vehicle = (xVehicle)player.Vehicle;
      xStorage gloveStorage = await storageHandler.GetStorage(vehicle.storageIdGloveBox);
      openInventorys.Add(gloveStorage.id);
      uiStorages.Add(gloveStorage.GetData());
      goto load;
    }
    xVehicle closestVehicle = vehicleHandler.GetClosestVehicle(player.Position);
    if (closestVehicle != null)
    {
      xStorage trunkStorage = await storageHandler.GetStorage(closestVehicle.storageIdTrunk);
      openInventorys.Add(trunkStorage.id);
      uiStorages.Add(trunkStorage.GetData());
      goto load;
    }
    xStorage closestStorage = storageHandler.GetClosestxStorage(player.Position);
    if (closestStorage != null)
    {
      openInventorys.Add(closestStorage.id);
      uiStorages.Add(closestStorage.GetData());
    }

  load:
    userOpenInventorys[player] = openInventorys;
    player.Emit("inventory:open", JsonConvert.SerializeObject(uiStorages));
    return true;
  }

  public async void OnLoad()
  {
    AltAsync.OnClient<IPlayer, int, int, int, int, int>("inventory:moveItem", async (player, fslot, tslot, fromStorage, toStorage, count) =>
    {
      xPlayer playerr = (xPlayer)player;
      IStorageHandler storageHandler = new StorageHandler();
      xStorage from = await storageHandler.GetStorage(fromStorage);
      xStorage to = await storageHandler.GetStorage(toStorage);
      
      InventoryItem item = from.items.Find(x => x.slot == fslot)!;
      InventoryItem item2 = to.items.Find(x => x.slot == tslot)!;
      if(count == 0){
        count = item.count;
      }
      
      if(item != null) {
        bool canMove1 = await to.DragAddItem(item);
        if (canMove1) {
          from.DragRemoveItem(fslot);
        }
      }

      if (item2 != null) {
        bool canMove2 = await from.DragAddItem(item2);
        if (canMove2) {
          to.DragRemoveItem(tslot);
        }
      }
      try {
        item!.slot = tslot;
        item2!.slot = fslot;
        to.items.Add(item);
        from.items.Add(item2);
      } catch (Exception e) {
        _logger.Log(e.Message);
      }


      List<object> uiStorages = new List<object>();
      foreach (int storageId in userOpenInventorys[(xPlayer)player])
      {
        xStorage storage = await storageHandler.GetStorage(storageId);
        uiStorages.Add(storage.GetData());
      }

      player.Emit("inventory:open", JsonConvert.SerializeObject(uiStorages));
    });
  }
}