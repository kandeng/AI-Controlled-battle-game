# Asset Management

<cite>
**Referenced Files in This Document**
- [Player.prefab](file://Assets/FPS-Game/Prefabs/Player/Player.prefab)
- [Bot.prefab](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab)
- [Swat.prefab](file://Assets/FPS-Game/Models/Swat/Swat.prefab)
- [M91.fbx.meta](file://Assets/FPS-Game/Models/Sniper Rifle/M91.fbx.meta)
- [Rifle Idle.fbx.meta](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta)
- [Player_Land.wav.meta](file://Assets/FPS-Game/Audio/Player_Land.wav.meta)
- [Black.mat](file://Assets/FPS-Game/Materials/Black.mat)
- [Grid.mat](file://Assets/FPS-Game/Materials/Grid.mat)
- [BotController.cs](file://Assets/FPS-Game/Scripts/Bot/BotController.cs)
- [PlayerManager.cs](file://Assets/FPS-Game/Scripts/PlayerManager.cs)
- [WeaponManager.cs](file://Assets/FPS-Game/Scripts/WeaponManager.cs)
- [Grenade.cs](file://Assets/FPS-Game/Scripts/Grenade.cs)
- [InGameManager.prefab](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab)
- [InGameManager.cs](file://Assets/FPS-Game/Scripts/System/InGameManager.cs)
- [SceneSetupTool.cs](file://Assets/Editor/SceneSetupTool.cs)
- [SpawnInGameManager.cs](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs)
- [TimePhaseCounter.cs](file://Assets/FPS-Game/Scripts/System/TimePhaseCounter.cs)
</cite>

## Update Summary
**Changes Made**
- Updated InGameManager prefab documentation to reflect cleanup of dangling component references
- Corrected scene path references from 'Play.unity' to 'Play Scene.unity'
- Added documentation for automated scene setup tools and prefab configuration reliability improvements
- Updated system architecture diagrams to show simplified InGameManager component structure

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)
10. [Appendices](#appendices)

## Introduction
This document describes the asset management system for the FPS game project, focusing on the organization, naming, import pipeline, and runtime usage of 3D models, animations, audio assets, prefabs, and materials. It covers:
- Asset organization structure and naming conventions
- Import pipeline and quality settings
- Character models (player and bots), weapons, environmental props, and UI sprites
- Usage patterns for loading, instantiating, and managing assets at runtime
- Optimization strategies (LOD, memory, cross-platform compatibility)
- Style customization for materials and animations
- Versioning, updates, and QA processes
- Automated setup tools for scene configuration reliability

## Project Structure
The asset system is organized under the Assets/FPS-Game folder with dedicated subfolders for Models, Animations, Audio, Materials, Prefabs, Scripts, Sprites, and imported packages. The structure supports scalable asset management and clear separation of concerns.

```mermaid
graph TB
subgraph "Assets/FPS-Game"
Models["Models"]
Anim["Animations"]
Audio["Audio"]
Mats["Materials"]
Prefs["Prefabs"]
Scripts["Scripts"]
Sprites["Sprites"]
Imported["ImportedPackage"]
Scenes["Scenes"]
end
Models --> |"FBX/Materials"| Mats
Anim --> |"FBX/Animations"| Models
Prefs --> |"Runtime Instantiation"| Models
Scripts --> |"Asset References"| Prefs
Scripts --> |"Audio Playback"| Audio
Mats --> |"Applied to Models"| Models
Sprites --> |"UI/Sprites"| Scripts
Imported --> |"Third-party Assets"| Models
Scenes --> |"Scene Configuration"| Prefs
```

**Section sources**
- [Player.prefab:1-800](file://Assets/FPS-Game/Prefabs/Player/Player.prefab#L1-L800)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)
- [Swat.prefab:1-800](file://Assets/FPS-Game/Models/Swat/Swat.prefab#L1-L800)

## Core Components
- **Prefabs**: Reusable GameObject templates for players, bots, weapons, effects, and UI elements. They encapsulate components, meshes, materials, and references to scripts.
- **Models**: 3D assets (FBX) with associated materials and textures. Examples include character models and weapons.
- **Animations**: FBX animations applied to models via Animator controllers and Humanoid rigs.
- **Materials**: Shader-based surface definitions used across models.
- **Audio**: Sound assets configured for 3D spatialization and playback.
- **Scripts**: Runtime managers and controllers that reference and orchestrate assets.
- **System Prefabs**: Specialized prefabs like InGameManager that coordinate game state and scene setup.

**Updated** The InGameManager prefab has been cleaned up to remove dangling component references, improving prefab configuration reliability and ensuring proper scene loading through automated setup tools.

Key runtime integration points:
- BotController manages behavior-driven activation of prefabs and animation states.
- PlayerManager and WeaponManager expose references to player assets and weapon prefabs.
- Grenade script demonstrates runtime instantiation and lifecycle.
- InGameManager coordinates game phases, camera management, and system initialization.
- SceneSetupTool automates scene configuration and prefab placement.

**Section sources**
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [Grenade.cs:1-19](file://Assets/FPS-Game/Scripts/Grenade.cs#L1-L19)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)

## Architecture Overview
The asset management architecture couples prefabs with scripts to deliver cohesive gameplay experiences. Prefabs define the visual and component structure; scripts manage state transitions, input feeding, and runtime behaviors. The system now includes automated setup tools for reliable scene configuration.

```mermaid
graph TB
subgraph "Runtime"
BC["BotController.cs"]
PM["PlayerManager.cs"]
WM["WeaponManager.cs"]
GRENADE["Grenade.cs"]
IGM["InGameManager.cs"]
SPAWN_IGM["SpawnInGameManager.cs"]
SCENE_SETUP["SceneSetupTool.cs"]
TPC["TimePhaseCounter.cs"]
end
subgraph "Assets"
BOT_PREFAB["Bot.prefab"]
PLAYER_PREFAB["Player.prefab"]
SWAT_PREFAB["Swat.prefab"]
WEAPON_PREFABS["Weapon Prefabs"]
MATERIALS["Materials/*.mat"]
ANIM["Animations/*.fbx.meta"]
AUDIO["Audio/*.wav.meta"]
IN_GAME_MANAGER["InGameManager.prefab"]
SCENE["Play Scene.unity"]
end
BC --> BOT_PREFAB
BC --> ANIM
PM --> PLAYER_PREFAB
PM --> WEAPON_PREFABS
WM --> WEAPON_PREFABS
GRENADE --> WEAPON_PREFABS
IGM --> IN_GAME_MANAGER
SPAWN_IGM --> IN_GAME_MANAGER
SCENE_SETUP --> SCENE
TPC --> IGM
BOT_PREFAB --> MATERIALS
PLAYER_PREFAB --> MATERIALS
SWAT_PREFAB --> MATERIALS
ANIM --> MATERIALS
AUDIO --> |"Audio playback"| PM
```

**Diagram sources**
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [Grenade.cs:1-19](file://Assets/FPS-Game/Scripts/Grenade.cs#L1-L19)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)
- [SpawnInGameManager.cs:1-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L1-L69)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [TimePhaseCounter.cs:1-110](file://Assets/FPS-Game/Scripts/System/TimePhaseCounter.cs#L1-L110)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)
- [Player.prefab:1-800](file://Assets/FPS-Game/Prefabs/Player/Player.prefab#L1-L800)
- [Swat.prefab:1-800](file://Assets/FPS-Game/Models/Swat/Swat.prefab#L1-L800)
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)

## Detailed Component Analysis

### Prefab-Based Character Models
- Player prefab: Contains skinned meshes, bones, materials, and components for animation and rendering. It serves as the base for player visuals and can be extended with scripts for movement and input.
- Bot prefab: Similar structure to the player, enabling AI-driven animation and behavior.
- Swat model prefab: Demonstrates character modeling with separate head/body meshes and materials.

```mermaid
classDiagram
class PlayerPrefab {
+SkinnedMeshRenderer
+Animator
+Transform hierarchy
+Materials applied
}
class BotPrefab {
+SkinnedMeshRenderer
+Animator
+Transform hierarchy
+Materials applied
}
class SwatPrefab {
+SkinnedMeshRenderer
+Materials applied
}
PlayerPrefab <.. BotPrefab : "similar structure"
PlayerPrefab <.. SwatPrefab : "character model"
```

**Diagram sources**
- [Player.prefab:1-800](file://Assets/FPS-Game/Prefabs/Player/Player.prefab#L1-L800)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)
- [Swat.prefab:1-800](file://Assets/FPS-Game/Models/Swat/Swat.prefab#L1-L800)

**Section sources**
- [Player.prefab:1-800](file://Assets/FPS-Game/Prefabs/Player/Player.prefab#L1-L800)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)
- [Swat.prefab:1-800](file://Assets/FPS-Game/Models/Swat/Swat.prefab#L1-L800)

### InGameManager System - Enhanced Prefab Configuration
The InGameManager prefab has been cleaned up to remove dangling component references, improving prefab configuration reliability. The system now focuses on essential components for game coordination and scene management.

**Updated** Key improvements:
- Removed obsolete LobbyRelayChecker component to eliminate dangling references
- Simplified component structure for better maintainability
- Enhanced automated scene setup through SceneSetupTool integration
- Improved prefab configuration reliability for automated setup tools

```mermaid
classDiagram
class InGameManagerPrefab {
+NetworkBehaviour
+Game Mode Configuration
+Camera Management
+Phase Control
+Component Cleanup
}
class SceneSetupTool {
+Automated Setup
+Prefab Placement
+Scene Verification
}
class SpawnInGameManager {
+Early Spawning
+Network Ready Detection
+Prefab Validation
}
InGameManagerPrefab --> SceneSetupTool : "Automated Setup"
InGameManagerPrefab --> SpawnInGameManager : "Early Initialization"
```

**Diagram sources**
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [SpawnInGameManager.cs:1-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L1-L69)

**Section sources**
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [SpawnInGameManager.cs:1-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L1-L69)

### Scene Configuration and Automated Setup
The scene setup system has been updated to use the corrected 'Play Scene.unity' path, ensuring reliable prefab configuration and proper scene loading through automated setup tools.

**Updated** Changes:
- Scene path corrected from 'Play.unity' to 'Play Scene.unity' for improved reliability
- Automated setup tools now reference the correct scene path
- Enhanced error handling for missing scenes or prefabs
- Improved verification process for scene configuration

```mermaid
sequenceDiagram
participant Tool as "SceneSetupTool.cs"
participant Scene as "Play Scene.unity"
participant Prefab as "InGameManager.prefab"
participant Manager as "InGameManager.cs"
Tool->>Scene : Open Scene (Play Scene.unity)
Tool->>Prefab : Load Prefab
Tool->>Scene : Instantiate Prefab
Tool->>Manager : Configure Components
Manager-->>Tool : Setup Complete
```

**Diagram sources**
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)

**Section sources**
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)

### Animation Pipeline and Import Settings
- Animations are imported from FBX with humanoid rigging and blend shapes enabled. Import settings control compression, wrap mode, and retargeting warnings.
- Example: Rifle Idle.fbx meta shows humanoid description, blend shape import, and animation clips.

```mermaid
flowchart TD
Start(["Import FBX"]) --> Rigging["Configure Humanoid Rig"]
Rigging --> BlendShapes["Enable Blend Shapes"]
BlendShapes --> Compression["Set Animation Compression"]
Compression --> WrapMode["Set Wrap Mode"]
WrapMode --> Retargeting["Adjust Retargeting Settings"]
Retargeting --> ImportOK["Import Complete"]
```

**Diagram sources**
- [Rifle Idle.fbx.meta:1-885](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta#L1-L885)

**Section sources**
- [Rifle Idle.fbx.meta:1-885](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta#L1-L885)

### Materials and Surface Customization
- Materials define shader properties, textures, and colors. Two examples:
  - Black.mat: Simple shader with color tweak.
  - Grid.mat: Uses a texture for tiling patterns.

```mermaid
classDiagram
class Material {
+Shader
+Textures
+Colors
+Floats
}
class BlackMat {
+Color : Dark Gray
+Glossiness : 0.5
}
class GridMat {
+MainTex : Grid Texture
+Color : White
}
Material <|-- BlackMat
Material <|-- GridMat
```

**Diagram sources**
- [Black.mat:1-84](file://Assets/FPS-Game/Materials/Black.mat#L1-L84)
- [Grid.mat:1-84](file://Assets/FPS-Game/Materials/Grid.mat#L1-L84)

**Section sources**
- [Black.mat:1-84](file://Assets/FPS-Game/Materials/Black.mat#L1-L84)
- [Grid.mat:1-84](file://Assets/FPS-Game/Materials/Grid.mat#L1-L84)

### Audio Assets and Spatial Playback
- Audio assets are configured for 3D spatialization, normalization, and preloading. Example: Player_Land.wav meta shows settings for 3D sound and quality.

```mermaid
sequenceDiagram
participant Script as "PlayerManager.cs"
participant Audio as "AudioClip (Player_Land.wav)"
Script->>Audio : Load AudioClip
Script->>Audio : Play 3D Sound
Audio-->>Script : Playback Complete
```

**Diagram sources**
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)
- [Player_Land.wav.meta:1-23](file://Assets/FPS-Game/Audio/Player_Land.wav.meta#L1-L23)

**Section sources**
- [Player_Land.wav.meta:1-23](file://Assets/FPS-Game/Audio/Player_Land.wav.meta#L1-L23)
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)

### Runtime Asset Loading and Instantiation Patterns
- Prefabs are referenced by managers and instantiated at runtime for weapons, grenades, and effects.
- Managers expose getters to provide references to prefabs and effects.
- InGameManager coordinates early spawning and system initialization for reliable runtime behavior.

```mermaid
sequenceDiagram
participant WM as "WeaponManager.cs"
participant Prefab as "Grenade Prefab"
WM->>Prefab : Instantiate()
Prefab-->>WM : Grenade Instance
WM-->>WM : Manage Lifecycle
```

**Diagram sources**
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [Grenade.cs:1-19](file://Assets/FPS-Game/Scripts/Grenade.cs#L1-L19)

**Section sources**
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [Grenade.cs:1-19](file://Assets/FPS-Game/Scripts/Grenade.cs#L1-L19)

### AI Behavior and Asset Coordination
- BotController coordinates behavior selection, perception events, and input feeding to prefabs and animations. It binds to Behavior Designer shared variables and switches states based on conditions.

```mermaid
sequenceDiagram
participant BC as "BotController.cs"
participant BD as "Behavior Designer"
participant Prefab as "Bot.prefab"
BC->>BD : Start Behavior
BD-->>BC : Bind Shared Variables
BC->>Prefab : Apply Inputs (Move/Look/Attack)
Prefab-->>BC : Animation Updates
```

**Diagram sources**
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)

**Section sources**
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)

## Dependency Analysis
Asset dependencies are primarily resolved through prefabs and script references. Managers own references to prefabs and assets, while prefabs depend on materials and animations. The InGameManager system now has simplified dependencies for improved reliability.

**Updated** Dependency improvements:
- Removed obsolete LobbyRelayChecker dependencies
- Simplified InGameManager component structure
- Enhanced automated setup tool integration
- Improved scene path resolution from 'Play.unity' to 'Play Scene.unity'

```mermaid
graph TB
PM["PlayerManager.cs"] --> PP["Player.prefab"]
WM["WeaponManager.cs"] --> WP["Weapon Prefabs"]
BC["BotController.cs"] --> BP["Bot.prefab"]
IGM["InGameManager.cs"] --> IN_GAME_MANAGER["InGameManager.prefab"]
SPAWN_IGM["SpawnInGameManager.cs"] --> IN_GAME_MANAGER
SCENE_SETUP["SceneSetupTool.cs"] --> SCENE["Play Scene.unity"]
TPC["TimePhaseCounter.cs"] --> IGM
BP --> MAT["Materials/*.mat"]
PP --> MAT
SW["Swat.prefab"] --> MAT
ANIM["Animations/*.fbx.meta"] --> PP
ANIM --> BP
AUD["Audio/*.wav.meta"] --> PM
```

**Diagram sources**
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)
- [SpawnInGameManager.cs:1-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L1-L69)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [TimePhaseCounter.cs:1-110](file://Assets/FPS-Game/Scripts/System/TimePhaseCounter.cs#L1-L110)
- [Player.prefab:1-800](file://Assets/FPS-Game/Prefabs/Player/Player.prefab#L1-L800)
- [Bot.prefab:1-800](file://Assets/FPS-Game/Prefabs/Bot/Bot.prefab#L1-L800)
- [Swat.prefab:1-800](file://Assets/FPS-Game/Models/Swat/Swat.prefab#L1-L800)
- [Rifle Idle.fbx.meta:1-885](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta#L1-L885)
- [Player_Land.wav.meta:1-23](file://Assets/FPS-Game/Audio/Player_Land.wav.meta#L1-L23)
- [Black.mat:1-84](file://Assets/FPS-Game/Materials/Black.mat#L1-L84)
- [Grid.mat:1-84](file://Assets/FPS-Game/Materials/Grid.mat#L1-L84)

**Section sources**
- [PlayerManager.cs:1-34](file://Assets/FPS-Game/Scripts/PlayerManager.cs#L1-L34)
- [WeaponManager.cs:1-74](file://Assets/FPS-Game/Scripts/WeaponManager.cs#L1-L74)
- [BotController.cs:1-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L1-L485)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)

## Performance Considerations
- LOD and draw call reduction:
  - Use SkinnedMeshRenderer efficiently; avoid unnecessary bones and blend shapes.
  - Batch materials and reduce per-instance material variations.
- Animation optimization:
  - Prefer humanoid animations with compression and wrap modes suited to gameplay loops.
  - Minimize additive animations and redundant masks.
- Audio optimization:
  - Preload frequently used sounds; enable 3D spatialization only when needed.
- Cross-platform:
  - Adjust graphics quality settings per platform; use platform-specific build configurations.
  - Validate asset sizes and streaming requirements for mobile targets.
- Memory management:
  - Pool reusable assets (e.g., bullets, effects) to reduce GC pressure.
  - Unload unused assets after scenes change.
- **Updated** System optimization:
  - Simplified InGameManager component structure reduces memory overhead.
  - Automated setup tools eliminate manual configuration errors.
  - Improved scene path resolution prevents runtime loading failures.

## Troubleshooting Guide
Common issues and resolutions:
- Missing materials on skinned meshes:
  - Verify material assignments in prefabs and ensure textures are included in builds.
- Animation errors:
  - Confirm humanoid rigging and blend shape import settings; check for retargeting warnings.
- Audio not playing:
  - Ensure AudioClip is marked as 3D and loaded via resources or addressables.
- Prefab instantiation failures:
  - Confirm references are assigned in the inspector and prefabs are present in the build.
- **Updated** InGameManager configuration issues:
  - Verify InGameManager prefab is properly placed in 'Play Scene.unity' (not 'Play.unity').
  - Use SceneSetupTool to automate prefab placement and configuration.
  - Check for missing component references in InGameManager prefab.
  - Ensure automated setup tools can locate the correct scene path.

**Section sources**
- [Rifle Idle.fbx.meta:1-885](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta#L1-L885)
- [Player_Land.wav.meta:1-23](file://Assets/FPS-Game/Audio/Player_Land.wav.meta#L1-L23)
- [Black.mat:1-84](file://Assets/FPS-Game/Materials/Black.mat#L1-L84)
- [Grid.mat:1-84](file://Assets/FPS-Game/Materials/Grid.mat#L1-L84)
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)

## Conclusion
The asset management system leverages prefabs, materials, animations, and scripts to create a scalable and maintainable pipeline. Recent improvements include InGameManager prefab cleanup to remove dangling component references and scene path corrections from 'Play.unity' to 'Play Scene.unity', enhancing prefab configuration reliability and ensuring proper scene loading through automated setup tools. By adhering to naming conventions, import settings, and runtime patterns described here, teams can ensure consistent asset delivery, optimized performance, and smooth cross-platform deployment.

## Appendices

### Naming Conventions
- Models: Descriptive names with underscores (e.g., Sniper_Rifle, Swat).
- Animations: Clear action names (e.g., Rifle_Idle, Walking).
- Materials: Descriptive names (e.g., Black, Grid).
- Prefabs: Capitalized nouns (e.g., Player, Bot, AK-47, InGameManager).
- Audio: Descriptive names indicating effect (e.g., Player_Land).
- **Updated** Scene files: Use descriptive names with spaces (e.g., 'Play Scene.unity') for improved path resolution.

### Import Pipeline Checklist
- FBX import settings:
  - Humanoid rigging enabled.
  - Blend shapes imported as needed.
  - Animation compression and wrap mode appropriate.
- Materials:
  - Correct shader assignment.
  - Texture filtering and compression set for target platform.
- Audio:
  - 3D spatialization enabled.
  - Normalization and preload flags set appropriately.
- **Updated** Prefab configuration:
  - Verify InGameManager prefab has all required components.
  - Ensure automated setup tools can locate scene files correctly.
  - Test scene path resolution from 'Play Scene.unity'.

**Section sources**
- [M91.fbx.meta:1-110](file://Assets/FPS-Game/Models/Sniper Rifle/M91.fbx.meta#L1-L110)
- [Rifle Idle.fbx.meta:1-885](file://Assets/FPS-Game/Animations/Player/Rifle Idle.fbx.meta#L1-L885)
- [Player_Land.wav.meta:1-23](file://Assets/FPS-Game/Audio/Player_Land.wav.meta#L1-L23)
- [InGameManager.prefab:1-189](file://Assets/FPS-Game/Prefabs/System/InGameManager.prefab#L1-L189)

### Versioning and QA
- Versioning:
  - Use asset GUIDs and version control for incremental updates.
  - Track prefab component changes and dependency updates.
- QA:
  - Validate animations on multiple rigs.
  - Test materials under various lighting scenarios.
  - Verify audio spatialization and volume balance.
  - **Updated** Test automated setup tools with corrected scene paths.
  - Verify InGameManager prefab cleanup and component validation.
  - Test scene path resolution from 'Play Scene.unity'.

### Automated Setup Tools
- SceneSetupTool: Automatically configures 'Play Scene.unity' with InGameManager prefab.
- SpawnInGameManager: Handles early InGameManager spawning during network initialization.
- **Updated** Benefits: Eliminates manual configuration errors, ensures consistent prefab placement, and improves scene loading reliability.

**Section sources**
- [SceneSetupTool.cs:1-107](file://Assets/Editor/SceneSetupTool.cs#L1-L107)
- [SpawnInGameManager.cs:1-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L1-L69)
- [InGameManager.cs:1-309](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L1-L309)