# Two-Mac regression

Final room P7RVZ3: both Editors joined/readied, loaded the Lab, saw the other avatar (own hidden, upright body, two hands, no remote cameras/listeners), guest completed three actual XRI pipe repairs, both boards agreed, both slept into Night 2, guest returned to the lobby and rejoined the active Night 2. Screenshots from both views were inspected.

The first attempt in BHPS78 failed at the synthetic guest grab. The test now supplies manual grip value=1 as well as the performed flag, and waits 0.6 seconds after the host-replicated hand pose arrives. The rerun succeeds, including drop/regrab during repair. No production grab/network rule was changed. Both original and final raw runs remain in artifacts/s4-master/part-c/verified/.

Voice facts in the first matching-build session show mono 48 kHz remote playback in both directions, active AEC/noise suppression, processor and output reference. The final room repeats the mono/DSP checks; the guest's first remote-voice query ran before its remote speaker appeared. These are transport/component checks, not intelligibility or echo listening acceptance. Microphones were muted after the checks to avoid same-room feedback. Mac+Quest and human listening remain pending.

The harness retains historical `host` screenshot/step labels inside guest-pipe; the invoking process was the guest worktree. Events and both board snapshots identify the guest action correctly.
