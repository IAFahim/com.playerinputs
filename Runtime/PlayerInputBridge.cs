using System.Collections.Generic;
using PlayerInputs.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace PlayerInputs
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        [Tooltip("Overrides the player index from PlayerInput. Leave at -1 to auto-read.")]
        public int playerIdOverride = -1;

        internal readonly List<(byte Id, InputAction Action)> Buttons = new();
        internal readonly List<(byte Id, InputAction Action)> Axes = new();

        private EntityManager _entityManager;
        private Entity _providerEntity;

        private void OnEnable()
        {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput.actions == null) return;

            var inputKeys = InputKeys.I;
            if (inputKeys != null)
            {
                foreach (var mapping in inputKeys.Mappings)
                {
                    if (mapping.Action == null) continue;

                    var action = playerInput.actions.FindAction(mapping.Action.action.id);
                    if (action == null) continue;

                    switch (action.type)
                    {
                        case InputActionType.Button:
                            Buttons.Add((mapping.Value, action));
                            break;
                        case InputActionType.Value:
                            Axes.Add((mapping.Value, action));
                            break;
                    }
                }
            }

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _providerEntity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(_providerEntity, new PlayerId { Value = GetPlayerId() });
            _entityManager.AddComponent<InputProviderTag>(_providerEntity);

            _entityManager.AddComponentObject(_providerEntity, new PlayerInputBridgeComponent { Value = this });

            _entityManager.AddBuffer<InputButtonDownBuffer>(_providerEntity);
            _entityManager.AddBuffer<InputButtonHeldBuffer>(_providerEntity);
            _entityManager.AddBuffer<InputButtonUpBuffer>(_providerEntity);
            _entityManager.AddBuffer<InputAxisBuffer>(_providerEntity);
        }

        private void OnDisable()
        {
            if (_entityManager != default && _entityManager.Exists(_providerEntity))
            {
                _entityManager.DestroyEntity(_providerEntity);
            }
            
            Buttons.Clear();
            Axes.Clear();
        }

        public byte GetPlayerId()
        {
            if (playerIdOverride >= 0) return (byte)playerIdOverride;
            var pi = GetComponent<PlayerInput>();
            return (byte)(pi != null ? pi.playerIndex : 0);
        }
    }

    public class PlayerInputBridgeComponent : IComponentData
    {
        public PlayerInputBridge Value;
    }
}