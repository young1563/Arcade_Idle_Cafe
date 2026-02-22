# 🎮 Arcade Idle Cafe - Factory System Project Summary
**Last Updated**: 2026-02-23
**Primary Goal**: Building a data-driven factory automation system from scratch.

---

## 🛠 1. Technical Stack
- **Engine**: Unity (Windows)
- **Input System**: New Input System (PC/Mobile compatible)
- **Animation/VFX**: DG.Tweening (DOTween), Cinemachine
- **Architecture**: ScriptableObject-based data-driven design, Singleton pattern (Managers)
- **Namespace**: None (Flat structure used for simplicity)

---

## 📂 2. Core Architecture & File Structure

### Data (`Assets/3.Script/Data/`)
- `FacilityData.cs`: ScriptableObject for machines (Name, Type, Prefab, Price, Recipe).
- `FacilityItemData.cs`: ScriptableObject for items (Name, Prefab, Sell Price).

### Systems (`Assets/3.Script/Build/`)
- `BuildManager.cs`: Handles building/removing.
    - Features: Grid snapping (1.0), R-key rotation, Preview ghost effects (Green/Red), Build/Remove mode UI guides.
- `BuildButton.cs`: UI bridge to trigger build mode.

### Factory Logic (`Assets/3.Script/Factory/`)
- `BaseFacility.cs`: Unified logic for ALL machines.
    - `Generator`: Spawns items.
    - `Processor`: Takes N inputs -> Spawns 1 output.
    - `Merger`: Takes multiple different inputs.
    - **Current Feature**: Includes a `waitingQueue` to handle items while processing.
- `ConveyorBelt.cs`: Moves items via `FactoryItem.SetTarget`.
- `SellPortal.cs`: Advanced portal-sucking effect with DOTween.
- `FactoryItem.cs`: Attached to items (Donuts, Waffles). Handles smooth movement towards targets.

---

## 🚀 3. Current Implementation Status

### ✅ Completed
- [x] Basic Build System with UX polish (Colors, Guides).
- [x] Player Movement & Cinemachine Follow.
- [x] Conveyor Belt flow logic.
- [x] Sell Portal with "Portal Suck" effect and Money integration.
- [x] Processor logic (e.g., Donut -> Waffle).

### 🛠 In Progress / Pending Issues
1. **Merger Refinement**:
    - **Issue**: Result item not spawning in some cases.
    - **Debugging Check**: Verify `OutputPoint` is assigned in the Inspector and `OutputItem` is set in the SO.
    - **Layout**: Recommended using T-Junction belts or wide trigger colliders for merging lines.
2. **Item Flow Optimization**:
    - Ensure `Generator` does not stop produced items in its own trigger.
    - Fixed `BaseFacility.cs` to skip `OnTriggerEnter` logic for `Generator` types.

---

## 📝 4. Instructions for Next Session (Hand-over)
When starting a new session on a different machine, provide this document and say:
> "Read this project summary. We are building a factory system. Currently, we just updated `BaseFacility.cs` to handle mergers and fixed an issue where produced items were stopping at the generator. Let's verify the **Merger's output issue** and then move to **Unlock Zones**."

---

## 📌 5. Important Settings to Remember
- **Layers**: All facilities MUST be on the **Building** layer for collision/removal to work.
- **Colliders**: Conveyor colliders should be slightly longer (Z: 1.1) to avoid gaps.
- **Pivots**: Ensure machine prefabs have pivots at the bottom center (Y=0).
