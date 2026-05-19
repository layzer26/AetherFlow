# AetherFlow Unity Setup

## Prerequisites
- Unity Hub — https://unity.com/download
- Unity **2022.3 LTS** installed via Unity Hub  
  *(choose "Android Build Support" module when installing)*

---

## Step 1 — Create the Unity project

1. Open Unity Hub → **New project**
2. Template: **2D (Core)**
3. Name it `AetherFlowUnity`
4. Click **Create project**

---

## Step 2 — Copy the game logic

In your file explorer, copy these folders from the AetherFlow repo into  
`AetherFlowUnity/Assets/Scripts/`:

| Copy from (repo)   | Paste into (Unity)                    |
|--------------------|---------------------------------------|
| `Core/*.cs`        | `Assets/Scripts/Core/`                |
| `Core/Enums/*.cs`  | `Assets/Scripts/Core/Enums/`          |
| `Engine/*.cs`      | `Assets/Scripts/Engine/`              |
| `Unity/Assets/Scripts/Game/*.cs` | `Assets/Scripts/Game/` |

After copying, Unity will compile the scripts automatically.  
Fix any error about `AetherFlow.Core.MatchState` namespace — it should be `using AetherFlow.Core;`.

---

## Step 3 — Open the sample scene

1. In Unity, open **File → New Scene** → choose **Basic (Built-in)**
2. Delete the default `Main Camera` from the Hierarchy (the bootstrapper creates its own)
3. Create an empty GameObject: right-click Hierarchy → **Create Empty**, name it `Bootstrap`
4. In the Inspector, click **Add Component** → search `SceneBootstrapper` → add it

---

## Step 4 — Press Play

Hit **▶ Play** in the Unity Editor.  
You should see:
- The Vienna map drawn as grey zone circles with connection lines
- Blue tokens (Attackers) on the left, red tokens (Defenders) on the right
- A card hand panel at the bottom
- Round/phase info at the top
- A log panel on the right

---

## How to play

| Action | What to do |
|--------|-----------|
| Select a card | Tap/click a card in the hand at the bottom |
| Target a zone | After playing a damage/smoke/stun card, tap a highlighted zone on the map |
| Move an agent | Tap a highlighted zone (cyan) after the card phase |
| Skip | Tap the **Skip** button |

The AI (Defenders) takes its turn automatically after yours.

**Win conditions:**
- Eliminate all 5 Defenders → Attackers win
- Plant spike on A Site or B Site, survive 4 rounds → Attackers win  
- Defuse spike (Defender must be on spike zone for 2 rounds) → Defenders win
- All Attackers eliminated → Defenders win

---

## Step 5 — Build for Android

1. **File → Build Settings** → select **Android** → click **Switch Platform**
2. **Player Settings** → set Package Name (e.g. `com.yourname.aetherflow`)
3. Set **Minimum API Level** to Android 8.0 (API 26)
4. Connect your Android device (USB debugging on) or use an emulator
5. Click **Build and Run**

> You'll need the Android SDK / NDK — Unity Hub installs these automatically  
> if you selected "Android Build Support" during Unity installation.

---

## Project layout (inside Unity Assets)

```
Assets/
└── Scripts/
    ├── Core/          ← AetherFlow data models & map
    │   └── Enums/
    ├── Engine/        ← CardResolver, MatchEngine, AgentFactory
    └── Game/          ← Unity-specific MonoBehaviours
        ├── GameController.cs    — game loop, player input, AI
        ├── MapView.cs           — zone circles, edges, agent tokens
        ├── HandView.cs          — card hand UI at bottom
        ├── HudView.cs           — top bar + log panel
        └── SceneBootstrapper.cs — creates everything, just add to a GameObject
```
