# Session A → D: physical-verb sequencing and reset

Status: [PROPOSED] integration; no headset result.

Reference assembly LK.Gameplay.Interaction. Exact shared events are present. Additional verb events are Action<int> OnPrimerStrokeCompleted, Action<int,bool> OnChokeChanged (bool=true means closed), and Action<int> OnGeneratorStartFailed. These additions do not replace the three agreed shared events. They carry local actor intent, not authoritative state.

GeneratorRestart physically gates OnGeneratorStartCompleted on >=3 accepted primer down-strokes and a CLOSED choke. It consumes those three strokes on success. An unprimed/wrong-choke pull plays failed_crank and emits OnGeneratorStartFailed; the player can prime/change choke and retry. D decides whether a valid event advances a day. D/E must communicate primer/choke state if those verbs are split between clients; A's component itself is local.

On BeginDay/ResetRun/retry, explicitly call GeneratorRestart.ResetInteraction(), each manual ConstrainedArc.ResetInteraction(), and WrenchInteraction.ResetRepair(). Do not simulate these by moving the camera. A's components emit no power/water writes. Release cancels partial pump/primer strokes. Choke snaps to its selected detent on release. The wrench accumulates signed tightening at the repair point; reverse turns undo unfinished progress. It emits one repair event until reset.

The existing Sprint 2 WatchControl / WatchWrench path remains in its owner's files. Switch the Sprint 3 scenes/director to the new events after merge so two systems cannot both submit repair/pump work. Completing this wiring is a dependency for integrated acceptance.
