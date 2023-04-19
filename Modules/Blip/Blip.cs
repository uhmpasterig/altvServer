using Newtonsoft.Json;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net;
using server.Events;
using server.Core;

namespace server.Modules.Blip;

/* public enum BlipColor {
  WHITE = 0,
  RED = 1,
  GREEN = 2,
  BLUE = 3,
  YELLOW = 5,
  LIGHT_RED = 6
} */
public class Blip : IPlayerConnectEvent
{
  public static List<xBlip> All = new List<xBlip>();

  public static void Create(string name, int sprite, int color, int scale, Position position)
  {
    var blip = new xBlip(name, sprite, color, scale, position);
    All.Add(blip);
    Alt.EmitAllClients("Blip:Create", JsonConvert.SerializeObject(blip));
  }

  public void OnPlayerConnect(IPlayer player, string reason)
  {
    foreach (var blip in All)
    {
      player.Emit("Blip:Create", JsonConvert.SerializeObject(blip));
    }
  }
}


public class xBlip
{
  public xBlip(string _name, int _sprite, int _color, int _scale, Position _position)
  {
    name = _name;
    sprite = _sprite;
    color = _color;
    scale = _scale;
    Position = _position;
  }

  public string name { get; set; }
  public int sprite { get; set; }
  public int color { get; set; }
  public int scale { get; set; }

  public Position Position { get; set; }
}