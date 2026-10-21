# C to E — production adoption and player renderers

C changed only the owned Assets/Scenes/Lab/LK_Lab_Environment.unity and environment assets. Sprint3EnvironmentAuthoring.BuildAndroid is an explicit two-scene environment assessment build (existing copied lobby + Lab), not a network/headset acceptance claim. Production StormWatch loads different scenes via owner code; E must coordinate adoption with D. Do not silently switch production loading to this local legacy rehearsal.

**Ship gate dependency:** the shared LK_QuestPlayer prefab contains built-in Capsule/Sphere/Cube render meshes. C's scene build gate rejects them even if hidden; please replace E-owned body/rig visuals with authored meshes and coordinate hand visuals with A. Do not disable the validator to hide a failure.

Add production shipping environment scenes to Assets/Scenes/Sprint3/ShipScenes.json / ShipScene marker after adoption. R1 prevented C from modifying the legacy production scenes under Assets/_LighthouseKeepers/Scenes. The one-line default sun fix is already in sprint3/base (m_Sun=0, no Storm daylight object).
