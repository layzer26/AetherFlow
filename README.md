# 🌌 AetherFlow: Turn-Based Tactical Card Game Backend

##  Project Overview

**AetherFlow** is a **turn-based card game backend** inspired by the team-based tactics of **Valorant** and the strategic deck mechanics of **Clash of Clans**. Designed using **Clean Architecture**, the backend prioritizes modularity, testability, and separation of concerns.

The game is built around players deploying Agents with custom mini-decks to control zones, deploy abilities, plant or defuse spikes, and eliminate opposing forces. Gameplay emphasizes **strategy over RNG**.

The overall essence of the game is planned to be relaxing and strategic. I'm hoping to complete the design with a balance that allows players to enjoy it casually, while also rewarding deep strategic thinking. Ideally, the agent pool will be large enough to minimize repetitive deck selections — although that may evolve as development continues.

**Incomplete Idea**
- Still trying to decide whether zone occupation should generate "signals" that opponents can use to strategize
- Current thought:
   - Agents or zones emit signals for information
   - Some abilities can suppress signals
   - Signals influence player strategy in real-time
- Past idea:
   - Cards autonomously taking space (Clash of Clans style)
   - But that may conflict with the turn-based concept — still undecided

---

##  Game Summary

* **2 Players:** Attackers vs. Defenders  
* **Each controls ~5 Agents** with unique ability decks  
* **Win Conditions:**  
   - Eliminate all enemy agents  
   - Plant and detonate spike  
   - Defuse spike

---

##  Core Gameplay Entities

### Agent

* `Name`
* `AgentRole`: Duelist, Controller, Initiator, Sentinel
* `Health`
* `CurrentZone`
* `MiniDeck`: 3 ability cards + 1 ultimate card

### Card

* `Name`
* `EffectType`: Damage, Heal, Stun, Smoke, Buff, etc.
* `Power`: Numeric value
* `Cost`: Mana or action points
* `TargetType`: Single, Area, Self
* `RngModifier`: Indicates if RNG involved
* `IsUltimate`: Boolean

### Zone

* `ZoneName`
* `Neighbours`: Adjacent Zones (for graph navigation)
* `ControlStatus`: None, Attacker, Defender

### Match State

* Turn-based flow:
   1. Draw  
   2. Play Card(s)  
   3. Move / Plant / Defuse  
   4. End Turn
* Deterministic damage resolution; some abilities may include RNG (e.g. smokes)

### ActionResult

* Represents the outcome of an ability or card action
* Still conceptualizing what data and effects this class should encapsulate

---
##  Project Structure

```
AetherFlow/
├── Core/                # Models (Agent, Card, Zone, Enums, MatchState,ActionResult)
├── Engine/              # Game logic (MatchEngine, TurnManager)
├── Map/                 # ZoneGraph + Control logic
├── Tests/               # xUnit or NUnit test project
├── CLI/                 # Console-based testing and interface
└── AetherFlow.sln
```


---

##  Next Steps

~~1. Build `ZoneGraph` class to manage zone connections~~  
2. Fully implement the Core folder  
3. Build `MatchEngine` to handle turns, spike logic, and win conditions  
4. Create `CardResolver` to apply card effects  
5. Add test coverage for:
   - Agent actions  
   - Card resolution  
   - Spike win/defuse mechanics  
6. Re-evaluate full gameplay design and mechanics  

---

## LICENSE

This project is **not open-source**. All rights reserved.  
Do not reuse this code or its ideas without explicit permission from the author.

