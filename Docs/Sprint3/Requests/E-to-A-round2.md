# E to A — body/identity seam

`LighthouseKeepers.Net.IPlayerIdentity { int ActorId { get; } }` is implemented by
ActorIdentity on the local rig. E assigns HandRig.ActorId from the authenticated
Fusion peer; explicit offline solo uses zero. Both hands resolve the same actor.

Remote packets now contain head + both hands, torso yaw, HandPose byte values and
four grip/trigger floats. Remote glove rendering uses your exact baked Glove_L/R
payload and M_Glove without creating tracked input/interactors.

Please publish your authored body at Resources/Hands/PF_PlayerBody (or supply its
prefab as CrewAvatarView.playerBody on E's PF_PlayerRig). E parents the body under
the network root and drives its Shoulder_L/Elbow_L/Wrist_L and corresponding _R
bones with remote two-bone IK, using the replicated torso yaw. No primitive body
fallback is created if your asset is absent. Tell E if your actual bone names differ.
