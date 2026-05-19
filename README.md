# 🌌 AetherFlow: Turn-Based Tactical Card Game Backend

##  Project Overview

**AetherFlow** is a **turn-based card game backend** inspired by the team-based tactics of **Valorant** and the strategic deck mechanics of **Clash of Clans**. Designed using **Clean Architecture**, the backend prioritizes modularity, testability, and separation of concerns.

The game is built around players deploying Agents with custom mini-decks to control zones, deploy abilities, plant or defuse spikes, and eliminate opposing forces. Gameplay emphasizes **strategy over RNG**.

The overall essence of the game is planned to be relaxing and strategic. I'm hoping to complete the design with a balance that allows players to enjoy it casually, while also rewarding deep strategic thinking. Ideally, the agent pool will be large enough to minimize repetitive deck selections — although that may evolve as development continues.

## 🎯 Spatial & Strategic Ability Design

In AetherFlow, every ability is a **card** in an agent’s mini-deck—but it’s not just numbers. Each card defines:

1. **Where** it can be played (range & shape)  
2. **What it does** (effect: damage, heal, control, buff, etc.)  
3. **How it changes the map** (temporary zone control, area denial, vision)

By combining these, we create a rich **top-down** experience where “taking space” and “controlling zones” are as important as raw damage numbers.

---

### 1. Range & Targeting

- **Zone Graph Distance**: Each zone has neighbors (adjacency).  
  - A card’s **Range** is defined by a maximum graph distance (e.g., 1 = adjacent zones, 2 = two hops away possibly).  
- **Target Shapes**  
  - **Single-target**: pick one zone within Range → affects some agents there, and partial area of that zone.  
  - **Area-of-Effect**: define a “pattern” (e.g., straight line, circle, or square with corners only).  
  - **Global** (ultimate only): affects the entire zone or all enemy zones.  

> *Example:* Agent 1 “Bendy” (smoke) has Range = 2 and Shape = single line in zone. When played, that zone becomes impassable for 1 or 2 turn/turns.

---

### 2. Zone Control & Space Denial

- **ControlStatus Modifiers**  
  - Some cards temporarily convert a zone’s `ControlStatus` to the caster’s side, granting buffs or blocking enemy movement.  
- **Blockades**  
  - Abilities like Agent 2 “Greenhous” overlay on a zone’s edges—agents cannot cross through that zone or lose vision/information while inside.  
- **Persistent Effects**  
  - Controller ultimates can create a lasting area (e.g., crater) that deals damage over time and prevents capture.

> *Tactical Tip:* Well-timed smokes or blockades force opponents to take longer routes, buying precious turns for planting or defusing.

---

### 3. Movement as a Resource

- **Action Points**  
  - Each turn, agents have a fixed “move budget” (e.g., 1 block in a zone hop).  
- **Ability-Movement Tradeoff**  
  - Playing a card costs “mana” or “action points,” so you might forgo movement to use a powerful skill.  
- **Strategic Positioning**  
  - Decks include both **mobility cards** (dashes, teleports) and **area cards** (barriers, smokes). Balancing those lets you outmaneuver enemies.

> *Example:* Duelists often run in with dashes, trade space for surprise attacks, then retreat behind a Sentinel’s wall card.

---

### 4. Line-of-Sight & Vision

- **Vision Zones**  
  - By default, agents see their own zone plus neighbors.  
- **Vision-Blocking**  
  - Smoke or Darkness effects prevent vision through that zone (but allies inside still see).  
- **Recon Abilities**  
  - Initiators like Sova fire a “Recon Bolt” that reveals enemy positions in a target zone for 1 turn.

> *Strategic Note:* Pinging recon before committing to a zone helps prevent ambushes—information becomes its own “zone control” mechanic.

---

### 5. Ultimate Charge & Timing

- **Charge Mechanic**  
  - Each played card grants “ultimate charge” (e.g., +1 for regular, +2 for successful kills).  
  - Once charge ≥ 100%, the **ultimate card** becomes playable.  
- **High-Impact Ultimates**  
  - These global or multi-zone effects can swing the tide—must be timed when enemies are clustered or key zones are being contested.

> *Pro Tip:* Coordinate ultimates with your team’s zone-push or defuse attempts to maximize their impact.

---

### Putting it All Together

By treating **zones** as both **movement nodes** and **ability targets**, AetherFlow becomes a living tactical board:
- Every card is a lever over **space** and **information**  
- “Taking space” means planting smokes, dashing past chokepoints, or forcing enemies onto slow detours  
- Each turn, you ask: *Where do I need presence? Where do I need to deny? And how much resource can I spend moving vs. playing cards?*

This **zone-driven** design ensures that every ability isn’t just a number, but a strategic tool for **shaping the battlefield**.


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

