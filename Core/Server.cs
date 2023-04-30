using AltV.Net;
using server.Models;

using server.Handlers.Logger;
using server.Handlers.Event;
using server.Handlers.Timer;
using server.Handlers.Vehicle;
using server.Handlers.Player;
using server.Handlers.Storage;
using server.Handlers.Items;


using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.SpatialPartitions;
using AltV.Net.EntitySync.ServerEvent;



namespace server.Core;

public class Server : IServer
{
  ILogger _logger = new Logger();
  ServerContext _serverContext;
  IEventHandler _eventHandler;
  ITimerHandler _timerHandler;
  IVehicleHandler _vehicleHandler;
  IPlayerHandler _playerHandler;
  IStorageHandler _storageHandler;
  IItemHandler _itemHandler;

  public Server(ILogger logger, ServerContext serverContext, IVehicleHandler vehicleHandler, IPlayerHandler playerHandler, IEventHandler eventHandler, ITimerHandler timerHandler, IStorageHandler storageHandler, IItemHandler itemHandler)
  {
    _logger = logger;
    _serverContext = serverContext;
    _vehicleHandler = vehicleHandler;
    _playerHandler = playerHandler;
    _eventHandler = eventHandler;
    _timerHandler = timerHandler;
    _storageHandler = storageHandler;
    _itemHandler = itemHandler;
  }

  public void Start()
  {
    _logger.Startup("Server startet...");

    _logger.Startup("AltEntitySync initialisieren...");
    AltEntitySync.Init(1, (threadId) => 100, (threadId) => false,
      (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
      (entity, threadCount) => (entity.Id % threadCount),
      (entityId, entityType, threadCount) => (entityId % threadCount),
      (threadId) => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 300),
      new IdProvider());
    _logger.Startup("AltEntitySync initialisiert!");

    _logger.Startup("Lade Handler...");
    _eventHandler.LoadHandlers();
    _logger.Startup("Handler Geladen!");

    _logger.Startup("Lade Timer...");
    _logger.Startup("Timer Geladen!");
    foreach (Models.Player _player in _serverContext.Players.ToList())
    {
      _player.isOnline = false;
    }

    _serverContext.SaveChangesAsync();
    _logger.Startup("Server gestartet!");

  }

  public async Task SaveAll()
  {
    await _storageHandler.SaveAllStorages();
    await _vehicleHandler.SaveAllVehicles();
    await _playerHandler.SaveAllPlayers();
  }

  public async Task Stop()
  {
    _timerHandler.StopAllIntervals();
    await SaveAll();

    foreach (IPlayer player in Alt.GetAllPlayers())
    {
      player.Kick("Server wurde gestoppt");
    }
  }
}