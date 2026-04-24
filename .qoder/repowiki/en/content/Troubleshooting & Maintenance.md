# Troubleshooting & Maintenance

<cite>
**Referenced Files in This Document**
- [DebugManager.cs](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs)
- [AIInputFeeder.cs](file://Assets/FPS-Game/Scripts/Bot/AIInputFeeder.cs)
- [WaypointPath.cs](file://Assets/FPS-Game/Scripts/Bot/WaypointPath.cs)
- [PerceptionSensor.cs](file://Assets/FPS-Game/Scripts/Bot/PerceptionSensor.cs)
- [BotController.cs](file://Assets/FPS-Game/Scripts/Bot/BotController.cs)
- [BlackboardLinker.cs](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs)
- [BotTactics.cs](file://Assets/FPS-Game/Scripts/Bot/BotTactics.cs)
- [PlayerRoot.cs](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs)
- [InGameManager.cs](file://Assets/FPS-Game/Scripts/System/InGameManager.cs)
- [GameMode.cs](file://Assets/FPS-Game/Scripts/System/GameMode.cs)
- [SpawnInGameManager.cs](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs)
- [GameSceneManager.cs](file://Assets/FPS-Game/Scripts/GameSceneManager.cs)
- [BehaviorTree.cs](file://Assets/Behavior%20Designer/Runtime/BehaviorTree.cs)
- [WebSocketServerManager.cs](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs)
- [WebSocket/README_WEBSOCKET_INSTALLATION.md](file://Assets/FPS-Game/Scripts/System/WebSocket/README_WEBSOCKET_INSTALLATION.md)
- [WebSocket/SETUP_GUIDE.md](file://Assets/FPS-Game/Scripts/System/WebSocket/SETUP_GUIDE.md)
- [CommandRouter.cs](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs)
- [PlayerUI.cs](file://Assets/FPS-Game/Scripts/Player/PlayerUI.cs)
- [NetcodeForGameObjects.asset](file://ProjectSettings/NetcodeForGameObjects.asset)
- [NetworkManager.prefab](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab)
- [System/NetworkManager.prefab](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab)
</cite>

## Update Summary
**Changes Made**
- Added comprehensive documentation for single-player mode initialization fixes
- Enhanced debugging information for NetworkManager initialization timing issues
- Updated troubleshooting procedures to include improved error handling and logging
- Added new section for single-player mode specific troubleshooting
- Enhanced NetworkManager initialization timing diagnostics

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Maintenance Procedures](#maintenance-procedures)
10. [Conclusion](#conclusion)

## Introduction
This document provides comprehensive troubleshooting and maintenance guidance for the project's networking infrastructure with enhanced focus on single-player mode initialization and NetworkManager timing issues. The documentation addresses modern networking connectivity problems, particularly "Cannot connect to host" scenarios, while emphasizing improved debugging capabilities for initialization timing issues. It covers AI behavior inconsistencies, performance bottlenecks, and asset loading errors, while providing systematic debugging approaches using the built-in debug system, log analysis, and network monitoring techniques.

## Project Structure
The project maintains a modular architecture with four primary networking modes:
- Direct Netcode connections using port 7777
- WebSocket integration for AI agent control
- Legacy lobby system (deprecated)
- Single-player mode for testing and development

```mermaid
graph TB
subgraph "Direct Netcode Mode"
A["NetworkManager.prefab<br/>Port 7777<br/>Address 127.0.0.1"]
B["System/NetworkManager.prefab<br/>ServerListenAddress 0.0.0.0"]
C["NetcodeForGameObjects.asset<br/>Default Prefabs"]
end
subgraph "WebSocket Mode"
D["WebSocketServerManager.cs<br/>Port 8080<br/>/agent endpoint"]
E["CommandRouter.cs<br/>Agent command routing"]
F["WebSocket/SETUP_GUIDE.md<br/>Installation instructions"]
end
subgraph "Single Player Mode"
G["InGameManager.cs<br/>SinglePlayer mode<br/>Auto-host startup"]
H["SpawnInGameManager.cs<br/>Timing-safe spawning"]
I["GameMode.cs<br/>SinglePlayer enum"]
end
subgraph "Legacy Systems"
J["LobbyManager.prefab<br/>Deprecated"]
K["PlayerUI.cs<br/>Quit handling"]
L["EscapeUI.cs<br/>Quit button logic"]
end
A --> C
B --> C
D --> E
F --> D
G --> H
H --> I
```

**Diagram sources**
- [NetworkManager.prefab:45-99](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L45-L99)
- [System/NetworkManager.prefab:46-99](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L46-L99)
- [WebSocketServerManager.cs:17-102](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L17-L102)
- [WebSocket/SETUP_GUIDE.md:1-51](file://Assets/FPS-Game/Scripts/System/WebSocket/SETUP_GUIDE.md#L1-L51)
- [PlayerUI.cs:128-170](file://Assets/FPS-Game/Scripts/Player/PlayerUI.cs#L128-L170)
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-38](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L38)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

**Section sources**
- [NetworkManager.prefab:45-99](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L45-L99)
- [System/NetworkManager.prefab:46-99](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L46-L99)
- [WebSocketServerManager.cs:17-102](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L17-L102)
- [WebSocket/SETUP_GUIDE.md:1-51](file://Assets/FPS-Game/Scripts/System/WebSocket/SETUP_GUIDE.md#L1-L51)
- [PlayerUI.cs:128-170](file://Assets/FPS-Game/Scripts/Player/PlayerUI.cs#L128-L170)
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-38](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L38)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

## Core Components
The networking system now centers around four key components with enhanced single-player support:
- **Direct Netcode Connections**: Using Unity Netcode for GameObjects with configurable port 7777
- **WebSocket Integration**: For AI agent control with port 8080 and /agent endpoint
- **Single-Player Mode**: Dedicated testing mode with automatic host initialization
- **Legacy Support**: Minimal lobby system remnants for backward compatibility

Key responsibilities:
- NetworkManager.prefab: Primary Netcode configuration with port 7777 settings
- WebSocketServerManager: Bi-directional communication for AI agents
- CommandRouter: Translates agent commands to game actions
- InGameManager: Central coordinator with game mode selection and timing-safe initialization
- SpawnInGameManager: Handles NetworkManager instantiation with proper timing
- NetcodeForGameObjects.asset: Default network prefab management

**Section sources**
- [NetworkManager.prefab:45-99](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L45-L99)
- [WebSocketServerManager.cs:17-102](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L17-L102)
- [CommandRouter.cs:9-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L9-L49)
- [InGameManager.cs:66-159](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L66-L159)
- [SpawnInGameManager.cs:5-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L5-L69)
- [NetcodeForGameObjects.asset:1-18](file://ProjectSettings/NetcodeForGameObjects.asset#L1-L18)

## Architecture Overview
The system now supports multiple networking architectures with clear separation of concerns and improved initialization timing:

```mermaid
sequenceDiagram
participant Client as "Client Device"
participant Netcode as "Netcode Transport<br/>Port 7777"
participant Server as "Server Host"
participant WS as "WebSocket Server<br/>Port 8080"
participant Agent as "AI Agent"
participant SP as "Single Player Mode"
Note over Client,Server : Direct Netcode Connection
Client->>Netcode : Connect to 127.0.0.1 : 7777
Netcode->>Server : Establish connection
Server-->>Client : Network session established
Note over Agent,WS : WebSocket Agent Integration
Agent->>WS : Connect ws : //localhost : 8080/agent
WS-->>Agent : Welcome message
Agent->>WS : Send commands (MOVE/LOOK/SHOOT)
WS->>CommandRouter : Route commands
CommandRouter->>Server : Execute actions
Note over SP : Single Player Initialization
SP->>Server : Auto-start host if not listening
Server-->>SP : Host ready with debug logging
```

**Diagram sources**
- [NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L92-L95)
- [System/NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L92-L95)
- [WebSocketServerManager.cs:71-95](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L71-L95)
- [CommandRouter.cs:14-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L14-L49)
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)

## Detailed Component Analysis

### Single-Player Mode Architecture
The single-player mode provides dedicated testing capabilities with automatic host initialization and improved timing safety:

```mermaid
classDiagram
class InGameManager {
+GameMode gameMode = SinglePlayer
+InitializeSinglePlayerMode()
+NetworkManager.Singleton.StartHost()
+Debug.Log("Single Player : NetworkManager started as host")
}
class SpawnInGameManager {
+Awaits NetworkManager.OnServerStarted
+TrySpawn() spawns InGameManager early
+Debug.Log("Spawned InGameManager sớm bằng OnServerStarted")
}
class GameMode {
<<enumeration>>
SinglePlayer
}
InGameManager --> GameMode : "uses"
SpawnInGameManager --> InGameManager : "spawns"
```

**Diagram sources**
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L69)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

**Section sources**
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L69)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

### Direct Netcode Connection Architecture
The primary networking mechanism uses Unity Netcode for GameObjects with configurable transport settings:

```mermaid
classDiagram
class NetworkManagerPrefab {
+NetworkConfig : ProtocolVersion, PlayerPrefab, Prefabs
+NetworkTransport : m_ProtocolType=1, m_MaxPayloadSize=6144
+ConnectionData : Address=127.0.0.1, Port=7777, ServerListenAddress=127.0.0.1
+EnableSceneManagement : 1, EnableNetworkLogs : 1
}
class SystemNetworkManager {
+ConnectionData : Address=127.0.0.1, Port=7777, ServerListenAddress=0.0.0.0
+RunInBackground : 1, LogLevel : 0
}
class NetcodeForGameObjects {
+NetworkPrefabsPath : Assets/DefaultNetworkPrefabs.asset
+GenerateDefaultNetworkPrefabs : 1
}
NetworkManagerPrefab --> NetcodeForGameObjects : "uses"
SystemNetworkManager --> NetcodeForGameObjects : "uses"
```

**Diagram sources**
- [NetworkManager.prefab:48-72](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L48-L72)
- [System/NetworkManager.prefab:48-72](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L48-L72)
- [NetcodeForGameObjects.asset:15-17](file://ProjectSettings/NetcodeForGameObjects.asset#L15-L17)

**Section sources**
- [NetworkManager.prefab:48-72](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L48-L72)
- [System/NetworkManager.prefab:48-72](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L48-L72)
- [NetcodeForGameObjects.asset:15-17](file://ProjectSettings/NetcodeForGameObjects.asset#L15-L17)

### WebSocket Integration for AI Agents
The WebSocket system provides bidirectional communication for AI agent control:

```mermaid
sequenceDiagram
participant Agent as "External Agent"
participant WS as "WebSocketServerManager"
participant Router as "CommandRouter"
participant Game as "Game System"
Agent->>WS : CONNECT ws : //localhost : 8080/agent
WS-->>Agent : WELCOME message
Agent->>WS : {"commandType" : "MOVE","data" : {"x" : 0,"y" : 0,"z" : 1}}
WS->>Router : ExecuteMove(player, command)
Router->>Game : Apply movement input
Game-->>Agent : GAME_STATE update
```

**Diagram sources**
- [WebSocketServerManager.cs:71-95](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L71-L95)
- [CommandRouter.cs:14-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L14-L49)

**Section sources**
- [WebSocketServerManager.cs:71-95](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L71-L95)
- [CommandRouter.cs:14-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L14-L49)

## Dependency Analysis
The networking system has evolved to minimize external dependencies with enhanced single-player support:
- Direct Netcode connections: Pure Unity Netcode implementation
- WebSocket integration: websocket-sharp library for AI agent control
- Single-player mode: Automatic NetworkManager host startup with timing safety
- Legacy lobby system: Minimal footprint, primarily for UI quit functionality

```mermaid
graph LR
PlayerRoot --> AIInputFeeder
BotController --> PerceptionSensor
BotController --> BlackboardLinker
BlackboardLinker --> BehaviorTree
BotController --> WaypointPath
InGameManager --> WaypointPath
InGameManager --> BotController
InGameManager --> SpawnInGameManager
GameSceneManager --> InGameManager
WebSocketServerManager --> CommandRouter
CommandRouter --> PlayerRoot
SpawnInGameManager --> InGameManager
```

**Diagram sources**
- [PlayerRoot.cs:159-366](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L159-L366)
- [BotController.cs:62-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L62-L485)
- [BlackboardLinker.cs:54-332](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs#L54-L332)
- [BehaviorTree.cs:6-11](file://Assets/Behavior%20Designer/Runtime/BehaviorTree.cs#L6-L11)
- [WaypointPath.cs:10-71](file://Assets/FPS-Game/Scripts/Bot/WaypointPath.cs#L10-L71)
- [InGameManager.cs:66-159](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L66-L159)
- [GameSceneManager.cs:4-26](file://Assets/FPS-Game/Scripts/GameSceneManager.cs#L4-L26)
- [WebSocketServerManager.cs:17-102](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L17-L102)
- [CommandRouter.cs:9-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L9-L49)
- [SpawnInGameManager.cs:5-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L5-L69)

**Section sources**
- [PlayerRoot.cs:159-366](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L159-L366)
- [BotController.cs:62-485](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L62-L485)
- [BlackboardLinker.cs:54-332](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs#L54-L332)
- [BehaviorTree.cs:6-11](file://Assets/Behavior%20Designer/Runtime/BehaviorTree.cs#L6-L11)
- [WaypointPath.cs:10-71](file://Assets/FPS-Game/Scripts/Bot/WaypointPath.cs#L10-L71)
- [InGameManager.cs:66-159](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L66-L159)
- [GameSceneManager.cs:4-26](file://Assets/FPS-Game/Scripts/GameSceneManager.cs#L4-L26)
- [WebSocketServerManager.cs:17-102](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L17-L102)
- [CommandRouter.cs:9-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L9-L49)
- [SpawnInGameManager.cs:5-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L5-L69)

## Performance Considerations
- **Netcode Optimization**: Configure m_MaxPacketQueueSize and m_MaxPayloadSize appropriately for your environment
- **WebSocket Efficiency**: Adjust broadcastInterval (default 0.1s) based on agent requirements
- **Single-Player Optimization**: Use SinglePlayer mode for development to avoid unnecessary networking overhead
- **Memory Management**: Monitor websocket-sharp library memory usage for long-running agent sessions
- **Logging Control**: Use EnableNetworkLogs judiciously to avoid performance impact during profiling

## Troubleshooting Guide

### Single-Player Mode Initialization Issues

**New** Added comprehensive single-player mode troubleshooting

Symptoms:
- Single-player mode fails to start host automatically
- NetworkManager not ready when InGameManager tries to initialize
- Timing issues with early spawning of InGameManager
- Debug logs not showing single-player initialization messages

Resolution steps:
1. **Verify Game Mode Configuration**:
   - Check InGameManager.gameMode is set to GameMode.SinglePlayer
   - Ensure SinglePlayer enum is properly defined in GameMode.cs
   - Verify NetworkManager.prefab has EnableSceneManagement enabled

2. **Check NetworkManager Timing**:
   - Confirm NetworkManager.Singleton is not null before StartHost()
   - Verify NetworkManager is not already listening before attempting to start
   - Check Unity console for "[InGameManager] Single Player: NetworkManager started as host"

3. **Validate Spawn Timing Safety**:
   - Ensure SpawnInGameManager subscribes to NetworkManager.Singleton.OnServerStarted
   - Verify TrySpawn() method checks for existing InGameManager instance
   - Confirm NetworkObject component exists on InGameManager prefab

4. **Debug Initialization Flow**:
   - Look for "[SpawnInGameManager] NetworkManager not ready yet" log messages
   - Check "[SpawnInGameManager] Spawned InGameManager sớm bằng OnServerStarted" logs
   - Verify NetworkManager initialization order in Unity editor

**Section sources**
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L69)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

### Direct Netcode Connection Issues ("Cannot connect to host")

**Updated** Enhanced with improved debugging information

Symptoms:
- Client cannot establish connection to server host
- Connection timeout errors during startup
- Port 7777 blocked by firewall or antivirus
- Incorrect IP address configuration
- NetworkManager initialization timing issues

Resolution steps:
1. **Verify Netcode Configuration**:
   - Check NetworkManager.prefab ConnectionData.Address and Port settings
   - Ensure ServerListenAddress is configured correctly (127.0.0.1 for localhost, 0.0.0.0 for external access)
   - Confirm ProtocolType is set to TCP (value 1)

2. **Firewall and Security Software**:
   - Add exception for port 7777 in Windows Firewall
   - Temporarily disable antivirus firewall to test connectivity
   - Verify router port forwarding if connecting across networks

3. **IP Connectivity Verification**:
   - Test telnet 127.0.0.1 7777 from client machine
   - Use netstat -an | findstr 7777 to verify server listening
   - Check if multiple instances are using the same port

4. **Network Interface Binding**:
   - For external connections, set ServerListenAddress to 0.0.0.0
   - For localhost testing, use 127.0.0.1
   - Verify network adapter configuration

5. **Initialization Timing Diagnostics**:
   - Monitor Unity console for NetworkManager initialization logs
   - Check for timing conflicts between NetworkManager and InGameManager startup
   - Verify EnableNetworkLogs setting in NetworkManager.prefab

**Section sources**
- [NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L92-L95)
- [System/NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L92-L95)
- [NetcodeForGameObjects.asset:15-17](file://ProjectSettings/NetcodeForGameObjects.asset#L15-L17)

### WebSocket Integration Issues (AI Agent Control)

**New** Added comprehensive WebSocket troubleshooting

Symptoms:
- AI agents cannot connect to Unity game
- WebSocket server fails to start
- Port 8080 blocked by firewall
- Command routing failures
- Missing websocket-sharp library

Resolution steps:
1. **Verify WebSocket Installation**:
   - Ensure websocket-sharp library is properly installed via Package Manager
   - Check that websocket-sharp.dll exists in Assets/Plugins/
   - Verify no compilation errors in WebSocket components

2. **Firewall Configuration**:
   - Add exception for port 8080 in Windows Firewall
   - Test connectivity using ws://localhost:8080/agent
   - Verify no security software blocking WebSocket connections

3. **Server Initialization**:
   - Confirm WebSocketServerManager is attached to InGameManager
   - Check port and endpoint configuration (default 8080, /agent)
   - Verify server starts successfully in Unity console

4. **Command Processing**:
   - Test basic agent commands (MOVE, LOOK, SHOOT)
   - Monitor CommandRouter execution logs
   - Verify agent session tracking and command routing

**Section sources**
- [WebSocketServerManager.cs:71-95](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L71-L95)
- [WebSocket/README_WEBSOCKET_INSTALLATION.md:49-55](file://Assets/FPS-Game/Scripts/System/WebSocket/README_WEBSOCKET_INSTALLATION.md#L49-L55)
- [WebSocket/SETUP_GUIDE.md:1-51](file://Assets/FPS-Game/Scripts/System/WebSocket/SETUP_GUIDE.md#L1-L51)
- [CommandRouter.cs:14-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L14-L49)

### Legacy System Issues (Deprecated)

**Updated** Removed Unity Services and Lobby troubleshooting

Symptoms:
- References to LobbyManager still appear in code
- Quit game functionality issues
- UI elements referencing lobby system

Resolution steps:
1. **Verify System Cleanup**:
   - Confirm LobbyManager.prefab is no longer referenced
   - Check PlayerUI.cs and EscapeUI.cs for lobby-dependent code
   - Ensure all lobby-related functionality has been removed

2. **Quit Functionality Testing**:
   - Test quit button behavior in EscapeUI
   - Verify proper NetworkManager shutdown
   - Confirm application exits cleanly without lobby dependencies

**Section sources**
- [PlayerUI.cs:128-170](file://Assets/FPS-Game/Scripts/Player/PlayerUI.cs#L128-L170)
- [EscapeUI.cs:9-19](file://Assets/FPS-Game/Scripts/Player/PlayerCanvas/EscapeUI.cs#L9-L19)

### Networking Synchronization Issues
Symptoms:
- Clients desync from server state
- Inputs not applied consistently
- Scene transitions lose managers
- Single-player mode timing conflicts

Resolution steps:
1. Verify Netcode for GameObjects initialization:
   - Ensure NetworkManager prefab is present and initialized.
   - Confirm PlayerRoot inherits NetworkBehaviour and initializes subsystems via priority hooks.
   - Check InGameManager.InitializeSinglePlayerMode() for proper host startup.
2. Check scene transitions:
   - Use GameSceneManager.LoadScene to load scenes asynchronously and avoid reinitialization conflicts.
   - Verify SpawnInGameManager timing safety with OnServerStarted event subscription.
3. Validate RPC flows:
   - Confirm server-side RPCs are invoked only on the server host.
   - Ensure ClientRpc callbacks receive expected data and trigger UI updates safely.
4. Monitor connection stability:
   - Observe logs for warnings from InGameManager.PathFinding indicating path calculation failures.
   - Inspect NavMesh surfaces and avoid placing bots on invalid geometry.
5. Debug single-player timing:
   - Check "[SpawnInGameManager] NetworkManager not ready yet" messages
   - Verify NetworkManager initialization order in Unity console

**Section sources**
- [PlayerRoot.cs:202-366](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L202-L366)
- [GameSceneManager.cs:20-26](file://Assets/FPS-Game/Scripts/GameSceneManager.cs#L20-L26)
- [InGameManager.cs:146-194](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L146-L194)
- [InGameManager.cs:202-231](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L202-L231)
- [SpawnInGameManager.cs:20-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L69)

### AI Behavior Anomalies
Symptoms:
- AI ignores player despite being visible
- AI does not patrol waypoints
- AI fails to start behaviors
- Single-player mode bot spawning issues

Resolution steps:
1. Confirm PerceptionSensor:
   - Verify FOV/range and obstacle masks.
   - Check last-known position updates and OnPlayerLost triggers.
2. Validate BlackboardLinker:
   - Ensure BindToBehavior is called when switching behaviors.
   - Confirm BD variables are set for current behavior (idle/patrol/combat).
3. Inspect BotController:
   - Verify state transitions and StartBehavior/StopBehavior calls.
   - Ensure AIInputFeeder receives movement/look/attack signals.
4. Debug Behavior Designer:
   - Confirm BehaviorTree components are enabled and seeded.
   - Check GlobalVariables for required keys.
5. Check Single-Player Bot Spawning:
   - Verify HandleSpawnBot.SpawnAllBots() runs on server
   - Check "[HandleSpawnBot] Spawning {botCount} bots" debug logs
   - Ensure BotList dictionary is properly populated

**Section sources**
- [PerceptionSensor.cs:64-107](file://Assets/FPS-Game/Scripts/Bot/PerceptionSensor.cs#L64-L107)
- [BlackboardLinker.cs:86-113](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs#L86-L113)
- [BotController.cs:230-275](file://Assets/FPS-Game/Scripts/Bot/BotController.cs#L230-L275)
- [AIInputFeeder.cs:12-29](file://Assets/FPS-Game/Scripts/Bot/AIInputFeeder.cs#L12-L29)
- [BehaviorTree.cs:6-11](file://Assets/Behavior%20Designer/Runtime/BehaviorTree.cs#L6-L11)
- [HandleSpawnBot.cs:27-55](file://Assets/FPS-Game/Scripts/System/HandleSpawnBot.cs#L27-L55)

### Performance Bottlenecks
Symptoms:
- Frame rate drops during AI scanning
- Excessive NavMesh calculations
- Overdraw in gizmos
- Single-player mode initialization delays

Resolution steps:
1. Disable debug gizmos:
   - Turn off PerceptionSensor and BotTactics gizmo drawing in play mode.
2. Reduce perception sampling:
   - Increase sample intervals and adjust FOV/range thresholds.
3. Optimize pathfinding:
   - Reuse calculated paths and avoid per-frame recalculation.
   - Use InGameManager.PathFinding to compute movement vectors efficiently.
4. Profile Behavior Designer:
   - Minimize variable reads/writes per frame.
   - Batch BD updates in BlackboardLinker.
5. Optimize Single-Player Mode:
   - Use SinglePlayer mode for development to avoid unnecessary networking overhead
   - Monitor "[SpawnInGameManager] Spawned InGameManager sớm bằng OnServerStarted" logs
   - Verify timing-safe initialization prevents race conditions

**Section sources**
- [PerceptionSensor.cs:296-324](file://Assets/FPS-Game/Scripts/Bot/PerceptionSensor.cs#L296-L324)
- [BotTactics.cs:368-456](file://Assets/FPS-Game/Scripts/Bot/BotTactics.cs#L368-L456)
- [InGameManager.cs:202-231](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L202-L231)
- [BlackboardLinker.cs:254-329](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs#L254-L329)

### Asset Loading Errors
Symptoms:
- Missing prefabs or components after scene load
- NullReference exceptions in PlayerRoot subsystems
- Single-player mode prefab reference issues

Resolution steps:
1. Verify Prefab references:
   - Ensure PlayerRoot references (e.g., WeaponHolder, PlayerModel) are assigned in prefabs.
   - Check InGameManager.prefab references in SinglePlayer mode.
2. Use child lookup:
   - Rely on SetBotController and FindChildWithTag to locate child components dynamically.
3. Check component priorities:
   - Ensure subsystems implement IInitAwake/IInitStart/IInitNetwork with appropriate priorities.
4. Validate scene loading:
   - Use GameSceneManager to avoid duplicate managers and ensure persistence.
5. Debug Single-Player Prefabs:
   - Verify NetworkManager.prefab has proper NetworkObject component
   - Check DefaultNetworkPrefabs.asset references
   - Ensure InGameManager prefab is properly configured

**Section sources**
- [PlayerRoot.cs:247-296](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L247-L296)
- [PlayerRoot.cs:298-339](file://Assets/FPS-Game/Scripts/Player/PlayerRoot.cs#L298-L339)
- [GameSceneManager.cs:8-18](file://Assets/FPS-Game/Scripts/GameSceneManager.cs#L8-L18)
- [HandleSpawnBot.cs:43-55](file://Assets/FPS-Game/Scripts/System/HandleSpawnBot.cs#L43-L55)

### Step-by-Step Resolution Procedures

#### Single-Player Mode Initialization Failure
**New** Comprehensive single-player troubleshooting procedure

1. **Verify Game Mode Configuration**:
   - Check InGameManager.gameMode is set to GameMode.SinglePlayer in inspector
   - Confirm GameMode.SinglePlayer enum is properly defined
   - Verify NetworkManager.prefab has EnableSceneManagement enabled

2. **Check NetworkManager Timing**:
   - Ensure NetworkManager.Singleton is not null before StartHost()
   - Verify NetworkManager is not already listening
   - Monitor Unity console for "[InGameManager] Single Player: NetworkManager started as host"

3. **Validate Spawn Timing Safety**:
   - Confirm SpawnInGameManager subscribes to OnServerStarted event
   - Check TrySpawn() method prevents duplicate InGameManager instances
   - Verify NetworkObject component exists on InGameManager prefab

4. **Debug Initialization Flow**:
   - Look for "[SpawnInGameManager] NetworkManager not ready yet" messages
   - Check "[SpawnInGameManager] Spawned InGameManager sớm bằng OnServerStarted" logs
   - Verify NetworkManager initialization order in Unity console

**Section sources**
- [InGameManager.cs:174-187](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L174-L187)
- [SpawnInGameManager.cs:20-69](file://Assets/FPS-Game/Scripts/System/SpawnInGameManager.cs#L20-L69)
- [GameMode.cs:16-20](file://Assets/FPS-Game/Scripts/System/GameMode.cs#L16-L20)

#### Direct Netcode Connection Failure
**Updated** Enhanced with improved debugging information

1. **Verify Server Configuration**:
   - Check System/NetworkManager.prefab ServerListenAddress (should be 0.0.0.0 for external access)
   - Confirm Port is set to 7777 in both NetworkManager.prefab and System/NetworkManager.prefab
   - Verify ProtocolType is TCP (value 1)

2. **Firewall and Security Testing**:
   - Add Windows Firewall exception for port 7777
   - Temporarily disable antivirus firewall to test connectivity
   - Use telnet 127.0.0.1 7777 to verify port accessibility

3. **Network Interface Binding**:
   - For localhost testing: set Address to 127.0.0.1
   - For external connections: set Address to server's IP and ServerListenAddress to 0.0.0.0
   - Verify network adapter is functioning correctly

4. **Connection Verification**:
   - Monitor Unity console for "Connection established" messages
   - Check NetworkManager logs for successful client authentication
   - Verify scene transition completes without manager reinitialization
   - Look for NetworkManager initialization timing logs

**Section sources**
- [System/NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/System/NetworkManager.prefab#L92-L95)
- [NetworkManager.prefab:92-95](file://Assets/FPS-Game/Prefabs/NetworkManager.prefab#L92-L95)
- [NetcodeForGameObjects.asset:15-17](file://ProjectSettings/NetcodeForGameObjects.asset#L15-L17)

#### WebSocket Agent Connection Failure
**New** Comprehensive WebSocket troubleshooting procedure

1. **Library Installation Verification**:
   - Confirm websocket-sharp library installed via Package Manager
   - Check Assets/Plugins contains websocket-sharp.dll
   - Verify no compilation errors in WebSocket components

2. **Server Startup Testing**:
   - Check Unity console for "[WebSocketServer] Server started on ws://0.0.0.0:8080/agent"
   - Verify WebSocketServerManager component attached to InGameManager
   - Test manual server initialization if auto-start disabled

3. **Firewall Configuration**:
   - Add Windows Firewall exception for port 8080
   - Test connectivity using ws://localhost:8080/agent
   - Verify no security software blocking WebSocket connections

4. **Agent Communication Testing**:
   - Send basic commands (MOVE, LOOK, SHOOT) from agent
   - Monitor CommandRouter execution logs
   - Verify game state broadcasts to agents

**Section sources**
- [WebSocketServerManager.cs:71-95](file://Assets/FPS-Game/Scripts/System/WebSocketServerManager.cs#L71-L95)
- [WebSocket/README_WEBSOCKET_INSTALLATION.md:49-55](file://Assets/FPS-Game/Scripts/System/WebSocket/README_WEBSOCKET_INSTALLATION.md#L49-L55)
- [CommandRouter.cs:14-49](file://Assets/FPS-Game/Scripts/System/CommandRouter.cs#L14-L49)

#### Desynchronization Issues
1. Inspect movement inputs:
   - Verify AIInputFeeder receives OnMove/OnLook/OnAttack events.
2. Check Behavior Designer variables:
   - Ensure BlackboardLinker binds to active behavior and sets required variables.
3. Validate NavMesh path:
   - Use InGameManager.PathFinding to confirm movement direction computation.
4. Check Single-Player Mode Timing:
   - Verify NetworkManager initialization order prevents race conditions
   - Monitor SpawnInGameManager timing logs

**Section sources**
- [AIInputFeeder.cs:12-29](file://Assets/FPS-Game/Scripts/Bot/AIInputFeeder.cs#L12-L29)
- [BlackboardLinker.cs:86-113](file://Assets/FPS-Game/Scripts/Bot/BlackboardLinker.cs#L86-L113)
- [InGameManager.cs:202-231](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L202-L231)

#### AI Pathfinding Failures
1. Verify NavMesh surfaces:
   - Ensure NavMesh is baked and accessible for bot positions.
2. Check PathFinding inputs:
   - Confirm owner and target transforms are valid.
3. Review path corners:
   - Log warnings indicate insufficient corners; adjust target positions or NavMesh.
4. Check Single-Player Bot Spawning:
   - Verify HandleSpawnBot.SpawnAllBots() runs on server
   - Check bot controller attachment and initialization

**Section sources**
- [InGameManager.cs:202-231](file://Assets/FPS-Game/Scripts/System/InGameManager.cs#L202-L231)
- [HandleSpawnBot.cs:27-55](file://Assets/FPS-Game/Scripts/System/HandleSpawnBot.cs#L27-L55)

## Maintenance Procedures

### Project Cleanup
- Remove unused prefabs and assets from scenes.
- Delete obsolete Behavior Designer variables and unused tasks.
- Clean up orphaned components on PlayerRoot and BotController.
- **Updated**: Remove legacy lobby system references and dependencies.
- **New**: Remove unused single-player mode configurations if not needed.

### Dependency Updates
- Update Unity packages via Package Manager (URP, Netcode for GameObjects, websocket-sharp).
- Reimport assets after package updates to resolve missing references.
- **Updated**: Verify websocket-sharp library compatibility with Unity version.
- **New**: Update Netcode for GameObjects to latest version for improved timing safety.

### Performance Optimization
- Reduce gizmo rendering in play mode.
- Batch Behavior Designer variable updates in BlackboardLinker.
- Cache NavMesh paths and reuse movement vectors.
- **Updated**: Monitor WebSocket server performance for long-running agent sessions.
- **New**: Optimize single-player mode initialization timing with proper event subscriptions.

### Production Monitoring Strategies
- Enable structured logging for AI state transitions and perception events.
- Monitor RPC throughput and latency; alert on excessive packet loss.
- Track NavMesh bake quality and surface connectivity.
- **Updated**: Monitor WebSocket connection counts and command processing rates.
- **New**: Monitor single-player mode initialization timing and NetworkManager startup logs.

## Conclusion
This guide consolidates practical troubleshooting and maintenance practices for the project's networking infrastructure with enhanced focus on single-player mode initialization and NetworkManager timing issues. The addition of comprehensive single-player mode support provides dedicated testing capabilities with automatic host initialization and improved timing safety. By leveraging Netcode port 7777 configuration, WebSocket integration for AI agents, single-player mode debugging, and the debug system, teams can systematically diagnose and resolve connectivity issues while maintaining robust production workflows. Regular cleanup, dependency updates, and performance tuning ensure long-term project health, with special attention to firewall configuration, direct connection verification, and improved initialization timing diagnostics.