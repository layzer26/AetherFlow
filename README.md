# 🌌 ValoCard: Turn-Based Tactical Card Game Backend

##  Project Overview

**ValoCard** is a **turn-based card game backend** inspired by the team-based tactics of **Valorant** and the strategic deck mechanics of **Clash of Clans**. Designed using **Clean Architecture**, the backend prioritizes modularity, testability, and separation of concerns.

The game is built around players deploying Agents with custom mini-decks to control zones, deploy abilities, plant or defuse spikes, and eliminate opposing forces. Gameplay emphasizes **strategy over RNG**.

The overall essence of the game is planned to be relaxing and strategic. I'm hoping to be able to complete the plan with a balance of being able to play slightly distracted while rewarding players that are actively being strategic. 
Hoepfully the agent base will be able to be vast enough to minimise the selection of the same agent decks. But As this is one of my first ideas, i suppose that may change as time goes on.

**Incomplete Idea**
- Still trying to figure out if there will be signals to identify occupation of zones in order to strategise a counter plan
- Current Thought:  sending signals for information 
   - abilities can supress certain signals 
   - allow players to restrategise 
- Past Thought
   - distribute the map in a way that allows cards to autonomously take space , although this conflicts the idea of turn base, i'm still weighing up the idea to use the clash of clans style of game play or not.

   

---

##  Game Summary

* **2 Players:** Attackers vs. Defenders
* **Each controls \~5 Agents** with unique ability decks
* **Win Conditions:** Eliminate all opponents, plant & detonate spike, defuse spike

---

##  Core Gameplay Entities

###  Agent

* `Name`
* `AgentRole`: Duelist, Controller, Initiator, Sentinel
* `Health`
* `CurrentZone`
* `MiniDeck`: 3 ability cards + 1 ultimate card

###  Card

* `Name`
* `EffectType`: Damage, Heal, Stun, Smoke, Buff, etc.
* `Power`: Numeric value
* `Cost`: Mana or action points
* `TargetType`: Single, Area, Self
* `RngModifier`: Indicates if RNG involved
* `IsUltimate`: Boolean

###  Zone

* `ZoneName`
* `Neighbors`: Adjacent Zones (for graph navigation)
* `ControlStatus`: None, Attacker, Defender

### Match state

* Turn-based
* Draw > Play Card(s) > Move/Plant/Defuse > End Turn
* Deterministic damage/resolution; some abilities use RNG (e.g., smokes)

### Action Result
* result of an ability card action 
* still trying to conceptualise fully what i want this class to take care of 


---

##  Project Structure

```
ValoCard/
├── Core/                # Models (Agent, Card, Zone, Enums, MatchState,ActionResult)
├── Engine/              # Game logic (MatchEngine, TurnManager)
├── Map/                 # ZoneGraph + Control logic
├── Tests/               # xUnit or NUnit test project
├── CLI/                 # Console-based testing and interface
└── ValoCard.sln
```

---

##  Next Steps

~~1. Build `ZoneGraph` class to manage zone connections~~
2. Fully implement the Core folder 
3. Build `MatchEngine` to handle turns, spike   logic, and win conditions
4. Create `CardResolver` to apply card effects
5. Add test coverage for:

   * Agent actions
   * Card resolution
   * Spike win/defuse mechanics
6. Re-Evaulate full gameplay plan etc 


