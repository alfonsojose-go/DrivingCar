# **Driving Car**

A 2D arcade-style driving game built with C#, demonstrating object-oriented programming principles, collision detection, and game state management.

<p>
  <img src="https://img.shields.io/badge/C%23-11-purple?style=flat-surface" alt="C# 11" />
</p>

<p>
  <img src="https://img.shields.io/badge/.NET-7-blue?style=flat-surface" alt=".NET 7" />
</p>

<p>
  <img src="https://img.shields.io/badge/MonoGame-3.8-orange?style=flat-surface" alt="Monogame 3.8" />
</p>

## **Problem Statement**

Game development is often taught with engines (Unity, Unreal) that abstract away core programming concepts. I wanted to build a game from the ground up to demonstrate:
- Pure OOP design without engine shortcuts
- Manual collision detection and physics
- State machine architecture for game flow
- Asset management and rendering pipelines
  
This project proves that strong fundamentals in C# and software architecture translate directly to interactive applications—not just business software.

## **Tech Stack**
| Layer            | Technology            | Purpose                                                       |
| ---------------- | --------------------- | ------------------------------------------------------------- |
| Language         | C# 11                 | Strongly-typed game logic                                     |
| Framework        | MonoGame 3.8          | Cross-platform game framework (rendering, input, audio)       |
| Platform         | .NET 7                | Runtime and build system                                      |
| Graphics         | SpriteBatch 2D        | 2D texture rendering and animation                            |
| Physics          | Custom AABB collision | Manual collision detection without physics engine             |
| State Management | State Machine Pattern | Clean separation of game modes (menu, play, pause, game over) |
| Data Persistence | JSON serialization    | High score storage and settings                               |

## **Key Features**
- **Top-down driving mechanics** — Acceleration, deceleration, steering with momentum and friction
- **Procedural obstacle generation** — Randomly placed obstacles with increasing density and speed as score rises
- **Collision detection system** — Custom Axis-Aligned Bounding Box (AABB) collision with collision response
- **Game state machine** — Clean transitions between Title, Playing, Paused, and GameOver states
- **Score and high score system** — Persistent leaderboard using JSON file storage
- **Difficulty scaling** — Game speed and obstacle frequency increase every 100 points
- **Sound effects** — Engine audio, collision sounds, and milestone celebrations using MonoGame's audio pipeline
- **Particle effects** — Simple explosion particles on collision for visual feedback

## **Architecture Decisions**

**Why MonoGame instead of Unity?**

Unity is powerful but hides the underlying mechanics. MonoGame provides:
- Direct access to the game loop (Update() / Draw())
- Manual sprite rendering and transformation
- No scene graph or component system—forcing explicit OOP design
- Lightweight deployment (single executable, no engine runtime)
  
This was a deliberate choice to demonstrate that I understand what Unity abstracts away.

**Why custom collision detection instead of a physics library?**

For a simple 2D game, Box2D or Farseer Physics would be overkill. Implementing AABB collision manually:
- Demonstrates understanding of coordinate geometry
- Keeps the codebase dependency-free
- Allows fine-tuned collision response (e.g., slight bounce vs. instant stop)
- Proves I can solve spatial problems without library assistance
  
**Why a state machine for game flow?**
  
Games have distinct modes with different input handling, rendering, and update logic. A naive approach uses boolean flags (isPaused, isGameOver). This becomes unmaintainable. The state machine pattern:
- Encapsulates each state's behavior in its own class
- Prevents illegal transitions (e.g., can't pause from game over)
- Makes adding new states (settings menu, level select) trivial

## **Challenges & Solutions**

**Challenge: Frame-rate independent movement**

Early versions ran at different speeds on different hardware. A fast PC made the game unplayable; a slow PC made it sluggish.

**Solution:** Implemented **delta-time-based movement**. All position updates use gameTime.ElapsedGameTime.TotalSeconds:
```csharp
// Before (frame-dependent)
player.Position += velocity; // Moves 5 pixels per frame

// After (frame-independent)
player.Position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds; // Moves 300 pixels per second
```

This ensures consistent gameplay across 30fps, 60fps, or 144fps displays.

**Challenge: Collision detection performance**

With 50+ obstacles on screen, checking every obstacle against the player every frame became expensive (O(n) per frame).

**Solution:** Implemented **spatial partitioning** with a simple grid system. The screen is divided into a 4x4 grid; each obstacle is assigned to a cell. The player only checks collisions with obstacles in their current cell and adjacent cells. This reduced collision checks from 50+ to an average of 6 per frame.

**Challenge: Game state cleanup**

Switching from GameOver back to Title screen left old game objects in memory, causing memory leaks and visual artifacts.

**Solution:** Enforced a **strict lifecycle contract** for game objects:
```csharp
interface IGameObject
{
    void LoadContent();   // Load textures, sounds
    void Update(GameTime gameTime);  // Logic update
    void Draw(SpriteBatch spriteBatch);  // Rendering
    void Unload();  // Cleanup resources
}
```
The state machine calls Unload() on all objects before transitioning, ensuring clean state resets.

## **How to Run**

**Prerequisites**

- .NET 7 SDK
- Visual Studio 2022 or VS Code with C# extension
  
**Setup**

```bash
git clone https://github.com/alfonsojose-go/DrivingCar.git
cd DrivingCar
dotnet restore
dotnet run
```

**Controls**
| Key               | Action        |
| ----------------- | ------------- |
| Arrow Keys / WASD | Drive         |
| Space             | Brake         |
| P                 | Pause         |
| Escape            | Quit to Title |


## **What I learned**
- **Game loop architecture:** The tight coupling between Update and Draw phases, and why separating logic from rendering prevents frame drops.
- **Collision mathematics:** AABB is simple, but getting the collision response to feel "right" (not too bouncy, not too sticky) requires tuning. I learned that game feel is 80% math and 20% magic.
- **Memory management in games:** Loading all assets at startup causes long load times. Loading per-level causes hitches. I learned to implement an asset manager with reference counting that loads on first use and unloads when no longer needed.
- **OOP in practice:** Inheritance hierarchies (GameObject -> MovingObject -> Vehicle) seemed clean initially but became rigid. I refactored toward composition (Entity + MovementComponent + CollisionComponent + RenderComponent), which is the pattern real game engines use.

- ## **Links**
- - [GitHub Reposity](https://github.com/alfonsojose-go/DrivingCar)

  **License:** MIT

