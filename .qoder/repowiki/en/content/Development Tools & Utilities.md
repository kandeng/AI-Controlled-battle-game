# Development Tools & Utilities

<cite>
**Referenced Files in This Document**
- [DebugManager.cs](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs)
- [BehaviorTreeInspector.cs](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs)
- [ExternalBehaviorTreeInspector.cs](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs)
- [QualitySettings.asset](file://ProjectSettings/QualitySettings.asset)
- [EditorSettings.asset](file://ProjectSettings/EditorSettings.asset)
- [ProjectSettings.asset](file://ProjectSettings/ProjectSettings.asset)
- [README.md](file://README.md)
- [WIKI.md](file://WIKI.md)
</cite>

## Update Summary
**Changes Made**
- Removed references to CLEANUP_SUMMARY.md which was dropped during project migration
- Updated troubleshooting section to remove cleanup-specific guidance
- Enhanced documentation to reflect current available development tools
- Maintained all existing technical content while removing obsolete references

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
This document focuses on the development tools and utility systems that support debugging, testing, and development workflow in the project. It explains the debug system implementation, test components for input validation, and editor extensions. It also documents debugging capabilities such as performance monitoring, network state visualization, and AI behavior inspection, along with concrete examples from the codebase, configuration options for debug levels and logging verbosity, and relationships with other systems for development and quality assurance. Guidance is included for both beginners and experienced developers to address common issues like debug performance impact, test automation, and development environment setup.

## Project Structure
The development tools and utilities are primarily located under:
- Assets/FPS-Game/Scripts/Debug: Centralized debug utilities and managers
- Assets/Behavior Designer/Editor: Editor extensions for Behavior Designer trees
- ProjectSettings: Global configuration affecting build, editor, and quality behavior

```mermaid
graph TB
subgraph "Debug"
DM["DebugManager.cs"]
end
subgraph "Editor Extensions"
BTI["BehaviorTreeInspector.cs"]
EBTI["ExternalBehaviorTreeInspector.cs"]
end
subgraph "Project Settings"
QS["QualitySettings.asset"]
ES["EditorSettings.asset"]
PS["ProjectSettings.asset"]
end
DM --> QS
DM --> ES
DM --> PS
BTI --> DM
EBTI --> DM
```

**Diagram sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)
- [QualitySettings.asset:1-321](file://ProjectSettings/QualitySettings.asset#L1-L321)
- [EditorSettings.asset:1-31](file://ProjectSettings/EditorSettings.asset#L1-L31)
- [ProjectSettings.asset:85-132](file://ProjectSettings/ProjectSettings.asset#L85-L132)

**Section sources**
- [README.md:1-24](file://README.md#L1-L24)
- [WIKI.md:1-823](file://WIKI.md#L1-L823)

## Core Components
- DebugManager: A lightweight singleton responsible for debug-related toggles and global debug state. It exposes a flag to ignore player during debugging sessions.
- Behavior Designer Editor Extensions: Custom inspectors for Behavior Designer trees to improve visibility and editing in the Unity Editor.

These components collectively support rapid iteration, debugging, and validation during development.

**Section sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

## Architecture Overview
The development tools integrate with Unity's runtime and editor subsystems. The DebugManager acts as a central toggle for debug behaviors. Behavior Designer editor extensions enhance authoring workflows for AI behaviors.

```mermaid
graph TB
DM["DebugManager.cs<br/>Singleton debug state"]
BTI["BehaviorTreeInspector.cs<br/>Editor extension"]
EBTI["ExternalBehaviorTreeInspector.cs<br/>Editor extension"]
DM --> BTI
DM --> EBTI
```

**Diagram sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

## Detailed Component Analysis

### DebugManager
- Purpose: Provide a global debug toggle and singleton lifecycle to avoid duplication.
- Key behaviors:
  - Singleton pattern ensures a single debug manager instance.
  - Public flag to ignore player during debugging sessions.
- Integration points:
  - Consumed by editor extensions to alter behavior during development.

```mermaid
classDiagram
class DebugManager {
+bool IgnorePlayer
+Awake()
+Instance
}
```

**Diagram sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)

**Section sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)

### Behavior Designer Editor Extensions
- Purpose: Improve authoring and inspection of Behavior Designer trees in the Unity Editor.
- Key behaviors:
  - Custom editors for BehaviorTree and ExternalBehaviorTree types.
  - Minimal overrides to preserve existing inspector behavior while integrating with the editor.
- Integration points:
  - Used by DebugManager to visualize AI decision-making during development.

```mermaid
classDiagram
class BehaviorTreeInspector {
+BehaviorTreeInspector()
}
class ExternalBehaviorTreeInspector {
+ExternalBehaviorTreeInspector()
}
BehaviorTreeInspector <|-- BehaviorDesigner_Editor_BehaviorInspector
ExternalBehaviorTreeInspector <|-- BehaviorDesigner_Editor_ExternalBehaviorInspector
```

**Diagram sources**
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

**Section sources**
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

## Dependency Analysis
- DebugManager depends on Unity's GameObject lifecycle and is consumed by editor extensions.
- Behavior Designer editor extensions depend on Unity Editor APIs and Behavior Designer runtime types.

```mermaid
graph TB
DM["DebugManager.cs"]
BTI["BehaviorTreeInspector.cs"]
EBTI["ExternalBehaviorTreeInspector.cs"]
DM --> BTI
DM --> EBTI
```

**Diagram sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

**Section sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [BehaviorTreeInspector.cs:1-11](file://Assets/Behavior%20Designer/Editor/BehaviorTreeInspector.cs#L1-L11)
- [ExternalBehaviorTreeInspector.cs:1-13](file://Assets/Behavior%20Designer/Editor/ExternalBehaviorTreeInspector.cs#L1-L13)

## Performance Considerations
- Quality settings: The project's quality presets influence rendering and runtime performance. Adjustments here can reduce overhead during debugging and testing.
- Editor settings: Texture streaming and async shader compilation can be enabled to improve editor responsiveness during iterative development.
- Project settings: Logging and analytics flags can be tuned to minimize overhead in development builds.

Practical tips:
- Use lower quality presets during heavy debugging sessions to maintain frame stability.
- Keep async shader compilation enabled to reduce editor stalls.
- Disable analytics and unnecessary logging in development builds to reduce noise and overhead.

**Section sources**
- [QualitySettings.asset:1-321](file://ProjectSettings/QualitySettings.asset#L1-L321)
- [EditorSettings.asset:1-31](file://ProjectSettings/EditorSettings.asset#L1-L31)
- [ProjectSettings.asset:85-132](file://ProjectSettings/ProjectSettings.asset#L85-L132)

## Troubleshooting Guide
Common issues and resolutions:
- Debug performance impact:
  - Reduce quality settings or disable expensive effects during debugging.
  - Use DebugManager.IgnorePlayer to skip heavy logic during tests.
- Development environment setup:
  - Ensure editor settings enable texture streaming and async shader compilation.
  - Verify Behavior Designer editor extensions are present to streamline AI authoring.

**Section sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [EditorSettings.asset:1-31](file://ProjectSettings/EditorSettings.asset#L1-L31)
- [README.md:1-440](file://README.md#L1-L440)
- [WIKI.md:1-823](file://WIKI.md#L1-L823)

## Conclusion
The development tools and utilities in this project provide a practical foundation for debugging, testing, and authoring workflows. DebugManager centralizes debug state, and Behavior Designer editor extensions enhance AI authoring. Together with project settings for quality, editor, and analytics, these components support efficient iteration and high-quality development practices.

## Appendices
- Configuration options overview:
  - Debug levels: Controlled via DebugManager.IgnorePlayer.
  - Logging verbosity: Adjust via Unity's player log and editor settings.
- Relationship with QA systems:
  - Behavior Designer editor extensions improve authoring reliability and reduce regression risk.

**Section sources**
- [DebugManager.cs:1-19](file://Assets/FPS-Game/Scripts/Debug/DebugManager.cs#L1-L19)
- [QualitySettings.asset:1-321](file://ProjectSettings/QualitySettings.asset#L1-L321)
- [EditorSettings.asset:1-31](file://ProjectSettings/EditorSettings.asset#L1-L31)
- [ProjectSettings.asset:85-132](file://ProjectSettings/ProjectSettings.asset#L85-L132)
- [README.md:1-440](file://README.md#L1-L440)
- [WIKI.md:1-823](file://WIKI.md#L1-L823)