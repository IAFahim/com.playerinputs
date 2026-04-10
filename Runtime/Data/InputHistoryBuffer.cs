using Unity.Entities;

namespace PlayerInputs.Data
{
    public struct InputHistoryBuffer : IBufferElementData
    {
        public byte ActionID;
    }
}
