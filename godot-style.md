# Preface
- Game development is quite eclectic. and thinking about code in too general of a way is actually a bad thing. Style guides like these are best served in a per-project basis, but our projects aren't big enough to justify the r&d required to come up with the best way to do things for each. We can come up with a "good enough" case that we can carry between projects.

# CSharp
- Why not GDScript? 
	- missing a lot of higher level language features I really enjoy
	- strong type system of C# allows for easier and more readable code 
	- no nullability checking?
	- no namespaces????
- Everyone install [CSharpier](https://csharpier.com/). It kind of sucks (Allman Braces) but a bad formatter is better than no formatter. There's a vscode extension for it.
## Other specific C# style stuff
- Use [null-conditional operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-) and [nullability checking](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) to avoid billion dollar mistakes, C# 14 is bringing [null conditional assignment](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#null-conditional-assignment) too 
``` C#
Object? mightBeNull = TryGetObject()
mightBeNull?.DoThing(); // won't run if null
```
- Instance node references with ``Node3D node = null!`` to prevent nullability warning
- PascalCase for function names and memebers, camelCase for function arguments and local function variables (this is the csharpier standard it will tell you to name them like this)
- Consider [LINQ](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) instead of foreach for iteration over lists
- Use an _ for parameters that your function doesn't actually use but requires you to pass in.
## Godot specific C# style stuff
- Compile time struct/object constants aren't a thing in C#, use the ``readonly`` keyword
- Name scene files the same as their script files
- Put all the node path referencing at the top of the ``Ready()`` function before anything else
- If a reference to the node isn't necessary to keep (connecting to a parent once to attach a signal) then don't assign the reference to a variable.
- If a parent scene is instancing a child scene, attach the signal within the parents scene creation function rather than the childs ready() function
- Connect signals and configure parameters under the nodes references
- Use [with expression](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression) to update struct members on Godot nodes
	```
	Position = Position with { X = 100.0f };
	```
- Going up towards parents for read-only referencing values is fine as long as the child is always guaranteed to exist within it's parent
	- For modifying values in parents we *should* set up signals. 
- Make node references as specific as they can be. If you're dealing with a list of Polymorphic data types then cast the list to their highest parent (or implement an interface or something)
- If a signal only has one line of a thing (calling queue_free or something) then you should make that function anonymous instead of passing in a function pointer
```
//example
HomeRoomArea.AreaEntered += (_) => InHomeRoom = false;
```
- Never attach signals, add a node to groups, or set collision layers through the editor. We want to be able to reason about as much as we can through just the code as possible.
# Scenes files
- scenes can be arbitrarily nested, and instantiated within each other. I generally separate Scene files into 3 different types:
	- Toplevel Scenes: the "levels" or "screens" of your game, they act as the root of the scene tree and aren't instanced into other scenes.
	- Entities: Instanced within top-level scenes. 
	- UI: The ui, try to keep ui nodes children of other ui nodes. Consider placing ui elements in a separate canvas layer
## Other scene file organization 
- Try to only have one script per entity
- Store a static reference to a packed scene within that scene's script
- Try to only have one Animation Player per entity
- if you're doing some wacky stuff in your AnimationPlayer Animation, consider making it a function that uses a tween (readability)
- Don't use Timer nodes (idk why they exist)
- Don't use AnimatedSprite nodes (also don't know why they exist)
- Include nested scenes for things that are shared across different parent scenes, if something will only exist within one scene then it doesn't need to be a scene itself. 
	- with how annoying merging changes on main.tscn was last project I might concede on this point... it's hard to make a generalization about it though.
- If you use more than one instance of an entity turn it into a scene.
- Save custom resources externally to the scene that uses it.
- Built in resources (primitive collision shapes, curves) can stay embedded if they don't change. Make sure to set any duplicates to unique though.
- Configure members of a node through it's ready() function. Only configure things within the editor if we can reason about them visually (position, scale, rotation).
	- things like groups, signals, and groups should be set up in the ready function
# File organization
- here's a sample file system for our project :)
```
project
	src
		entities
			player
				player.tscn
				Player.cs
		resources 
			entitydata.tres
		toplevel_scenes
			main.tscn
			titlescreen.tscn
		ui
			hud.tscn
	assets
		2d
			sprites
			ui
		3d
			model.glb
		audio
			sfx
				sound.wav
			bgm
				song.ogg
```
# git
- main is write protected, we'll use pull requests to upload things to there
- [squash commits](https://stackoverflow.com/questions/5189560/how-do-i-squash-my-last-n-commits-together)
## dealing with main.tscn (or any other pesky scene file) merge conflicts
- Make copies of the conflicted file
	-  ``git show <branch1>:src/toplevel_scenes/main.tscn > src/toplevel_scenes/main_branch1.tscn``
	-  ``git show <branch2>:src/toplevel_scenes/main.tscn > src/toplevel_scenes/main_branch2.tscn``
- Try to fix the merge conflicts on main
- Try running the merged main, if it's broken, copy and paste the changes manually using the two versions from the parent branches