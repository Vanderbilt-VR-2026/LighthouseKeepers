# Spear Throw VR comparison

Reference: `../First VR Build/Spear Throw VR`, located by searching inside the immediate parent directory. Assets, Packages/manifest.json and ProjectSettings/ProjectVersion.txt are present. Inspected read-only; no assets, scripts, identities, caches or meta files copied.

| Area | Observed reference | Lighthouse decision |
|---|---|---|
| Editor | 6000.5.10f1 (3bd4f66ad299) | Match |
| URP | 17.5.0; Forward mobile renderer | Match version/path; create game-owned pipeline assets |
| Input System | 1.20.0; activeInputHandler 1 | Match |
| XRI | 3.6.0; Starter Assets imported | Install same package; import its own official Starter Assets |
| XR Management | 4.7.0 | Match |
| OpenXR | 1.18.0; Android loader initialized at startup | Match |
| Core Utils | 2.6.0 | Match compatible dependency |
| Controller features | MetaQuestFeature, MetaQuestTouchPlusControllerProfile, OculusTouchControllerProfile | Enable same; deprecated OculusQuestFeature stays disabled |
| Stereo | Android render mode 1, SinglePassInstanced | Match |
| Android | IL2CPP, ARM64, API minimum 32, target automatic | Match |
| Graphics APIs | Vulkan then OpenGLES3 | Match |
| Color | Linear | Match |
| Render quality | Scale 1.0, MSAA 4, shadow distance 25 m | Match baseline; optimize for interiors/storm |
| Rig | Floor origin enum 2; enabled XRI input action asset; CharacterController | Match behavior using newly imported sample rig |
| Locomotion | Left smooth move 1.8 m/s; right 45-degree snap | Start more conservatively indoors; add configurable smooth turning and comfort |
| Inputs | XRI Head, Left/Right interaction and locomotion, UI action maps | Use package-owned Starter Assets action maps |
| Organization | Assets/Scripts, Editor, Scenes, Settings, Samples, XR, XRI | Game content under Assets/_LighthouseKeepers, with familiar Scripts/Editor/Scenes/Settings conventions |
| Scenes | Single CastleRange entry with legacy fallback | New Bootstrap + eight additive content scenes; no reference scene copies |
| Identity | com.Vanderbilt.SpearThrowVR | Independent Lighthouse Keepers product and Android identifier; preserve current cloud identity |
| AI Assistant | Present in reference and initial Lighthouse manifest | Exclude from Lighthouse as explicitly required |
| Networking | Not needed for this sprint | Install no networking or voice framework |

Statuses above are audit findings and implementation decisions; they do not themselves assert that setup/build verification has passed. Final verification is recorded separately.
