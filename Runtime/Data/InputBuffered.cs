using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInputs.Data
{

    [InternalBufferCapacity(8)]
    public struct InputButtonDownBuffer : IBufferElementData
    {
        public int ActionID;
    }

    [InternalBufferCapacity(8)]
    public struct InputButtonHeldBuffer : IBufferElementData
    {
        public int ActionID;
    }

    [InternalBufferCapacity(8)]
    public struct InputButtonUpBuffer : IBufferElementData
    {
        public int ActionID;
    }

    [InternalBufferCapacity(4)]
    public struct InputAxisBuffer : IBufferElementData
    {
        public int ActionID;
        public float2 Value;
    }
}