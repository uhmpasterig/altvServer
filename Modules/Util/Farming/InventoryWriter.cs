using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using server.Core;
using server.Models;
using server.Handlers.Storage;
using server.Modules.Items;

namespace server.Util.Farming;
public class verarbeiterWriter : IWritable
{
  private readonly List<xVehicle> vehicles;
  public verarbeiterWriter(List<xVehicle> _vehicles)
  {
    this.vehicles = _vehicles;
  }

  public void OnWrite(IMValueWriter writer)
  {
    writer.BeginObject();
    writer.Name("vehicles");
    writer.BeginArray();
    foreach(xVehicle veh in vehicles.ToList()) 
    {
      writer.BeginObject();
      writer.Name("id");
      writer.Value(veh.vehicleId);
      writer.Name("model");
      writer.Value(veh.model);
    }
    writer.EndArray();
    writer.EndObject();
  }
}