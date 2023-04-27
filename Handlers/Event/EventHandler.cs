using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using server.Handlers.Timer;
using server.Core;
using server.Events;
using server.Extensions;
using _logger = server.Logger.Logger;
using System.Diagnostics;
using System.Threading.Tasks;

namespace server.Handlers.Event;

public class EventHandler : IEventHandler
{
  private readonly ITimerHandler _timerHandler;
  private readonly IEnumerable<IPlayerConnectEvent> _playerConnectedEvents;
  private readonly IEnumerable<IPlayerDisconnectEvent> _playerDisconnectedEvents;
  private readonly IEnumerable<IPlayerDeadEvent> _playerDeadEvents;
  private readonly IEnumerable<ILoadEvent> _loadEvents;

  public readonly IEnumerable<IItemsLoaded> _itemsLoadedEvent;

  // Timer event
  private readonly IEnumerable<IFiveSecondsUpdateEvent> _fiveSecondsUpdateEvents;
  private readonly IEnumerable<IOneMinuteUpdateEvent> _oneMinuteUpdateEvents;
  private readonly IEnumerable<ITwoMinuteUpdateEvent> _twoMinuteUpdateEvents;

  // keypress event
  private readonly IEnumerable<IPressedEEvent> _pressedEEvents;
  private readonly IEnumerable<IPressedIEvent> _pressedIEvents;

  // timer event


  public EventHandler(ITimerHandler timerHandler,
                      IEnumerable<IPlayerConnectEvent> playerConnectedEvents,
                      IEnumerable<IPlayerDisconnectEvent> playerDisconnectEvents,
                      IEnumerable<IPlayerDeadEvent> playerDeadEvents,
                      IEnumerable<ILoadEvent> loadEvents,
                      IEnumerable<IItemsLoaded> itemsLoadedEvent,
                      
                      IEnumerable<IFiveSecondsUpdateEvent> fiveSecondsUpdateEvents,
                      IEnumerable<IOneMinuteUpdateEvent> oneMinuteUpdateEvents,
                      IEnumerable<ITwoMinuteUpdateEvent> twoMinuteUpdateEvents,

                      IEnumerable<IPressedEEvent> pressedEEvents,
                      IEnumerable<IPressedIEvent> pressedIEvents
                      )
  {
    AltAsync.OnClient<IPlayer>("PressE", OnKeyPressE);
    AltAsync.OnClient<IPlayer>("PressI", OnKeyPressI);
    AltAsync.OnServer("ItemsLoaded", ItemsLoaded);
    _timerHandler = timerHandler;
    _playerConnectedEvents = playerConnectedEvents;
    _playerDisconnectedEvents = playerDisconnectEvents;
    _playerDeadEvents = playerDeadEvents;
    _loadEvents = loadEvents;
    _itemsLoadedEvent = itemsLoadedEvent;

    _fiveSecondsUpdateEvents = fiveSecondsUpdateEvents;
    _oneMinuteUpdateEvents = oneMinuteUpdateEvents;
    _twoMinuteUpdateEvents = twoMinuteUpdateEvents;

    _pressedEEvents = pressedEEvents;
    _pressedIEvents = pressedIEvents;
  }

  public Task LoadHandlers()
  {
    foreach (var loadEvent in _loadEvents)
    {
      _logger.Debug($"Loading event handler: {loadEvent.GetType().Name}");
      loadEvent.OnLoad();
    }
    _logger.Debug("Loading event handlers");

    AltAsync.OnPlayerConnect += async (IPlayer player, string reason) =>
      _playerConnectedEvents?.ForEach(playerConnectEvent => playerConnectEvent.OnPlayerConnect(player, reason));

    AltAsync.OnPlayerDisconnect += async (IPlayer player, string reason) =>
      _playerDisconnectedEvents?.ForEach(playerDisconnectEvent => playerDisconnectEvent.OnPlayerDisconnect(player, reason));

    AltAsync.OnPlayerDead += async (IPlayer player, IEntity killer, uint weapon) =>
      _playerDeadEvents?.ForEach(playerDeadEvent => playerDeadEvent.OnPlayerDeath(player, killer, weapon));

    _timerHandler.AddInterval(1000 * 5, async (s, e) =>
      _fiveSecondsUpdateEvents?.ForEach(fiveSecondsUpdateEvent => fiveSecondsUpdateEvent.OnFiveSecondsUpdate()));
    
    _timerHandler.AddInterval(1000 * 60, async (s, e) =>
      _oneMinuteUpdateEvents?.ForEach(oneMinuteUpdateEvent => oneMinuteUpdateEvent.OnOneMinuteUpdate()));

    _timerHandler.AddInterval(1000 * 60 * 2, async (s, e) =>
      _twoMinuteUpdateEvents?.ForEach(twoMinuteUpdateEvent => twoMinuteUpdateEvent.OnTwoMinuteUpdate()));
      
    return Task.CompletedTask;
  }

  public async void OnCommand(IPlayer iplayer, string commandName)
  {
    xPlayer player = (xPlayer)iplayer;
  }

  public async void OnKeyPressE(IPlayer iplayer)
  {
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    xPlayer player = (xPlayer)iplayer;
    if (!player.CanInteract()) return;
    foreach (var pressedEEvent in _pressedEEvents)
    {
      if (await pressedEEvent.OnKeyPressE((xPlayer)player)) return;
    }
    stopwatch.Stop();
    _logger.Debug($"OnKeyPressE took {stopwatch.ElapsedMilliseconds}ms | {stopwatch.ElapsedTicks} ticks");
  }

  public async void OnKeyPressI(IPlayer iplayer)
  {
    xPlayer player = (xPlayer)iplayer;
    if (!player.CanInteract()) return;
    foreach (var pressedIEvent in _pressedIEvents)
    {
      if (await pressedIEvent.OnKeyPressI((xPlayer)player)) return;
    }
  }

  public void ItemsLoaded()
  {
    _logger.Info("ItemsLoaded");
    foreach (var itemsLoadedEvent in _itemsLoadedEvent)
    {
      itemsLoadedEvent.ItemsLoaded();
    }
  }
}