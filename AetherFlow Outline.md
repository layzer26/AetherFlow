# 🌌 ValoCard: Turn-Based Tactical Card Game Backend

## 🔎 Project Overview

**ValoCard** is a **turn-based card game backend** inspired by the team-based tactics of **Valorant** and the strategic deck mechanics of **Clash of Clans**. Designed using **Clean Architecture**, the backend prioritizes modularity, testability, and separation of concerns.

The game is built around players deploying Agents with custom mini-decks to control zones, deploy abilities, plant or defuse spikes, and eliminate opposing forces. Gameplay emphasizes **strategy over RNG**.

---

## ✨ Game Summary

* **2 Players:** Attackers vs. Defenders
* **Each controls \~5 Agents** with unique ability decks
* **Win Conditions:** Eliminate all opponents, plant & detonate spike, defuse spike

---

## 🔹 Core Gameplay Entities

### 🔵 Agent

* `Name`
* `AgentRole`: Duelist, Controller, Initiator, Sentinel
* `Health`
* `CurrentZone`
* `MiniDeck`: 3 ability cards + 1 ultimate card

### 🏃 Card

* `Name`
* `EffectType`: Damage, Heal, Stun, Smoke, Buff, etc.
* `Power`: Numeric value
* `Cost`: Mana or action points
* `TargetType`: Single, Area, Self
* `RngModifier`: Indicates if RNG involved
* `IsUltimate`: Boolean

### 🌍 Zone

* `ZoneName`
* `Neighbors`: Adjacent Zones (for graph navigation)
* `ControlStatus`: None, Attacker, Defender

### 📆 Match Flow

* Turn-based
* Draw > Play Card(s) > Move/Plant/Defuse > End Turn
* Deterministic damage/resolution; some abilities use RNG (e.g., smokes)

---

## 🏠 Project Structure

```
ValoCard/
├── Core/                # Models (Agent, Card, Zone, Enums)
├── Engine/              # Game logic (MatchEngine, TurnManager)
├── Map/                 # ZoneGraph + Control logic
├── Tests/               # xUnit or NUnit test project
├── CLI/                 # Console-based testing and interface
└── ValoCard.sln
```

---

## ✅ Initial Implementation Plan

### 🌐 1. Enums

```csharp
public enum AgentRole { Duelist, Controller, Initiator, Sentinel }
public enum EffectType { Damage, Heal, Smoke, Stun, Buff }
public enum TargetType { Self, Single, Area, Global }
public enum ZoneType { Neutral, ControlledByAttackers, ControlledByDefenders }
```

### 🔎 2. Card.cs

```csharp
public class Card
{
    public string Name { get; set; }
    public EffectType EffectType { get; set; }
    public int Power { get; set; }
    public int Cost { get; set; }
    public TargetType TargetType { get; set; }
    public bool IsUltimate { get; set; }
    public bool HasRngModifier { get; set; }

    public Card(string name, EffectType effectType, int power, int cost, TargetType targetType, bool isUltimate, bool hasRng)
    {
        Name = name;
        EffectType = effectType;
        Power = power;
        Cost = cost;
        TargetType = targetType;
        IsUltimate = isUltimate;
        HasRngModifier = hasRng;
    }
}
```

### 🔊 3. Agent.cs

```csharp
public class Agent
{
    public string Name { get; set; }
    public AgentRole Role { get; set; }
    public int Health { get; set; }
    public Zone CurrentZone { get; set; }
    public List<Card> MiniDeck { get; set; }

    public Agent(string name, AgentRole role, int health, Zone startingZone, List<Card> deck)
    {
        Name = name;
        Role = role;
        Health = health;
        CurrentZone = startingZone;
        MiniDeck = deck;
    }
}
```

### 🌐 4. Zone.cs

```csharp
public class Zone
{
    public string ZoneName { get; set; }
    public List<Zone> Neighbors { get; set; }
    public ZoneType ControlStatus { get; set; }

    public Zone(string name)
    {
        ZoneName = name;
        Neighbors = new List<Zone>();
        ControlStatus = ZoneType.Neutral;
    }

    public void AddNeighbor(Zone neighbor)
    {
        if (!Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
            neighbor.Neighbors.Add(this);
        }
    }
}
```

---

## 🔧 Next Steps

1. Build `ZoneGraph` class to manage zone connections
2. Build `MatchEngine` to handle turns, spike logic, and win conditions
3. Create `CardResolver` to apply card effects
4. Add test coverage for:

   * Agent actions
   * Card resolution
   * Spike win/defuse mechanics

---

Would you like me to scaffold the next folder (`Map/ZoneGraph.cs`) or `MatchEngine.cs` logic next?
