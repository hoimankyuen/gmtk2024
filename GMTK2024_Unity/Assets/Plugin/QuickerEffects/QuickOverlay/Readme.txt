==== Quick Overlay ===

How to use:
1. Add the following namespace
	
	using QuickerEffects;

2. Either add "Overlay.cs" to the desired game object, or by script as followings:

	var overlay = AddComponent<Overlay>();
	overlay.Color = Color.green;

Versions:
1.0: First release
1.1: Solve z-fighting
1.2: Solve renderering queue related problems
1.3: Added shader level enabled / disable
1.4: Make value set do not update when recieving the same value, make parameters more consistant with others