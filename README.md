# Project Cobalt
 Unturned Rust Framework

## Stacking System
### Stacking
- [x] Dragging items onto other stacks to *stack* them
- [x] Picking up items will attempt to stakc them with others, and if not will create a new stack, also supports partial stacking
- [x] Salvaging items acts the same as picking them up.
### Splitting
- [x] Items can be put in the *splitter* allowing for fractions of the stack to be split into new stacks.
- [ ] *Splitter* should ideally be moved into an inventory slot instead of storage.
- [ ] Splitting data should be made to be crash proof to prevent item loss
## Building System
### Swapping Structures
- [x] Swapper UI can be opened by holding plugin hotkey 1 down, player can then choose type of structure.
- [ ] Ideally add cost display for current type, either on the Swapper UI or a seperate UI.
### Placement Cost
- [x] Allow for configuring cost of building/upgrading structure in a seperate file.
- [ ] Check if player has enough resources and if so remove them.
### Barricades
- [x] Placement of barricades should only use 1 if stacked.
- [ ] Health is not currently taken into account when salvaging.
### Salvaging/Picking Up
- [ ] Add custom UI for picking up barricades.
- [ ] Add ability to destroy recently placed structures using a hammer
## Crafting System
### Stackable Resources
- [x] Crafting recipes can make use of stacked resources and properly handle using resources from multiple stacks.
- [ ] Crafting recipes that produce multiple of an item properly grant stacked products *(May already been added idk)*
### Workbench System
- [ ] Crafting requires being near the appropriate workbench tier required for the blueprint.
## Blueprint System
### Researching 
- [ ] Allow for items to be researched through placing an item and X amount of scrap in research table, consuming the item and granting the blueprint.
- [ ] Allow for players to see what blueprints they have unlocked, ideally using a UI but a chat implementation may be used at first.
## Reloading System
### Ammo
- [x] G.u.n.s will share ammunition types similar to rust with r.i.f.l.e/p.i.s.t.o.l b.u.l.l.e.t.s, g.u.n.s will reload using the ammo from these stacks rather than mags.
## Junkpile System
### Spawning
- [x] Junkpiles spawn along roads randomly with a randomly generated assortment of barrels/crates on them.
- [ ] Make them spawn on the sides of the roads instead of down the center between nodes.
### Loot tables
- [ ] Allow for different types of loot crates i.e. normal/miitary/elite
- [ ] Allow for weighted drop chances for individual loot crate types
## Monuments
### Loot
- [ ] Monuments have individual loot spawn locations with a weight chance to be of a certain type i.e. normal/military
- [ ] Loot spawns are tracked by the server and respawned on interval in regards to how many players are online at the time.
### Puzzles
- [ ] Unlikely addition, or at least wont be added till later 
## Events
### Aidrop
- [ ] Airdrop should be randomly spawned and have a weighted drop table ideally containing x amount of y type items.
### Locked Crate
- [ ] Locked crate event will occur at monuments, similar to rust it will have a countdown which is started by a player.
- [ ] Potentially marked on the map somehow or the location will be announced in chat.
## Loot caches
### Bodybags
- [ ] Spawn a bodybag containing a players loot after death.
- [ ] Add a seperate section with clothing
- [ ] Prevent the addition of items to the storage
- [ ] Add a life timer to clear the barricade after x time.
- [ ] Make the life timer extended based on the value of the items in the bag.
- [ ] Add data persistance in the event of crash/restart.
### Loot caches
- [ ] Spawn a loot cache when a barricade with items in is destroyed.
- [ ] Add similar functions as with the body bags.
## Smelting/Cooking
### Smelting
- [ ] Allow players to smelt mined ore by making use of a furnace
- [ ] Add Multiple sized capacity furnaces Normal/Large
- [ ] Different rates of smelting for different resources
### Cooking
- [ ] Allow players to cook stuff such as meat 
- [ ] Varying capacity - i.e. campfire/barbeque

## Claiming Land/Upkeep
**Very rough ideas, unsure of implementation**
### Upkeep/Claiming Land
- [ ] Make use of BaseClustering to group structures together
- [ ] Calculate an upkeep cost based on the structures attached to the TC.
- [ ] TC acts as a claimflag
### Decay
- [ ] Ideally layered decay similar to in rust, however if not then gradual overall decay.
- [ ] Resource specific decay i.e. stone only decaying if stone is not in TC
## Map
**These ideas are very rough and unlikely to be implemented**
Server generates a web server which allows players to login using the steam web client, and view a rendered map with events such as chinook crates, previous death locations ect.
