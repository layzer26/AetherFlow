using System;
using System.Collections.Generic;
using AetherFlow.Core.Enums;

namespace AetherFlow.Core
{
    public static class Maps {


    public static ZoneGraph BuildViennaMap(){
        var graph = new ZoneGraph();
        //Spawn Zones
        graph.AddZone("Attackers Spawn");
        graph.AddZone("Defenders Spawn");
        //Sites and Key Zones
        graph.AddZone("A Site");
        graph.AddZone("B Site");
        graph.AddZone("Mid");
        //Paths to Zones
        graph.AddZone("A Long");
        graph.AddZone("B Long");
        graph.AddZone("A Short");
        graph.AddZone("B Short");
        //graph.AddZone("Mid Connector");
        graph.AddZone("Mid To A");
        graph.AddZone("Mid To B");

        //Connect Zones
        //Attackers Spawn
        //Connecting the Attackers Spawn to various paths which lead to the sites
        graph.ConnectZones("Attackers Spawn", "A Long");
        graph.ConnectZones("Attackers Spawn", "A Short");
        graph.ConnectZones("Attackers Spawn", "B Long");
        graph.ConnectZones("Attackers Spawn", "B Short");
        graph.ConnectZones("Attackers Spawn", "Mid");

        //Defenders Spawn
        graph.ConnectZones("Defenders Spawn", "A Site");
        graph.ConnectZones("Defenders Spawn", "B Site");
        graph.ConnectZones("Defenders Spawn", "Mid");

        //A Site Connections
        graph.ConnectZones("A Site", "A Long");
        graph.ConnectZones("A Site", "A Short");
        graph.ConnectZones("A Site", "Mid To A");
        
        //B Site Connections
        graph.ConnectZones("B Site", "B Long");
        graph.ConnectZones("B Site", "B Short");
        graph.ConnectZones("B Site", "Mid To B");

        //Mid Connections
        graph.ConnectZones("Mid","Mid To A");
        graph.ConnectZones("Mid","Mid To B");

        return graph;




    }

    public static void PrintGraph(ZoneGraph graph) {
        foreach (var zone in graph.Zones) {
            Console.WriteLine($"Zone: {zone.ZoneName}");
            Console.WriteLine("Neighbours:");
            if (zone.Neighbours.Count == 0) {
                Console.WriteLine(" - No Neighbours");
            }
            foreach (var neighbour in zone.Neighbours) {
                Console.WriteLine($" - {neighbour.ZoneName}");
            }
            Console.WriteLine();
        }
    }

    
    public static ZoneGraph LoadMap(string mapName)
    {
        return mapName switch
        {
            "Vienna" => BuildViennaMap(),
            // Add cases for other maps later
            _ => throw new ArgumentException("Unknown map name")
        };
    }

}//end of Maps class
}// End of namespace