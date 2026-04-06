using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInputs.Data
{
    public static class InputUtility
    {
        // A Burst-compatible dictionary to map Hashes back to Strings for Debugging
        public static readonly SharedStatic<UnsafeHashMap<int, FixedString32Bytes>> DebugNames = 
            SharedStatic<UnsafeHashMap<int, FixedString32Bytes>>.GetOrCreate<DebugNameRegistryKey>();

        private struct DebugNameRegistryKey {}

        public static int GetActionID(string actionName)
        {
            var fixedName = new FixedString32Bytes(actionName);
            int hash = fixedName.GetHashCode();
            
            // Cache the name for Quill Debugging
            if (DebugNames.Data.IsCreated)
            {
                if (!DebugNames.Data.ContainsKey(hash))
                    DebugNames.Data.Add(hash, fixedName);
            }

            return hash;
        }
    }

}