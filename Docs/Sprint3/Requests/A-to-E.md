# Session A → E: local hands and network intent

Status: [PROPOSED] integration; no remote transport/device evidence.

Embed these owned hand prefabs under your tracked controller transforms:
Assets/Prefabs/Hands/Resources/Hands/PF_Hand_L.prefab
Assets/Prefabs/Hands/Resources/Hands/PF_Hand_R.prefab

Both prefabs contain a 26-joint skeleton and HandRig. Awake creates the shared baked skinned mesh and an 0.08 m palm XRDirectInteractor, driven by grip analog via the installed XRI reader. Do not add a second gameplay direct interactor. Existing ray interactors must retain only UI; the compatibility installer adds DirectGameplayFilter to XRInteractionManager. It rejects XRRayInteractor/NearFarInteractor gameplay selection while preserving BaseTeleportationInteractable selection and the separate UI path. It does not clear teleport layers.

Assign HandRig.ActorId to your actual local actor ID on both hands before permitting interactions. Serialized zero is an explicit unassigned local fallback, not a valid multiplayer identity guarantee. Do not animate remote replicas from local XRNode input. Network a visual-only remote representation/pose values through your existing authority path. A's compatibility installer recognizes legacy 'Left Controller' / 'Right Controller' under an XROrigin with an active camera; embed prefabs explicitly if your hierarchy differs. It skips rigs that already have HandRig.

Subscribe/forward local InteractionEvents once; B/D validate on the host. Primer/choke/current-generation state needs forwarding if two keepers service the same generator from different clients. Keep teleport/snap-turn and tracked camera ownership in E. No networking package or transport is added by A.

A persists only the picked-up wrench GameObject using DontDestroyOnLoad. If your scene lifecycle destroys/recreates the whole local inventory or adds network tools, preserve/rebind the same held tool and avoid duplicate local/network wrench instances.
