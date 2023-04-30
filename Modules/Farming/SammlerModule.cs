using server.Core;
using server.Events;
using server.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using server.Enums;
using server.Handlers.Items;
using server.Handlers.Logger;

namespace server.Modules.Farming.Sammler;
public class SammlerMain : ILoadEvent, IPressedEEvent, IFiveSecondsUpdateEvent
{
  ILogger _logger;
  IItemHandler _itemHandler;

  public SammlerMain(ILogger logger, IItemHandler itemHandler)
  {
    _logger = logger;
    _itemHandler = itemHandler;
  }
  public static List<Farming_Collector> _sammler = new List<Farming_Collector>();
  private Dictionary<xPlayer, int> _farmingPlayers;

  public static async Task<xEntity> CreatePropForRoute(Farming_Prop prop)
  {
    xEntity _entity = new xEntity();
    _entity.entityType = ENTITY_TYPES.PROP;
    _entity.dimension = (int)DIMENSIONEN.WORLD;
    _entity.position = prop.Position;
    _entity.range = 250;
    _entity.data.Add("model", prop.model);
    _entity.data.Add("rotation", JsonConvert.SerializeObject(prop.Rotation));
    _entity.CreateEntity();

    return _entity;
  }

  public async void LoadSammler(Farming_Collector sammlerData)
  {

    foreach (Farming_Prop prop in sammlerData.Props)
    {
      xEntity entity = await CreatePropForRoute(prop);
      sammlerData.Entities.Add(entity);
    }
  }

  public async void OnLoad()
  {
    _farmingPlayers = new Dictionary<xPlayer, int>();
    await using ServerContext serverContext = new ServerContext();
    _sammler = await serverContext.Farming_Collectors.Include(f => f.Props).ToListAsync();
    _sammler.ForEach((sammler) =>
    {
      LoadSammler(sammler);
    });
  }

  public async Task<bool> OnKeyPressE(xPlayer player)
  {

    #region Checks if he can farm
    if (_farmingPlayers.ContainsKey(player))
    {
      _farmingPlayers.Remove(player);
      player.Emit("stopAnim");
      return true;
    };

    Farming_Collector _currentSammler = null!;
    // Get the Closest Sammler
    _sammler.ForEach((sammler) =>
    {
      if (sammler.Position.Distance(player.Position) < 180)
      {
        _currentSammler = sammler;
      }
    });
    if (_currentSammler == null) return false;
    // Get the Closest Prop of the closest Farming field
    xEntity _currentEntity = null!;
    _currentSammler.Entities.ForEach((entity) =>
    {
      if (entity.position.Distance(player.Position) < 3)
      {
        _currentEntity = entity;
      }
    });
    if (_currentEntity == null) return false;

    //TODO Check if he has the tool
    /* if (await player.HasItem(_currentSammler.tool) == false)
    {
      Item item = await _itemHandler.GetItem(_currentSammler.tool);
      player.SendMessage("Du benötigst ein/eine " + item.label, NOTIFYS.ERROR);
      return false;
    }; */
    #endregion
    _logger.Debug("Entity found");
    player.Emit("pointAtCoords", _currentEntity.position.X, _currentEntity.position.Y, _currentEntity.position.Z);
    await Task.Delay(1000);
    player.Emit("playAnim", _currentSammler.animation);

    _farmingPlayers.Add(player, _currentSammler.id);
    return true;
  }

  public Farming_Collector GetSammler(int id)
  {
    foreach (Farming_Collector sammler in _sammler)
    {
      if (sammler.id == id) return sammler;
    }
    return null!;
  }

  public async Task<bool> FarmingStep(xPlayer player, Farming_Collector feld)
  {
    if (player == null) return false;
    if (feld == null) return false;
    int random = new Random().Next(feld.amountmin, feld.amountmax);
    
    //TODO Check if he has the tool
    // await player.GiveItem(feld.item, random);
    Item item = await _itemHandler.GetItem(feld.item);
    player.SendMessage("Du hast " + random + " " + item.label + " gesammelt", NOTIFYS.INFO);

    return true;
  }

  public async void OnFiveSecondsUpdate()
  {
    foreach (KeyValuePair<xPlayer, int> kvp in _farmingPlayers)
    {
      if (kvp.Key == null)
      {
        _farmingPlayers.Remove(kvp.Key!);
        continue;
      };
      bool done = await FarmingStep(kvp.Key, GetSammler(kvp.Value));
      if (!done) _farmingPlayers.Remove(kvp.Key!);
    }
  }
}