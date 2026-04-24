# Netcode for GameObjects Integration

<cite>
**Referenced Files in This Document**
- [NetcodeForGameObjects.asset](file://ProjectSettings/NetcodeForGameObjects.asset)
- [DefaultNetworkPrefabs.asset](file://Assets/DefaultNetworkPrefabs.asset)
- [PlayerRoot.cs](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs)
- [PlayerNetwork.cs](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs)
- [PlayerTakeDamage.cs](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs)
- [PlayerShoot.cs](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs)
- [PlayerAnimation.cs](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs)
- [PlayerMovement.cs](file://Assets/FPS-Game/Scripts/PlayerMovement.cs)
- [Grenade.prefab](file://Assets/FPS-Game/Prefabs/Weapon/Grenade.prefab)
- [PackageManagerSettings.asset](file://ProjectSettings/PackageManagerSettings.asset)
</cite>

## Update Summary
**Changes Made**
- Updated all code examples and references to reflect Netcode for GameObjects v2 compatibility patterns
- Migrated from ClientNetworkTransform to NetworkTransform for v2 namespace integration
- Enhanced Unity.Netcode.Components namespace usage for NetworkTransform components
- Updated ServerRpc RequireOwnership attribute handling for v2 deprecation
- Revised component-based networking architecture to align with v2 namespace changes
- Updated RPC security patterns and authority-based movement systems
- Enhanced documentation with v2-specific troubleshooting guidance

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
This document explains how Netcode for GameObjects integrates into the FPS game with v2 compatibility. It focuses on the NetworkObject inheritance pattern via NetworkBehaviour, the NetworkBehaviour lifecycle, component-based networking architecture, server-client relationships, ownership management, NetworkVariable usage for state synchronization, and the RPC system (ServerRpc and ClientRpc). Practical examples cover networked object instantiation, despawning, and state replication across clients. Serialization considerations, custom data types, and performance optimization techniques are included, along with troubleshooting guidance for desynchronization, lag compensation, and bandwidth management.

**Updated** Enhanced with Netcode v2 compatibility features including NetworkTransform migration from ClientNetworkTransform, Unity.Netcode.Components namespace usage, and ServerRpc RequireOwnership attribute deprecation handling.

## Project Structure
The networking configuration and prefabs are centralized under ProjectSettings and Assets. The Netcode configuration points to default network prefabs, which define which prefabs are registered for network spawning. The PlayerRoot component orchestrates initialization order across multiple PlayerBehaviour components and exposes NetworkVariable-backed state. PlayerNetwork and PlayerTakeDamage are primary NetworkBehaviour implementations that demonstrate ownership checks, RPCs, and state synchronization.

```mermaid
graph TB
subgraph "ProjectSettings"
NFG["NetcodeForGameObjects.asset"]
PM["PackageManagerSettings.asset"]
end
subgraph "Assets"
DNP["DefaultNetworkPrefabs.asset"]
subgraph "Prefabs/System"
NM["NetworkManager.prefab"]
end
subgraph "Scripts/Player"
PR["PlayerRoot.cs"]
PN["PlayerNetwork.cs"]
PTD["PlayerTakeDamage.cs"]
PSH["PlayerShoot.cs"]
PAN["PlayerAnimation.cs"]
end
subgraph "Scripts/System"
PMV["PlayerMovement.cs"]
end
subgraph "Prefabs/Weapon"
GR["Grenade.prefab"]
end
end
NFG --> DNP
DNP --> NM
PR --> PN
PR --> PTD
PR --> PSH
PR --> PAN
PMV --> GR
```

**Diagram sources**
- [NetcodeForGameObjects.asset:1-18](file://ProjectSettings/NetcodeForGameObjects.asset#L1-L18)
- [DefaultNetworkPrefabs.asset:1-72](file://Assets/DefaultNetworkPrefabs.asset#L1-L72)
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)
- [PlayerShoot.cs:20-162](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L20-L162)
- [PlayerAnimation.cs:5-50](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs#L5-L50)
- [PlayerMovement.cs:5-158](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L5-L158)
- [Grenade.prefab:215-259](file://Assets/FPS-Game/Prefabs/Weapon/Grenade.prefab#L215-L259)

**Section sources**
- [NetcodeForGameObjects.asset:1-18](file://ProjectSettings/NetcodeForGameObjects.asset#L1-L18)
- [DefaultNetworkPrefabs.asset:1-72](file://Assets/DefaultNetworkPrefabs.asset#L1-L72)

## Core Components
- PlayerRoot: Inherits NetworkBehaviour and acts as a root container for player subsystems. It initializes components in priority order and exposes NetworkVariable-backed flags and identifiers. It also manages events and references to subsystems like camera, input, movement, and UI.
- PlayerNetwork: Inherits PlayerBehaviour and extends NetworkBehaviour. It manages per-character stats (NetworkVariable-based kill/death counts), ownership-aware logic, camera assignment for local players, and RPCs for mapping player info and respawn coordination.
- PlayerTakeDamage: Inherits PlayerBehaviour and extends NetworkBehaviour. It encapsulates health state via NetworkVariable and coordinates hit detection and scoring through ServerRpc and ClientRpc patterns.
- PlayerShoot: Implements authoritative shooting mechanics with ServerRpc validation and ClientRpc effects.
- PlayerAnimation: Manages rig builder state synchronization across clients using ServerRpc/ClientRpc pairs.
- PlayerMovement: Demonstrates authority-based movement with NetworkVariable position tracking and NetworkTransform integration.

Key lifecycle and ownership patterns:
- Ownership is checked via IsOwner and OwnerClientId.
- Network spawn order is orchestrated by PlayerRoot's priority-based initialization across IInitNetwork-implementing components.
- NetworkVariable values propagate automatically to clients; listeners update UI and game state locally.
- Authority-based movement ensures server validation of all player actions.

**Updated** Enhanced with NetworkTransform migration from ClientNetworkTransform for v2 compatibility and improved subsystem management using Unity.Netcode.Components namespace.

**Section sources**
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)
- [PlayerShoot.cs:20-162](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L20-L162)
- [PlayerAnimation.cs:5-50](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs#L5-L50)
- [PlayerMovement.cs:5-158](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L5-L158)

## Architecture Overview
The system follows a component-based networking architecture with enhanced authority patterns:
- PlayerRoot aggregates subsystems and coordinates initialization order across NetworkBehaviour components.
- PlayerNetwork handles ownership-specific logic, camera binding, and RPCs for mapping player info and respawns.
- PlayerTakeDamage centralizes hit detection and scoring via ServerRpc, updating NetworkVariable-based HP and Kill/Death counts.
- PlayerShoot implements authoritative combat mechanics with server-side validation.
- PlayerAnimation synchronizes visual states across clients.
- PlayerMovement demonstrates authority-based physics simulation.

```mermaid
classDiagram
class NetworkBehaviour
class PlayerRoot {
+NetworkTransform NetworkTransform
+PlayerNetwork PlayerNetwork
+PlayerTakeDamage PlayerTakeDamage
+PlayerShoot PlayerShoot
+PlayerController PlayerController
+PlayerUI PlayerUI
+PlayerCamera PlayerCamera
+NetworkVariable~bool~ IsBot
+NetworkVariable~FixedString32Bytes~ BotID
+OnNetworkSpawn()
}
class PlayerNetwork {
+string playerName
+NetworkVariable~int~ KillCount
+NetworkVariable~int~ DeathCount
+InitializeOnNetworkSpawn()
+MappingValues_ServerRpc(...)
+OnPlayerDead()
+OnPlayerRespawn()
}
class PlayerTakeDamage {
+NetworkVariable~float~ HP
+InitializeOnNetworkSpawn()
+ChangeHPServerRpc(...)
+ResetPlayerHP_ServerRpc(...)
}
class PlayerShoot {
+GunType CurrentGun
+HandleServerShoot_ServerRPC(...)
+BulletHitSpawn_ClientRpc(...)
}
class PlayerAnimation {
+Animator Animator
+RigBuilder RigBuilder
+UpdateRigBuilder_ServerRPC(...)
+UpdateRigBuilder_ClientRPC(...)
}
class PlayerMovement {
+float moveSpeed
+Vector3 moveDirection
+MovementState state
+FixedUpdate()
}
PlayerRoot --> PlayerNetwork : "aggregates"
PlayerRoot --> PlayerTakeDamage : "aggregates"
PlayerRoot --> PlayerShoot : "aggregates"
PlayerRoot --> PlayerAnimation : "aggregates"
PlayerRoot --> PlayerMovement : "aggregates"
PlayerRoot --|> NetworkBehaviour
PlayerNetwork --|> PlayerBehaviour
PlayerTakeDamage --|> PlayerBehaviour
PlayerShoot --|> PlayerBehaviour
PlayerAnimation --|> PlayerBehaviour
PlayerMovement --|> NetworkBehaviour
```

**Updated** Added PlayerShoot and PlayerAnimation classes to represent enhanced networking patterns with v2 compatibility and NetworkTransform integration.

**Diagram sources**
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)
- [PlayerShoot.cs:20-162](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L20-L162)
- [PlayerAnimation.cs:5-50](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs#L5-L50)
- [PlayerMovement.cs:5-158](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L5-L158)

## Detailed Component Analysis

### PlayerRoot: Component Orchestration and Initialization
PlayerRoot inherits NetworkBehaviour and serves as a hub for subsystems. It:
- Assigns references to subsystems (input, camera, controller, UI, etc.) via TryGetComponent and tag-based lookup.
- Implements priority-based initialization across IInitNetwork, IInitStart, and IInitAwake interfaces.
- Exposes NetworkVariable-backed flags for bot identity and ID, guarded by server checks.

Lifecycle highlights:
- OnNetworkSpawn triggers priority-based initialization of NetworkBehaviour components.
- Update loop reads zone data for pathfinding contexts.

```mermaid
sequenceDiagram
participant Net as "NetworkManager"
participant PR as "PlayerRoot"
participant PN as "PlayerNetwork"
participant PTD as "PlayerTakeDamage"
Net->>PR : OnNetworkSpawn()
PR->>PR : InitOnNetworkSpawn(root)
PR->>PN : InitializeOnNetworkSpawn()
PR->>PTD : InitializeOnNetworkSpawn()
Note over PR,PN : Priority-driven initialization across components
```

**Updated** Enhanced with NetworkTransform integration and improved subsystem management using Unity.Netcode.Components namespace.

**Diagram sources**
- [PlayerRoot.cs:215-218](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L215-L218)
- [PlayerRoot.cs:333-340](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L333-L340)

**Section sources**
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)

### PlayerNetwork: Ownership, Camera, and RPCs
PlayerNetwork demonstrates:
- Ownership-aware logic using IsOwner and OwnerClientId.
- ServerRpc for mapping player info to in-game names.
- ClientRpc for targeted position and rotation updates during spawn and respawn.
- Camera binding for local players via Cinemachine virtual camera.

Key behaviors:
- InitializeOnNetworkSpawn enables scripts conditionally based on ownership and bot status.
- OnPlayerDead disables scripts and schedules respawn; OnPlayerRespawn rebinds camera.
- MappingValues_ServerRpc updates remote clients' player names using direct client lookup.

```mermaid
sequenceDiagram
participant S as "Server"
participant PN as "PlayerNetwork"
participant NM as "NetworkManager"
participant CL as "Client"
S->>PN : MappingValues_ServerRpc(playerID, targetClientId)
PN->>NM : Lookup ConnectedClients[targetClientId]
NM-->>PN : PlayerObject
PN->>CL : Update playerName on target client
Note over PN,CL : Ownership not required for this broadcast-like mapping
```

**Updated** Enhanced with improved bot synchronization and authority-based state management using v2 RPC patterns.

**Diagram sources**
- [PlayerNetwork.cs:184-195](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L184-L195)

**Section sources**
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)

### PlayerTakeDamage: Health State and Scoring
PlayerTakeDamage encapsulates:
- NetworkVariable-based HP synchronized across clients.
- ServerRpc for applying damage and updating scores.
- ServerRpc for resetting HP on respawn.

Highlights:
- OnNetworkSpawn subscribes to HP change callbacks and player respawn events.
- OnHPChanged updates UI and triggers death events when HP reaches zero.
- ChangeHPServerRpc validates targets, applies damage, increments kill/death counters, and logs current HP.

```mermaid
sequenceDiagram
participant Attacker as "Attacker Client"
participant S as "Server"
participant Victim as "Victim Client"
participant PTD as "PlayerTakeDamage"
Attacker->>S : ChangeHPServerRpc(damage, targetClientId, ownerClientId)
S->>Victim : Apply HP reduction
Victim->>PTD : HP.OnValueChanged(previous, current)
PTD->>Victim : Update UI and invoke OnPlayerDead if HP <= 0
S->>Attacker : Increment KillCount on owner
Note over S,Attacker : Kill/Death counters synchronized via NetworkVariable
```

**Updated** Enhanced with improved bot damage handling and authority validation using v2 RPC patterns.

**Diagram sources**
- [PlayerTakeDamage.cs:58-83](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L58-L83)

**Section sources**
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)

### PlayerShoot: Authoritative Combat Mechanics
PlayerShoot implements:
- ServerRpc-based shooting validation with spread angle and weapon type parameters.
- ClientRpc for visual effects like bullet hits.
- Authority-based damage calculation and hit detection.

Key behaviors:
- Shoot method validates weapon type and hit area before sending ServerRpc.
- HandleServerShoot_ServerRPC performs raycasting with layer filtering and hit area determination.
- BulletHitSpawn_ClientRpc creates visual effects on all clients.

```mermaid
sequenceDiagram
participant Local as "Local Client"
participant Server as "Server"
participant Remote as "Remote Client"
Local->>Server : HandleServerShoot_ServerRPC(point, direction, spread, gunType, owner, networkObj)
Server->>Server : Raycast validation and hit detection
Server->>Remote : BulletHitSpawn_ClientRpc(hitPoint)
Server->>Server : Damage calculation and HP update
Note over Local,Remote : Authority ensures consistent combat results
```

**New** Added comprehensive combat system demonstrating authority patterns with v2 compatibility.

**Diagram sources**
- [PlayerShoot.cs:80-146](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L80-L146)

**Section sources**
- [PlayerShoot.cs:20-162](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L20-L162)

### PlayerAnimation: Visual State Synchronization
PlayerAnimation manages:
- ServerRpc/ClientRpc pairs for rig builder state synchronization.
- Authority-based animation control for dead/alive states.
- Client-side animation event handling.

Key behaviors:
- UpdateRigBuilder_ServerRPC validates state and synchronizes across clients.
- ClientRpc updates rig builder state without authority conflicts.
- Animation events trigger appropriate movement callbacks.

**New** Added animation synchronization patterns for enhanced visual consistency using v2 RPC patterns.

**Section sources**
- [PlayerAnimation.cs:5-50](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs#L5-L50)

### PlayerMovement: Authority-Based Movement
PlayerMovement demonstrates:
- Authority-based movement validation with IsOwner checks.
- NetworkVariable-based position tracking for interpolation.
- NetworkTransform integration for smooth client-side prediction.

Key behaviors:
- FixedUpdate only executes on owner client for movement input.
- Movement state machine with speed control and ground detection.
- Authority validation prevents cheating through client-side movement.

**New** Added authority-based movement patterns for secure gameplay with v2 NetworkTransform integration.

**Section sources**
- [PlayerMovement.cs:5-158](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L5-L158)

### NetworkVariable Usage and State Replication
- PlayerNetwork: KillCount, DeathCount, playerName.
- PlayerTakeDamage: HP.
- PlayerRoot: IsBot, BotID.
- PlayerMovement: Movement state and authority flags.

State replication:
- NetworkVariable values are authoritative on the server and replicated to clients automatically.
- Client-side listeners update UI and gameplay state without manual serialization.
- NetworkTransform provides smooth interpolation for non-authority objects.

Best practices:
- Keep NetworkVariable updates minimal and deterministic.
- Use ServerRpc for authoritative state transitions.
- Avoid frequent writes from clients unless ownership is required.
- Implement authority patterns for critical gameplay elements.

**Updated** Enhanced with NetworkTransform integration and authority-based patterns using Unity.Netcode.Components namespace.

**Section sources**
- [PlayerNetwork.cs:14-16](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L14-L16)
- [PlayerTakeDamage.cs:7](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L7)
- [PlayerRoot.cs:186-187](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L186-L187)
- [PlayerMovement.cs:36](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L36)

### RPC Patterns: ServerRpc and ClientRpc
Patterns demonstrated:
- ServerRpc with RequireOwnership=false for cross-client mapping and global state updates.
- ServerRpc with RequireOwnership=true for authority-critical operations.
- ClientRpc for targeted updates with ClientRpcParams to restrict delivery.

Security considerations:
- Prefer RequireOwnership=true when only the owning client should invoke sensitive operations.
- Validate target identifiers and existence before applying state changes.
- Use ServerRpc for all authority-critical operations like combat and movement.
- Implement parameter validation and range checking.

Parameter passing:
- Use primitive and serializable types for RPC parameters.
- For complex data, pass IDs and fetch objects on the server side.
- Use ClientRpcParams for targeted broadcasting to specific clients.

**Updated** Enhanced with authority patterns and improved security measures using v2 RPC attribute handling.

**Section sources**
- [PlayerNetwork.cs:184-195](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L184-L195)
- [PlayerTakeDamage.cs:58-83](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L58-L83)
- [PlayerShoot.cs:80-146](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L80-L146)

### Networked Object Instantiation and Despawning
- DefaultNetworkPrefabs defines which prefabs are registered for network spawning.
- NetworkManager prefab is the runtime anchor for the network session.
- PlayerRoot orchestrates initialization after OnNetworkSpawn, enabling subsystems and camera binding for local players.

Practical flow:
- Instantiate player prefabs on the server.
- NetworkManager registers prefabs from DefaultNetworkPrefabs.
- OnNetworkSpawn, PlayerRoot initializes subsystems and applies ownership-specific logic.

**Updated** Enhanced with NetworkTransform integration for smooth instantiation using v2 namespace patterns.

**Section sources**
- [DefaultNetworkPrefabs.asset:1-72](file://Assets/DefaultNetworkPrefabs.asset#L1-L72)
- [PlayerRoot.cs:215-218](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L215-L218)

### NetworkTransform Migration: ClientNetworkTransform to NetworkTransform
**Updated** The project has successfully migrated from ClientNetworkTransform to NetworkTransform for Netcode v2 compatibility:

- **Component Reference**: PlayerRoot now uses `NetworkTransform NetworkTransform` instead of `ClientNetworkTransform ClientNetworkTransform`
- **Namespace Integration**: Components are properly integrated with Unity.Netcode.Components namespace
- **Prefab Configuration**: Networked objects like grenades use `Unity.Netcode.Runtime::Unity.Netcode.Components.NetworkTransform` script
- **Migration Benefits**: Improved performance, better interpolation, and full v2 feature support

**Section sources**
- [PlayerRoot.cs:163](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L163)
- [Grenade.prefab:244](file://Assets/FPS-Game/Prefabs/Weapon/Grenade.prefab#L244)

## Dependency Analysis
The following diagram shows how core networking components depend on each other and on Netcode primitives.

```mermaid
graph TB
PR["PlayerRoot.cs"]
PN["PlayerNetwork.cs"]
PTD["PlayerTakeDamage.cs"]
PSH["PlayerShoot.cs"]
PAN["PlayerAnimation.cs"]
PMV["PlayerMovement.cs"]
NM["NetworkManager.prefab"]
DNP["DefaultNetworkPrefabs.asset"]
GR["Grenade.prefab"]
PR --> PN
PR --> PTD
PR --> PSH
PR --> PAN
PR --> PMV
PN --> NM
PTD --> NM
PSH --> NM
PAN --> NM
PMV --> NM
DNP --> NM
GR --> NM
```

**Updated** Added PlayerShoot, PlayerAnimation, and PlayerMovement dependencies with v2 compatibility and NetworkTransform integration.

**Diagram sources**
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)
- [PlayerShoot.cs:20-162](file://Assets/FPS-Game/Scripts/Player/PlayerShoot.cs#L20-L162)
- [PlayerAnimation.cs:5-50](file://Assets/FPS-Game/Scripts/Player/PlayerAnimation.cs#L5-L50)
- [PlayerMovement.cs:5-158](file://Assets/FPS-Game/Scripts/PlayerMovement.cs#L5-L158)
- [Grenade.prefab:215-259](file://Assets/FPS-Game/Prefabs/Weapon/Grenade.prefab#L215-L259)

**Section sources**
- [PlayerRoot.cs:160-367](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L160-L367)
- [PlayerNetwork.cs:12-541](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L12-L541)
- [PlayerTakeDamage.cs:5-124](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L5-L124)

## Performance Considerations
- Minimize RPC frequency: batch updates and throttle high-frequency events.
- Use ClientRpcParams to target specific clients when broadcasting is unnecessary.
- Prefer NetworkVariable for smooth interpolation; disable interpolation temporarily during teleportation and re-enable after stabilization.
- Avoid heavy computations in RPC handlers; delegate to server-side validators.
- Serialize only essential data; avoid large payloads in RPC parameters.
- Use authority-based updates to reduce redundant state broadcasts.
- Implement NetworkTransform for smooth client-side prediction.
- Use NetworkVariable for position tracking instead of frequent RPCs.
- Optimize raycast operations in ServerRpc methods.

**Updated** Enhanced with NetworkTransform and authority-based movement performance tips using v2 patterns.

## Troubleshooting Guide
Common issues and remedies:
- Desynchronization:
  - Verify NetworkVariable values are updated on the server and consumed on clients.
  - Ensure OnNetworkSpawn initializes subsystems consistently across clients.
  - Check NetworkTransform interpolation settings for authority objects.
- Lag compensation:
  - Use NetworkTransform interpolation judiciously; disable during teleportation and re-enable after stabilization.
  - Consider snapshot-based reconciliation for latency-sensitive actions.
  - Implement authority-based movement validation to prevent cheating.
- Bandwidth management:
  - Limit RPC calls; coalesce updates where possible.
  - Use ClientRpcParams to send targeted updates instead of broadcasting.
  - Monitor NetworkVariable update frequency for expensive state changes.
- Ownership errors:
  - Confirm RequireOwnership flags match intended caller scope.
  - Validate OwnerClientId and ConnectedClients before invoking RPCs.
  - Ensure authority patterns are correctly implemented for critical operations.
- Prefab registration:
  - Ensure player and object prefabs are present in DefaultNetworkPrefabs and registered with NetworkManager.
  - Verify NetworkTransform components are properly configured on networked objects.
- v2 Compatibility Issues:
  - Ensure Unity.Netcode.Components namespace is used for NetworkTransform components.
  - Update ServerRpc attributes to handle RequireOwnership deprecation warnings.
  - Verify package versions support v2 namespace changes.
  - Check for proper migration from ClientNetworkTransform to NetworkTransform.

**Updated** Enhanced with Netcode v2 specific troubleshooting scenarios including NetworkTransform issues, authority validation problems, and RequireOwnership attribute handling.

**Section sources**
- [PlayerNetwork.cs:184-195](file://Assets/FPS-Game/Scripts/Player/PlayerNetwork.cs#L184-L195)
- [PlayerTakeDamage.cs:58-83](file://Assets/FPS-Game/Scripts/Player/PlayerTakeDamage.cs#L58-L83)
- [DefaultNetworkPrefabs.asset:1-72](file://Assets/DefaultNetworkPrefabs.asset#L1-L72)
- [Grenade.prefab:215-259](file://Assets/FPS-Game/Prefabs/Weapon/Grenade.prefab#L215-L259)

## Conclusion
The FPS game leverages Netcode for GameObjects v2 through a clean component-based architecture with enhanced authority patterns. PlayerRoot orchestrates initialization order and subsystem references, while PlayerNetwork and PlayerTakeDamage implement ownership-aware logic, RPCs, and NetworkVariable-backed state synchronization. The new PlayerShoot and PlayerAnimation components demonstrate advanced networking patterns including authoritative combat mechanics and visual state synchronization. The migration to NetworkTransform from ClientNetworkTransform provides improved performance and full v2 compatibility. By following the patterns documented here—prioritizing initialization, using ServerRpc for authoritative updates, implementing NetworkTransform for smooth interpolation, and carefully managing RPC scope and bandwidth—you can build a robust, scalable multiplayer experience with enhanced security and performance using Netcode v2 compatibility features.

**Updated** Enhanced conclusion reflecting Netcode v2 improvements including NetworkTransform migration, Unity.Netcode.Components namespace usage, and authority-based patterns.

## Appendices

### Appendix A: Configuration and Prefabs
- NetcodeForGameObjects.asset configures default network prefabs and generation settings.
- DefaultNetworkPrefabs.asset enumerates prefabs eligible for network spawning.
- NetworkManager.prefab is the runtime anchor for the network session.
- PackageManagerSettings.asset controls package dependencies and registry configuration.

**Updated** Added PackageManagerSettings for dependency management with v2 compatibility.

**Section sources**
- [NetcodeForGameObjects.asset:1-18](file://ProjectSettings/NetcodeForGameObjects.asset#L1-L18)
- [DefaultNetworkPrefabs.asset:1-72](file://Assets/DefaultNetworkPrefabs.asset#L1-L72)
- [PackageManagerSettings.asset:1-36](file://ProjectSettings/PackageManagerSettings.asset#L1-L36)

### Appendix B: Authority and Security Patterns
- RequireOwnership=true for critical operations like combat and movement.
- ServerRpc validation prevents cheating through client-side manipulation.
- NetworkTransform provides smooth interpolation without authority conflicts.
- NetworkVariable updates ensure consistent state across all clients.

**New** Added comprehensive authority and security patterns for Netcode v2.

### Appendix C: NetworkTransform Integration
- Smooth client-side prediction for non-authority objects using Unity.Netcode.Components namespace.
- Interpolation settings for different movement types.
- Authority-based teleportation with interpolation control.
- v2 namespace compatibility for NetworkTransform components.
- Migration from ClientNetworkTransform to NetworkTransform for enhanced performance.

**New** Added NetworkTransform integration patterns for enhanced networking with v2 compatibility.

### Appendix D: v2 Migration Guide
- Unity.Netcode.Components namespace for NetworkTransform.
- ServerRpc RequireOwnership attribute deprecation handling.
- Updated package dependencies and registry configuration.
- Backward compatibility considerations for legacy code.
- ClientNetworkTransform to NetworkTransform migration checklist.

**New** Added comprehensive v2 migration guidance for Netcode upgrade.