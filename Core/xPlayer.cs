using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async.Elements.Entities;
using AltV.Net.Async;
using server.Modules.Weapons;
using Newtonsoft.Json;

using AltV.Net.Resources.Chat.Api;
using AltV.Net.Data;
using server.Modules.Clothing;
using server.Handlers.Storage;
using server.Models;
using server.Handlers.Vehicle;
using server.Config.Weapons;
using server.Enums;
namespace server.Core;

#region enums
public enum DIMENSIONEN
{
  WORLD,
  HOUSE,
  CAMPER,
  STORAGEROOM,
  PVP
}

public enum NOTIFYS
{
  INFO,
  ERROR,
  SUCCESS,
  WARNING
}
#endregion

public partial class xPlayer : AsyncPlayer
{
  private string[] _notifys = new string[] { "default", "error", "success", "warning" };
  public int id { get; set; }
  public string name { get; set; }
  public string ped { get; set; }
  public int cash { get; set; }
  public int bank { get; set; }
  public Dictionary<int, int> boundStorages { get; set; } = new Dictionary<int, int>();
  public List<xWeapon> weapons { get; set; } = new List<xWeapon>();
  public DateTime creationDate { get; set; }
  public DateTime lastLogin { get; set; }
  public Dictionary<string, object> dataCache { get; set; } = new Dictionary<string, object>();

  public Player_Skin player_skin { get; set; }
  public Player_Cloth player_cloth { get; set; }
  public List<Vehicle_Key> vehicle_keys { get; set; }
  public Player_Society player_society { get; set; }
  public Player_Factory player_factory { get; set; }

  public int isDead { get; set; }

  public xPlayer(ICore core, IntPtr nativePointer, ushort id) : base(core, nativePointer, id)
  {
  }

  public async Task SetDataFromDatabase(Models.Player _player)
  {
    this.id = _player.id;
    this.name = _player.name;
    this.ped = _player.ped;

    this.cash = _player.cash;
    this.bank = _player.bank;
    this.boundStorages = _player.boundStorages;
    this.weapons = _player.weapons;
    this.dataCache = _player.dataCache;

    this.creationDate = _player.creationDate;
    this.lastLogin = _player.lastLogin;
    this.dataCache = _player.dataCache;

    this.player_skin = _player.player_skin;
    this.player_cloth = _player.player_cloth;
    this.vehicle_keys = _player.vehicle_keys;
    this.player_society = _player.player_society;
    this.player_factory = _player.player_factory;
  }

  #region Methods

  #region UiStuff

  public void SendMessage(string message, NOTIFYS notifyType)
  {
    this.SendMessage("SERVER", message, 5000, notifyType);
  }

  public void SendMessage(string title, string text, int time, NOTIFYS notifyType)
  {
    this.Emit("clientNotify", title, text, time, _notifys[(int)notifyType]);
  }

  public async Task StartProgressBarAsync(int time)
  {
    this.Emit("clientProgressbarStart", time);
    await Task.Delay(time).ContinueWith(_ =>
    {
      this.Emit("clientProgressbarStop");
      return Task.CompletedTask;
    });
  }

  public void StartProgressBar(int time)
  {
    this.Emit("clientProgressbarStart", time);
  }

  public void StopProgressBar()
  {
    this.Emit("clientProgressbarStop");
  }

  public bool CanInteract()
  {
    if (this.isDead == 1)
    {
      this.SendMessage("Du kannst nichts machen, während du tot bist!", NOTIFYS.ERROR);
      return false;
    }

    return true;
  }
  #endregion

  #region Inventory and Weapon Stuff
  public void SetPlayerInventoryId(int key, int value)
  {
    if (boundStorages.ContainsKey(key))
    {
      boundStorages[key] = value;
    }
    else
    {
      boundStorages.Add(key, value);
    }
  }

  public void LoadWeaponsFromDb(string weapons)
  {
    this.weapons = JsonConvert.DeserializeObject<List<xWeapon>>(weapons)!;
    foreach (xWeapon weapon in this.weapons)
    {
      this.GiveWeapon(Alt.Hash(weapon.name), weapon.ammo, false);
    }
  }

  public async Task<bool> GiveSavedWeapon(string name, int ammo = 100, bool hold = false, string job = null!)
  {
    if (await WeaponConfig.IsValidWeapon(name.ToLower()) == false)
    {
      this.SendMessage("Dieses Waffe existiert nicht!", NOTIFYS.ERROR);
      return false;
    }

    if (weapons.Find(x => x.name == name.ToLower()) != null)
    {
      this.SendMessage("Du hast dieses Waffe bereits!", NOTIFYS.ERROR);
      return false;
    }

    xWeapon weapon = new xWeapon(0, name, ammo, job);
    this.weapons.Add(weapon);
    this.GiveWeapon(Alt.Hash(name), ammo, hold);
    return true;
  }
  #endregion

  #region Death and Revive
  public void SetDead(int isDead)
  {
    this.isDead = isDead;
    if (isDead == 1)
    {
      this.Spawn(this.Position, 0);
      this.Health = this.MaxHealth;
      this.Invincible = true;
    }
    this.Emit("player:dead", isDead);
  }

  public void Revive()
  {
    this.SetDead(0);
    this.ClearBloodDamage();
    this.Spawn(this.Position, 0);
    this.Health = this.MaxHealth;
  }

  #endregion

  #region Money
  public async void GiveMoney(int amount)
  {
    this.cash += amount;
  }

  public async void RemoveMoney(int amount)
  {
    this.cash -= amount;
  }

  public async Task<bool> HasMoney(int amount)
  {
    return this.cash >= amount;
  }

  public async void SaveMoney()
  {
    ServerContext _serverContext = new ServerContext();
    Models.Player? player = await _serverContext.Players.FindAsync(this.id);
    player.cash = this.cash;
    player.bank = this.bank;
    await _serverContext.SaveChangesAsync();
  }
  #endregion

  #region Skin
  public async Task LoadSkin(Player_Skin? skin = null)
  {
    if (skin == null) skin = this.player_skin;
    this.SetHeadBlendData(
            skin.shape1,
            skin.shape2,
            0,
            skin.skin1,
            skin.skin2,
            0,
            skin.shapeMix,
            skin.skinMix,
            0);

    this.SetEyeColor(skin.eyeColor);
    this.HairColor = skin.hairColor;
    this.HairHighlightColor = skin.hairColor2;
    this.SetClothes(2, skin.hair, skin.hair2, 0);
  }

  public async Task SetClothPiece(int id)
  {
    Models.Cloth? cloth = ClothModule.GetCloth(id);
    if (cloth == null) return;
    this.SetDlcClothes(cloth.component, cloth.drawable, cloth.texture, cloth.palette, cloth.dlc);
  }

  public async Task LoadClothes(Player_Cloth? cloth = null)
  {
    if (cloth == null) cloth = this.player_cloth;
    foreach (int id in cloth.ToList())
    {
      if (id == -1) continue;
      await this.SetClothPiece(id);
    }
  }
  #endregion


  public int maxArmor
  {
    get
    {
      return (int)this.MaxArmor;
    }
    set
    {
      this.MaxArmor = (ushort)value;
      this.Emit("maxarmor", value);
    }
  }

  public async Task<bool> CanControllVehicle(xVehicle vehicle)
  {
    if (vehicle.owner_id == this.id && vehicle.owner_type == (int)OWNER_TYPES.PLAYER) return true;

    return false;
  }
  #endregion
}