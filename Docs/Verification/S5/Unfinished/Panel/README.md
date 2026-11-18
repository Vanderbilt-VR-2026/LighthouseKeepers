# Helper power-budget correction

The final review found that a Machinist helper attempted to enable all four branches on the 2.5-unit Nights 4–5 grid. It now plans a feasible selection around the current job, sheds unwanted load first, enables the needed branch and then resets the breaker. Human use of panel levers yields control. No new replicated fields or protocol bump.

Nine focused helper/order checks pass. Actual muted Play fixture starts with an overloaded 2.5-unit panel; Owen travels to F2, operates the switches/reset, then keeps a stable 2.25-unit selection for five seconds. An initial live check exposed an unnecessary post-reset change caused by inconsistent priority; the shared pre/post-reset choice fixes it. `live-scene.json` records the successful result. This deliberately constrained live Night 3 scene exercises the same 2.5-unit budget used by Nights 4–5; it is not a full Night 4/5 run or worn usability measurement.

Reproduce with a compiled muted Editor: `python3 artifacts/s5/unfinished/check.py CrewCompanionTests CampaignOrderTests`; `python3 artifacts/s5/unfinished/panel-stable.py`, changing the output folder before re-running.
