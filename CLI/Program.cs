using System;
using System.Linq;
using AetherFlow.Core;
using AetherFlow.Engine;

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║           AETHERFLOW — MVP DEMO MATCH            ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();

// Build map.
var map = Maps.LoadMap("Vienna");

Console.WriteLine("MAP: Vienna");
Console.WriteLine("Zones:");
foreach (var zone in map.Zones)
    Console.WriteLine($"  {zone.ZoneName} → {string.Join(", ", zone.Neighbours.Select(n => n.ZoneName))}");

Console.WriteLine();
Console.WriteLine("Press ENTER to start the match...");
Console.ReadLine();

// Create teams.
var attackers = AgentFactory.CreateAttackers(map);
var defenders = AgentFactory.CreateDefenders(map);

var state = new MatchState(attackers, defenders, map);
var engine = new MatchEngine(state);

// Run match, printing each log line.
string winner = engine.RunMatch(line => Console.WriteLine(line));

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine($"║  WINNER: {winner.ToUpper(),-40}║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();

// Final scoreboard.
Console.WriteLine("--- Final Agent Status ---");
Console.WriteLine("Attackers:");
foreach (var a in attackers)
    Console.WriteLine($"  {a.Name,-12} {(a.IsAlive ? $"{a.Health}/{a.MaxHealth} HP" : "ELIMINATED")}");

Console.WriteLine("Defenders:");
foreach (var d in defenders)
    Console.WriteLine($"  {d.Name,-12} {(d.IsAlive ? $"{d.Health}/{d.MaxHealth} HP" : "ELIMINATED")}");

Console.WriteLine();
Console.WriteLine("Press ENTER to exit.");
Console.ReadLine();
