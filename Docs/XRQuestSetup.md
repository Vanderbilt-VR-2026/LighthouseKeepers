# XR / Quest setup

Unity 6000.3.23f1; URP 17.3.0, Input System 1.20.0, XR Interaction Toolkit 3.6.0, XR Management 4.7.0, OpenXR 1.18.0, Core Utils 2.6.0. The official XRI Starter Assets are imported from the installed registry package, not copied from Spear Throw VR. UGUI 2.0.0 provides TMP; TMP essentials were imported. Unity Test Framework 1.6.0 matches the team’s editor baseline. No XR Hands dependency was added.

Android: ARM64, IL2CPP, minimum API32, automatic target API, linear colour, Vulkan then GLES3, landscape, Input System only. OpenXR initializes on startup with MetaQuestFeature, Touch Plus and Oculus Touch profiles; deprecated OculusQuestFeature is disabled. Single-pass instanced stereo. Quest URP uses 4× MSAA, render scale1, HDR off, 20m shadow distance, one shadowed directional light and unshadowed practical lights. Two per-pixel additional lights are allowed per object because sparse architecture vertices made vertex-lit practical lighting unreadable. This is a documented departure from the reference mobile renderer.

The rig is authored from the package rig with floor tracking, a 0.05m near plane, character collision, gravity and head-relative continuous movement. Left movement and right turning are explicitly separated. Starter near/far interactors provide close grabbing and distance rays, including package haptic hooks. Floating glove proxies and a smoothed torso add lightweight embodiment; no arms or IK. XRI's stereo tunnelling prefab is driven by measured rig motion and optional smooth turn speed. Snap is the default.

ComfortSettings controls speed, turn mode/angle/speed, vignette strength and fade. The DynamicMoveProvider exposes controller-relative movement if wanted. Recenter remains supported by the Quest system; floor-relative tracking should be reset in a clear safe space.

The setup tool is repeatable and only modifies Lighthouse. An OpenXR 1.18 validation predicate incorrectly queries the selected platform while validating Android; rule fixing is therefore deferred until Android is active. Do not run a second Unity instance against this directory. Never open the reference project for editing as part of Lighthouse setup.

The unused preinstalled AI inference 2.6.1 package was removed after the first build produced hundreds of Sentis shader warnings. AI Assistant 2.19.0-pre.2 was removed during initial setup. The template Multiplayer Center editor hub remains; it is not a multiplayer runtime framework. No networking or voice package was added.

The repaired rig uses left smooth-motion/right turn action ownership, and Bootstrap gates motion until geometry loads. The original block proxies are replaced by rounded glove meshes. Baked navigation fills and cold flat ambient lighting with environment reflections disabled replace the first build’s red/pink ambient appearance. Headset acceptance is recorded separately.
