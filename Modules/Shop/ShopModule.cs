using server.Core;
using AltV.Net;
using server.Events;
using server.Handlers.Event;
using server.Models;
using _logger = server.Logger.Logger;
using AltV.Net.Async;
using server.Handlers.Entities;
using server.Handlers.Vehicle;
using server.Util.Garage;
using server.Modules.Blip;

namespace server.Modules.Shop;

enum SHOP_TYPES
{
  ITEMSHOP = 1,
  WEAPONSHOP = 2,
  TOOLSHOP = 3
}

enum SHOP_SPRITES : int
{
  ITEMSHOP = 52,
  WEAPONSHOP = 110,
  TOOLSHOP = 566
}

enum SHOP_COLORS : int
{
  ITEMSHOP = 2,
  WEAPONSHOP = 1,
  TOOLSHOP = 81
}

class SHOP_NAMES
{
  static Dictionary<string, string> _names = new Dictionary<string, string>()
  {
    { "ITEMSHOP", "Supermarkt" },
    { "WEAPONSHOP", "Waffenladen" },
    { "TOOLSHOP", "Baumarkt" }
  };

  public static string GetName(string name)
  {
    return _names[name];
  }
}

class GaragenModule : ILoadEvent
{
  ServerContext _serverContext = new ServerContext();
  public static List<Models.Shop> shopList = new List<Models.Shop>();

  public static Dictionary<string, int> GetShopBlipByType(int type)
  {
    string typeName = Enum.GetName(typeof(SHOP_TYPES), type)!;

    Dictionary<string, int> dict = new Dictionary<string, int>();
    dict.Add("sprite", (int)Enum.Parse(typeof(SHOP_SPRITES), typeName));
    dict.Add("color", (int)Enum.Parse(typeof(SHOP_COLORS), typeName));
    return dict;
  }

  public async void OnLoad()
  {
    foreach (Models.Shop shop in _serverContext.Shop.ToList())
    {
      foreach (Models.ShopItems shopItems in _serverContext.ShopItems.Where(x => x.type == shop.type).ToList())
      {
        shop.items.Add(shopItems);
      }

      xEntity ped = new xEntity();
      ped.position = shop.Position;
      ped.dimension = (int)DIMENSIONEN.WORLD;
      ped.entityType = ENTITY_TYPES.PED;
      ped.range = 100;
      ped.data.Add("model", shop.ped);
      ped.data.Add("heading", shop.heading);
      ped.CreateEntity();

      Dictionary<string, int> blip = GetShopBlipByType(shop.type);
      Blip.Blip.Create(SHOP_NAMES.GetName(Enum.GetName(typeof(SHOP_TYPES), shop.type)!),
        blip["sprite"], blip["color"], 1, shop.Position);

      shopList.Add(shop);
    }
  }
}
