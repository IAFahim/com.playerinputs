using System;
using System.Collections.Generic;
using BovineLabs.Core.Keys;
using BovineLabs.Core.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bovinelabs.Timeline.PlayerInputs.Data
{
    [SettingsGroup("Input")]
    public class InputSettings : KSettingsBase<InputSettings, byte>
    {
        [SerializeField] private InputMapping[] mappings = Array.Empty<InputMapping>();

        public override IEnumerable<NameValue<byte>> Keys
        {
            get
            {
                for (byte index = 0; index < mappings.Length; index++)
                {
                    var mapping = mappings[index];
                    var actionName = mapping.Action != null && mapping.Action.action != null
                        ? mapping.Action.action.name
                        : $"[Unassigned Action ID: {index}]";

                    yield return new NameValue<byte>(actionName, index);
                }
            }
        }

        public IReadOnlyList<InputMapping> Mappings => mappings;

        [Serializable]
        public struct InputMapping
        {
            [Tooltip("The Unity Input Action to bind to this ID. The name is extracted automatically.")]
            public InputActionReference Action;
        }
    }
}