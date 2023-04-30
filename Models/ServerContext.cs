using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace server.Models;

public partial class ServerContext : DbContext
{
  public ServerContext() { }
  public ServerContext(DbContextOptions<ServerContext> options) : base(options) { }

  public virtual DbSet<Player> Players { get; set; }
  public virtual DbSet<Vehicle> Vehicles { get; set; }

  public virtual DbSet<Storage> Storages { get; set; }
  public virtual DbSet<Item> Items { get; set; }

  public virtual DbSet<Faction> Factions { get; set; }
  public virtual DbSet<Faction_rank> Faction_ranks { get; set; }
  public virtual DbSet<Faction_ug> Faction_ugs { get; set; }

  public virtual DbSet<Business> Businesses { get; set; }

  public virtual DbSet<Farming_Collector> Farming_Collectors { get; set; }
  public virtual DbSet<Farming_Processor> Farming_Processors { get; set; }

  public virtual DbSet<Garage> Garages { get; set; }
  public virtual DbSet<GarageSpawn> GarageSpawns { get; set; }

  public virtual DbSet<Shop> Shops { get; set; }
  public virtual DbSet<ShopItems> ShopItems { get; set; }
  public virtual DbSet<Vehicle_Shop> Vehicle_Shops { get; set; }
  public virtual DbSet<Cloth_Shop> Cloth_Shops { get; set; }

  public virtual DbSet<Factory_Processes> Factory_Processes { get; set; }

  public virtual DbSet<Bank> Banks { get; set; }

  public virtual DbSet<Cloth> Clothes { get; set; }
  public virtual DbSet<Vehicle_Key> Vehicle_Keys { get; set; }

  public virtual DbSet<Prop> Props { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    if (!optionsBuilder.IsConfigured)
    {
      optionsBuilder.UseMySql("server=45.157.233.24;database=server;user=root;password=KrjganovOnTop1!23;treattinyasboolean=true",
        new MySqlServerVersion(new Version(8, 0, 25)));
    }
  }

  #region Generate Entities
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {

    #region Player
    modelBuilder.Entity<Player>(entity =>
    {
      entity.HasOne(d => d.player_skin)
        .WithOne(d => d.Player)
        .HasForeignKey<Player_Skin>(d => d.player_id)
        .HasPrincipalKey<Player>(d => d.id);

      entity.HasOne(d => d.player_cloth)
        .WithOne(d => d.Player)
        .HasForeignKey<Player_Cloth>(d => d.player_id)
        .HasPrincipalKey<Player>(d => d.id);

      entity.HasMany(d => d.vehicle_keys)
        .WithOne(d => d.Player)
        .HasForeignKey(d => d.player_id)
        .HasPrincipalKey(d => d.id);

      entity.HasOne(d => d.player_society)
        .WithOne(d => d.Player)
        .HasForeignKey<Player_Society>(d => d.player_id)
        .HasPrincipalKey<Player>(d => d.id);

      entity.HasOne(d => d.player_factory)
        .WithOne(d => d.Player)
        .HasForeignKey<Player_Factory>(d => d.player_id)
        .HasPrincipalKey<Player>(d => d.id);
    });

    modelBuilder.Entity<Player_Society>(entity =>
    {
      entity.HasOne(d => d.Player)
        .WithOne(p => p.player_society)
        .HasForeignKey<Player_Society>(d => d.player_id)
        .HasPrincipalKey<Player>(p => p.id);

      entity.HasOne(d => d.Faction)
        .WithMany(p => p.Members)
        .HasForeignKey(d => d.faction_id)
        .HasPrincipalKey(p => p.id);
    });
    #endregion

    #region Vehicle
    modelBuilder.Entity<Vehicle>(entity =>
    {
      entity.HasOne(d => d.vehicle_data)
        .WithOne(d => d.Vehicle)
        .HasForeignKey<Vehicle_Data>(d => d.vehicle_id)
        .HasPrincipalKey<Vehicle>(d => d.id);

      entity.HasMany(d => d.vehicle_keys)
        .WithOne(d => d.Vehicle)
        .HasForeignKey(d => d.vehicle_id)
        .HasPrincipalKey(d => d.id);
    });

    modelBuilder.Entity<Vehicle_Key>(entity =>
    {
      entity.HasOne(d => d.Player)
        .WithMany(p => p.vehicle_keys)
        .HasForeignKey(d => d.player_id)
        .HasPrincipalKey(p => p.id);

      entity.HasOne(d => d.Vehicle)
        .WithMany(p => p.vehicle_keys)
        .HasForeignKey(d => d.vehicle_id)
        .HasPrincipalKey(p => p.id);
    });

    #endregion

    #region Faction and Business

    modelBuilder.Entity<Faction>(entity =>
    {
      entity.HasMany(d => d.Members)
        .WithOne(d => d.Faction)
        .HasForeignKey(d => d.faction_id)
        .HasPrincipalKey(d => d.id);

      entity.HasMany(d => d.Ranks)
        .WithOne(d => d.Faction)
        .HasForeignKey(d => d.faction_id)
        .HasPrincipalKey(d => d.id);
    });

    modelBuilder.Entity<Faction_rank>(entity =>
    {
      entity.HasOne(d => d.Faction)
        .WithMany(p => p.Ranks)
        .HasForeignKey(d => d.faction_id)
        .HasPrincipalKey(p => p.id);
    });

    modelBuilder.Entity<Business>(entity =>
    {
      entity.HasMany(d => d.Members)
        .WithOne(d => d.Business)
        .HasForeignKey(d => d.business_id)
        .HasPrincipalKey(d => d.id);
    });


    #endregion

    #region Garage
    modelBuilder.Entity<Garage>(entity =>
    {
      entity.HasMany(d => d.GarageSpawns)
        .WithOne(d => d.Garage)
        .HasForeignKey(d => d.garage_id)
        .HasPrincipalKey(d => d.id);
    });

    modelBuilder.Entity<GarageSpawn>(entity =>
    {
      entity.HasOne(d => d.Garage)
        .WithMany(p => p.GarageSpawns)
        .HasForeignKey(d => d.garage_id)
        .HasPrincipalKey(p => p.id);
    });
    #endregion


    modelBuilder.Entity<Vehicle_Shop>(entity =>
    {
      entity.HasMany(d => d.Vehicles)
        .WithOne(d => d.Vehicle_Shop)
        .HasForeignKey(d => d.vehicle_shop_id)
        .HasPrincipalKey(d => d.id);
    });

    modelBuilder.Entity<Cloth_Shop>(entity =>
    {
      entity.HasMany(d => d.cloth_items)
        .WithOne(p => p.shop)
        .HasForeignKey(d => d.shop_id)
        .HasPrincipalKey(p => p.id);
    });

    modelBuilder.Entity<Farming_Collector>(entity =>
    {
      entity.HasMany(d => d.Props)
        .WithOne(p => p.collector)
        .HasForeignKey(d => d.route_id)
        .HasPrincipalKey(p => p.id);
    });


    OnModelCreatingPartial(modelBuilder);
  }
  #endregion
  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}