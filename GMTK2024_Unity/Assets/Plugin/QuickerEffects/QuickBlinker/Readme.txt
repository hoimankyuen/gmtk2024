==== Quick Blinker ===

How to use:
1. Add the following namespace
	
	using QuickerEffects;

2. Either add "Blinker.cs" to the desired game object, or by script as followings:

	var blinker = AddComponent<Blinker>();
	blinker.Color = Color.green;
	blinker.Speed = 1.5f;
	blinker.CyclePeriod = 2f;

Parameters:
	BlinkerSpeed: The moving speed of the blinker. Mesured in the whole total height range per second.
	BlinkerCyclePeriod: The time difference between two consective blinker flash. Mesure in seconds. Should be at least 1 / blinkerSpeed long.

Versions:
1.0: First release
1.1: Added shader level enabled / disable
1.2: Make value set do not update when recieving the same value, make parameters more consistant with others
1.3: Streamline the setup process, remove subtle paramters and provide a standard generated one instead.
1.4: Added direction option for the positive and negative of X,Y,Z axis.