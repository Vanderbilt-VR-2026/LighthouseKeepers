# Owen and walkie objective locations

When Owen was doing a job, “Stay with Owen” had no location, and generic floor selection would have defaulted to F4. The local objective and path cues now follow Owen's current floor, with the House handled separately. The target follows his actual actor. A portable “use your walkie” instruction shows “Your walkie” without pointing to a fixed floor. No synchronized fields changed; protocol remains `LK-S5-BODYBELT1 / LK-Lab-Coop-v57`.

In a muted staged Play Mode scene, the actual objective text and target followed Owen through the House and Floors 1–4, and the walkie label had no fictitious floor: **11/11 checks passed**. The fixture moves a simulated Owen to each floor; it does not prove his navigation, a full solo night or headset readability. [Checks](checks.txt) · [fixture](fixtures/GuideCheck.cs).

Focused objective/voice tests passed after the final pole-hint guard (**41 checks**). Just before that one-line guard, a muted full EditMode run passed **711/716** with no new failing cases compared with the earlier 708/715 run: [comparison](suite-comparison.json). The two objective tests were updated because current guidance can use up to 10 short words/48 characters and some steps are observation or portable-tool instructions, not button presses; the previously missing Owen and walkie locations were implemented rather than omitted from the checks. Five unrelated baseline failures remain. A new full run after the guard has not been claimed.

Quest APK `c47cd351` includes this change and was installed without launching. Its local and device SHA-256 match: `b19f61f2327c3d343a3207f71ac0dfb8353f617368ab4589ce232baa6468a24a`. ARM64, the normal package ID, VR category, signature and archive integrity were checked. This is installation evidence, not a headset playtest.

Reproduce with a muted Unity 6000.3.23f1 Editor and Pipeline bridge: `python3 Docs/Verification/S5/CompanionGuidance/fixtures/run.py "$PWD/artifacts/s5/companion-guide/repro"` (use a fresh output directory).
