using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using LighthouseKeepers.Core;
using LighthouseKeepers.Player;
using LighthouseKeepers.Flood;
using LighthouseKeepers.Audio;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace LighthouseKeepers.Tests
{
    public sealed class PlayerTraversalTests
    {
        XROrigin rig;
        ContinuousMoveProvider move;
        GravityProvider gravity;
        CharacterController character;
        XRInputValueReader<Vector2> input;
        float oldCaptureDelta;

        [UnitySetUp]
        public IEnumerator LoadEnvironment()
        {
            oldCaptureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 72f;
            yield return SceneManager.LoadSceneAsync("LK_Bootstrap", LoadSceneMode.Single);
            var deadline = Time.realtimeSinceStartup + 60;
            while (!Object.FindAnyObjectByType<EnvironmentBootstrap>().Ready && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue(Object.FindAnyObjectByType<EnvironmentBootstrap>().Ready);
            Object.FindAnyObjectByType<FloodController>().Paused = true;
            rig = Object.FindAnyObjectByType<XROrigin>();
            character = rig.GetComponent<CharacterController>();
            move = rig.GetComponentInChildren<ContinuousMoveProvider>();
            gravity = rig.GetComponentInChildren<GravityProvider>();
            // Let desktop preview establish its standing camera pose before taking control of input.
            yield return null;
            rig.GetComponent<DesktopPreview>().enabled = false;
            input = new XRInputValueReader<Vector2>("Traversal test", XRInputValueReader.InputSourceMode.ManualValue);
            move.leftHandMoveInput = input;
            for (int i = 0; i < 72; i++) yield return null;
        }

        [TearDown]
        public void RestoreClock()
        {
            if (input != null) input.manualValue = Vector2.zero;
            Time.captureDeltaTime = oldCaptureDelta;
        }

        [UnityTest]
        public IEnumerator StandingOnEnvironmentIsGroundedAndReleasingStickStops()
        {
            Assert.IsTrue(gravity.isGrounded, "Gravity must recognize the Environment floor, not accelerate forever against it.");
            yield return WalkTo(new Vector3(-6, 0, 0));
            input.manualValue = Vector2.zero;
            yield return null;
            var stopped = rig.transform.position;
            for (int i = 0; i < 36; i++) yield return null;
            Assert.That(Vector3.Distance(stopped, rig.transform.position), Is.LessThan(.02f), "Player drifts after releasing the stick.");
        }

        [UnityTest]
        public IEnumerator FootstepsFollowTravelAndStaySilentAtRestAgainstWallsAndInAir()
        {
            var footsteps = rig.GetComponent<PlayerFootsteps>();
            Assert.IsNotNull(footsteps, "Bootstrap must inherit footsteps from the player prefab.");
            Assert.AreEqual(0, footsteps.StepsPlayed);
            var start = rig.transform.position;
            yield return WalkTo(start + Vector3.right * 2);
            // Coroutines resume before LateUpdate; let the last moving frame's audio finish sampling.
            yield return null;
            Assert.That(footsteps.StepsPlayed, Is.InRange(2, 3), "Two metres should produce approximately three boot impacts.");
            int count = footsteps.StepsPlayed;
            for (int i = 0; i < 72; i++) yield return null;
            Assert.AreEqual(count, footsteps.StepsPlayed, "Idle footsteps.");

            Place(new Vector3(-10, .04f, 0));
            SetDirection(Vector3.left);
            for (int i = 0; i < 144; i++) yield return null;
            count = footsteps.StepsPlayed;
            var blocked = rig.transform.position;
            for (int i = 0; i < 144; i++) yield return null;
            Assert.That(Vector3.Distance(blocked, rig.transform.position), Is.LessThan(.02f));
            Assert.AreEqual(count, footsteps.StepsPlayed, "Pushing against a wall must stay silent.");

            Place(new Vector3(-7, 2, 0));
            SetDirection(Vector3.right);
            for (int i = 0; i < 20; i++) yield return null;
            Assert.IsFalse(gravity.isGrounded);
            Assert.AreEqual(count, footsteps.StepsPlayed, "Teleport/fall must not play a step.");
            input.manualValue = Vector2.zero;
        }

        [UnityTest]
        public IEnumerator SnapTurnWithOffsetHeadDoesNotPlayFootsteps()
        {
            var footsteps = rig.GetComponent<PlayerFootsteps>();
            rig.Camera.transform.localPosition += Vector3.right * .6f;
            for (int i = 0; i < 20; i++) yield return null;
            int count = footsteps.StepsPlayed;
            var originalRotation = rig.transform.rotation;
            var snap = rig.GetComponentInChildren<SnapTurnProvider>();
            var turn = new XRInputValueReader<Vector2>("Footstep turn test", XRInputValueReader.InputSourceMode.ManualValue);
            snap.rightHandTurnInput = turn;
            turn.manualValue = Vector2.right;
            for (int i = 0; i < 40; i++) yield return null;
            turn.manualValue = Vector2.zero;
            Assert.That(Quaternion.Angle(originalRotation, rig.transform.rotation), Is.GreaterThan(30));
            Assert.AreEqual(count, footsteps.StepsPlayed);
        }

        [UnityTest]
        public IEnumerator WalksThroughHouseAndBalconyDoorwaysInBothDirections()
        {
            yield return WalkTo(new Vector3(-3.3f, 0, 0));
            yield return WalkTo(new Vector3(-8, 0, 0));
            Place(new Vector3(3.5f, 9.64f, 0));
            yield return WalkTo(new Vector3(6, 9.6f, 0));
            Assert.That(rig.transform.position.y, Is.EqualTo(9.6f).Within(.15f));
            yield return WalkTo(new Vector3(3.5f, 9.6f, 0));
        }

        [UnityTest]
        public IEnumerator WalksAllThreeFlightsUpAndDownThroughLandings()
        {
            Place(new Vector3(1.85f, .04f, .15f));
            var footsteps = rig.GetComponent<PlayerFootsteps>();
            for (int floor = 0; floor < 3; floor++)
            {
                int before = footsteps.StepsPlayed;
                for (int degrees = 12; degrees <= 360; degrees += 6)
                {
                    yield return WalkTo(Polar(1.85f, degrees, floor * 3.2f + degrees / 360f * 3.2f));
                    Assert.That(rig.transform.position.y, Is.EqualTo(floor * 3.2f + degrees / 360f * 3.2f).Within(.18f));
                }
                Assert.That(footsteps.StepsPlayed - before, Is.GreaterThanOrEqualTo(12), $"Footstep cadence interrupted ascending flight {floor + 1}.");
                yield return WalkTo(new Vector3(3.15f, (floor + 1) * 3.2f, .15f));
                Assert.That(rig.transform.position.y, Is.EqualTo((floor + 1) * 3.2f).Within(.15f));
                yield return WalkTo(new Vector3(1.85f, (floor + 1) * 3.2f, .15f));
            }
            for (int floor = 2; floor >= 0; floor--)
            {
                int before = footsteps.StepsPlayed;
                for (int degrees = 354; degrees >= 0; degrees -= 6)
                {
                    yield return WalkTo(Polar(1.85f, degrees, floor * 3.2f + degrees / 360f * 3.2f));
                    Assert.That(rig.transform.position.y, Is.EqualTo(floor * 3.2f + degrees / 360f * 3.2f).Within(.18f));
                }
                Assert.That(footsteps.StepsPlayed - before, Is.GreaterThanOrEqualTo(12), $"Footstep cadence interrupted descending flight {floor + 1}.");
                yield return WalkTo(new Vector3(3.15f, floor * 3.2f, .15f));
                Assert.That(rig.transform.position.y, Is.EqualTo(floor * 3.2f).Within(.15f));
                yield return WalkTo(new Vector3(1.85f, floor * 3.2f, .15f));
            }
            Assert.That(rig.GetComponent<PlayerFootsteps>().StepsPlayed, Is.GreaterThan(60), "Footsteps must also work along the stair route.");
        }

        [UnityTest]
        public IEnumerator TowerRouteAlsoWorksAtThirtyFramesPerSecond()
        {
            Time.captureDeltaTime = 1f / 30f;
            yield return WalksAllThreeFlightsUpAndDownThroughLandings();
        }

        void Place(Vector3 position)
        {
            character.enabled = false;
            rig.transform.SetPositionAndRotation(position, Quaternion.identity);
            character.enabled = true;
            gravity.ResetFallForce();
            Physics.SyncTransforms();
        }

        IEnumerator WalkTo(Vector3 target)
        {
            for (int frame = 0; frame < 720; frame++)
            {
                var delta = Vector3.ProjectOnPlane(target - rig.transform.position, Vector3.up);
                if (delta.magnitude < .06f)
                {
                    input.manualValue = Vector2.zero;
                    yield break;
                }
                SetDirection(delta.normalized);
                yield return null;
            }
            Assert.Fail($"Traversal stuck at {rig.transform.position}, target {target}, grounded={gravity.isGrounded}, controllerGrounded={character.isGrounded}");
        }

        void SetDirection(Vector3 direction)
        {
            var forward = Vector3.ProjectOnPlane(rig.Camera.transform.forward, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            input.manualValue = new Vector2(Vector3.Dot(direction, right), Vector3.Dot(direction, forward));
        }

        static Vector3 Polar(float radius, float degrees, float height) =>
            new Vector3(Mathf.Cos(degrees * Mathf.Deg2Rad) * radius, height, Mathf.Sin(degrees * Mathf.Deg2Rad) * radius);
    }
}
