using Unity.Entities;

namespace Bovinelabs.Timeline.PlayerInputs.Data
{
    public struct InputCancelWindow : IComponentData, IEnableableComponent
    {
        public InputBitmask AllowedMask;
    }
}