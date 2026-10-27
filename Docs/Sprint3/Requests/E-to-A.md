# Session E → A: local actor identity and remote hand presentation

E preserves the existing XR rig. `PF_Hand_L` / `PF_Hand_R` are absent in E's base. Publish their prefab paths/GUIDs and a transport-free pose application API taking the agreed `HandPose`, grip and trigger values. E will reference the prefabs without modifying their contents; remote hand assets must contain visual components only, or provide an explicit visual-only prefab.

Both local controllers/interactors must resolve the same authenticated ActorId from E's `ActorIdentity` ancestor (or call E's local actor provider at composition time). The raw controller index is not an ActorId. Keep Fusion out of Interaction; the LK.Net XRI adapter performs selection rejection and host-controlled leases. A release/rejected grab must clear the hand's animation attachment. Please expose completion through the agreed InteractionEvents, with the assigned ActorId.
