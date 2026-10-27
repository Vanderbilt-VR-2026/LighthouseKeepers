# Adding a watch day
1. In Unity, open Assets/Data/Days and create Lighthouse Keepers > Sprint 3 > Day (or duplicate Day03).
2. Name the asset Day05.asset and set Day to 5; numbers must be unique and consecutive.
3. Set Day Length Seconds to the desired deadline (at least 30 seconds).
4. Add task entries with a station ID and the exact text to show on the House board.
5. Use STN_PIPE_LEAK, STN_GENERATOR and STN_RADIO_CONSOLE for pipe, generator and broadcast.
6. Add the faults that start active to Starting Faults using those same station IDs.
7. A generator fault starts power off; absent faults are already complete; task order is free.
8. Saving/importing the asset refreshes Resources/WatchDays automatically; the build revalidates it.
9. Sleep after all Day 4 tasks to enter Day 5; no code or manual catalog edit is required.
