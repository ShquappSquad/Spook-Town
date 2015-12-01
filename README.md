# Spook-Town

Game Development starter project using Unity and Blender

## Contents
* Environment
* Objective
* Interactions
* Animations
* Menus

## Environment
The game environment is a dark graveyard with semi-randomized walls, graves, scenery, and pathways. There are multiple grave models, and one grave is specified as the cursed grave, with a special model and particles around it.

Periodically along the pathways are unlit torches that can be lit by the player and will emit a small amount of light.

Score for the background music can be found on [Noteflight](https://www.noteflight.com/scores/view/e0573185c1fb8fa6ccde44f62383f8c5700a559f).

## Objective
The goal of the game is to find and purify the cursed grave. Obstacles to achieving this goal include the darkness of the graveyard and ghosts that spawn and attempt to kill the player.

## Interactions
The player carries a torch which can be used to light the torches in the environment by standing next to them. The player also has a number of charms which they can use to banish individual ghosts.

Ghosts spawn periodically in the environment and wander around until the player comes in a certain range, at which point they pursue the player. If a ghost successfully attacks the player, the player is killed.

## Animations
Player:
* Spawn
* Walk
* Light torch?
* Use charm
* Cleanse cursed grave
* Death

Ghosts:
* Spawn
* Move
* Notice player
* Attack
* Death

Torches (environment):
* On light up
* Passive (lit)

Cursed grave:
* Passive (particles)
* Player cleanses grave

## Menus
Pause menu: Contains options to resume, how to play, and exit game.
Main menu: Contains options to start and quit.