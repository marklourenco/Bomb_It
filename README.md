# Bomb It

A Bomberman-style multiplayer game built in Unity, with **fully hand-rolled peer-to-peer networking**. No Unity Netcode, Mirror, or Photon. Built as a class final project to demonstrate custom network setup and communication.

## Overview

Bomb It is a top-down grid game for 2–4 players. Players move freely around a 13x11 arena, drop bombs that explode in a plus-shaped blast after a fixed fuse, destroy crates to reveal random upgrades, and try to be the last one standing.

One player hosts the match; everyone else connects directly to them over UDP. The host runs the only real simulation and streams the game state to every client, so all players always see the same thing.

## Features

- 2–4 player free-for-all, last player standing wins.
- Bombs with a fixed fuse timer, plus-shaped explosions that destroy crates and chain into other bombs.
- Three upgrade types dropped from destroyed crates: bomb range, bomb count, and move speed.
- A lobby screen to host or join a game, with a live list of connected players.
- Host-controlled **Start Game** button. Players can join and look around the map, but no one can move or place a bomb until the host starts the round.
- A **seed field** in the host panel so the same crate/wall layout can be reproduced across runs without rebuilding, or left blank for a random layout.
- A win/game-over screen announcing the winner (or a draw), with a **Play Again** button that resets the round without dropping anyone's connection.
- Pixel art environment (Cainos "Pixel Art Top Down - Basic" pack) with randomized floor tile variety, plus color-coded player identities.

## Controls

| Action | Input |
| --- | --- |
| Move | WASD / Arrow keys |
| Place bomb | Space |

## How to Play

1. One player opens the game and, in the **Host a game** panel, optionally sets a port and a seed (leave the seed blank for a random layout), then presses **Start Host**.
2. Everyone else enters the host's address and port in the **Join a game** panel and presses **Connect**.
   - On the same Wi-Fi/LAN, use the local network address shown in the host's status panel.
   - Across different networks, the host needs to forward the chosen port on their router and share their *public* IP address instead. This isn't handled automatically.
3. Once everyone has joined, the host presses **Start Game** to begin the round. Movement and bomb placement are locked until then.
4. Last player alive wins. When the round ends, the host can press **Play Again** to reset the map and jump into another round.

## Networking Architecture

The netcode is built directly on raw UDP sockets. No third-party networking library.

- **Host-authoritative simulation**: only the host runs the real game logic (movement, bomb fuses, explosions, upgrades). Clients never simulate gameplay locally.
- **Input up, snapshot down**: each client sends its raw input (movement axes + a bomb-request counter) to the host every tick over an unreliable channel. The host applies input from all players, ticks the simulation, and broadcasts a compact state snapshot back to everyone.
- **Snapshots over unreliable UDP**: since packets can be dropped or arrive out of order, the game is designed to tolerate that. Clients simply render whatever the latest snapshot says, and a lost snapshot is superseded by the next one moments later.
- **Reliable one-shot events over an unreliable channel**: instead of a single "place bomb" message that could be lost, each input packet carries an ever-incrementing bomb-request counter. The host compares it against the last value it processed for that player, so a request is never silently dropped, without needing an ack/retry system.
- **Manual packet encoding**: grid state, player state, bombs, explosions, and pickups are all packed into a byte-budgeted custom format (`StateSnapshot`) rather than relying on generic serialization.

## Requirements

- Unity (Universal Render Pipeline, 2D Renderer).
- No external packages or asset store networking plugins required.

## Known Limitations

- **LAN by default.** Joining across different networks works over the same UDP code, but requires manual port forwarding on the host's router and sharing the host's public (not local) IP address. There's no NAT traversal/rendezvous server.
- **No reconnect.** If a client disconnects mid-round, they can't rejoin that same round.
- **No sound yet.** Sound effects are planned but not yet implemented.

## Credits

- Environment art: "Pixel Art Top Down - Basic" by Cainos.
