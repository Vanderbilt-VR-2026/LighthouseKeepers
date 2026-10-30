#!/bin/zsh
# Build first with NetworkingProofBuild.Build, then run from the repository root.
set -eu
root="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$root"
run_dir="$root/Builds/NetworkingProof/run-$(date +%Y%m%d-%H%M%S)"
app="$root/Builds/NetworkingProof/LighthouseNetworkProof.app"
executable_name=$(/usr/libexec/PlistBuddy -c 'Print CFBundleExecutable' "$app/Contents/Info.plist")
player="$app/Contents/MacOS/$executable_name"
room="LKProof-$(date +%s)"
mkdir -p "$run_dir/host" "$run_dir/client"
"$player" -proof-role host -proof-room "$room" -proof-output "$run_dir/host" -proof-quit 1 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$run_dir/host.log" > "$run_dir/host-stdout.log" 2>&1 &
host_pid=$!
trap 'kill "$host_pid" ${client_pid:+$client_pid} 2>/dev/null || true' EXIT
for attempt in {1..60}; do
    if /usr/bin/grep -q PROOF_CONNECTED "$run_dir/host.log" 2>/dev/null; then break; fi
    if ! kill -0 "$host_pid" 2>/dev/null; then print "Host failed; inspect $run_dir/host.log"; exit 1; fi
    sleep 1
done
if ! /usr/bin/grep -q PROOF_CONNECTED "$run_dir/host.log"; then
    print "Host did not connect in time; inspect $run_dir/host.log"; exit 1
fi
"$player" -proof-role client -proof-room "$room" -proof-output "$run_dir/client" -proof-quit 1 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$run_dir/client.log" > "$run_dir/client-stdout.log" 2>&1 &
client_pid=$!
wait "$host_pid"
wait "$client_pid"
trap - EXIT
python3 tools/NetworkingProof/verify.py "$run_dir"
swiftc -module-cache-path /private/tmp/lk-swift-module-cache tools/NetworkingProof/encode.swift -o Builds/NetworkingProof/encode-video
"$root/Builds/NetworkingProof/encode-video" "$run_dir/host" "$run_dir/FirstPerson.mp4"
print "Recording and per-process evidence: $run_dir"
