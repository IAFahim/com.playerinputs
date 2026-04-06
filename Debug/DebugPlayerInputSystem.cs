using BovineLabs.Core;
using BovineLabs.Quill;
using PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace PlayerInputs.Debug
{
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
    public partial struct DebugPlayerInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        { 
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var drawer = SystemAPI.GetSingleton<DrawSystem.Singleton>().CreateDrawer();
            

            state.Dependency = new DebugDrawInputJob
            {
                Drawer = drawer,
                ActionNames = InputUtility.DebugNames.Data
            }.Schedule(state.Dependency); // Schedule single for Quill Text
        }

        [BurstCompile]
        private partial struct DebugDrawInputJob : IJobEntity
        {
            public Drawer Drawer;
            [ReadOnly] public UnsafeHashMap<int, FixedString32Bytes> ActionNames;

            private void Execute(
                in ECSPlayerInputID id,
                in DynamicBuffer<InputAxisBuffer> axes,
                in DynamicBuffer<InputButtonDownBuffer> downs,
                in DynamicBuffer<InputButtonHeldBuffer> helds,
                in DynamicBuffer<InputButtonUpBuffer> ups)
            {
                // Skip drawing if absolutely nothing is happening to keep the screen clean
                if (axes.Length == 0 && downs.Length == 0 && helds.Length == 0 && ups.Length == 0) return;

                float3 headPos = new float3(0, 2.2f, 0);

                // Draw an anchor line from the player to the UI block
                Drawer.Line(new float3(0, 0, 0), headPos, new Color(1f, 1f, 1f, 0.2f));

                // We use a 512 byte string to build a multi-line formatted block
                FixedString512Bytes text = new FixedString512Bytes();
                
                // HEADER
                text.Append("[Player ");
                text.Append(id.ID);
                text.Append("]\n");

                // AXES (Cyan)
                if (axes.Length > 0)
                {
                    text.Append("Axes: ");
                    for (int i = 0; i < axes.Length; i++)
                    {
                        text.Append(GetName(axes[i].ActionID));
                        text.Append("(");
                        // Format float2 to 1 decimal place for compactness
                        text.Append(((int)(axes[i].Value.x * 10)) / 10f);
                        text.Append(", ");
                        text.Append(((int)(axes[i].Value.y * 10)) / 10f);
                        text.Append(") ");
                    }
                    text.Append("\n");
                }

                // DOWNS (Green)
                if (downs.Length > 0)
                {
                    text.Append("Down: ");
                    for (int i = 0; i < downs.Length; i++) { text.Append(GetName(downs[i].ActionID)); text.Append(" "); }
                    text.Append("\n");
                }

                // HELDS (Yellow)
                if (helds.Length > 0)
                {
                    text.Append("Held: ");
                    for (int i = 0; i < helds.Length; i++) { text.Append(GetName(helds[i].ActionID)); text.Append(" "); }
                    text.Append("\n");
                }

                // UPS (Red)
                if (ups.Length > 0)
                {
                    text.Append("Up: ");
                    for (int i = 0; i < ups.Length; i++) { text.Append(GetName(ups[i].ActionID)); text.Append(" "); }
                    text.Append("\n");
                }

                // Draw the final block
                Drawer.Text512(headPos, text, Color.white, 14f);
            }

            // Helper to get the string, or fallback to Hex ID if missing
            private FixedString32Bytes GetName(int actionID)
            {
                if (ActionNames.IsCreated && ActionNames.TryGetValue(actionID, out var name))
                {
                    return name;
                }
                return new FixedString32Bytes("Unknown");
            }
        }
    }
}