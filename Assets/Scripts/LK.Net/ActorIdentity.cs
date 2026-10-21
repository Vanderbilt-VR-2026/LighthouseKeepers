using UnityEngine;
namespace LighthouseKeepers.Net
{
    /// <summary>One component on the local rig root; both hands resolve the same session actor.</summary>
    [DisallowMultipleComponent]
    public sealed class ActorIdentity : MonoBehaviour
    {
        public int ActorId { get; private set; }
        public void Bind(int authenticatedActor) => ActorId = authenticatedActor > 0 ? authenticatedActor : 0;
        public static int Of(Component interactor)
        {
            var owner = interactor ? interactor.GetComponentInParent<ActorIdentity>() : null;
            return owner ? owner.ActorId : 0;
        }
    }
}
