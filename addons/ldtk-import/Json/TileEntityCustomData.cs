#if TOOLS

#pragma warning disable CS8618, IDE0044

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Picalines.Godot.LDtkImport.Json
{
    internal sealed class TileEntityCustomData
    {
        private TileEntityCustomData() { }

        [JsonProperty(LDtkConstants.SpecialFieldNames.TileEntityName)]
        public string EntityName { get; private set; }

        [JsonProperty(LDtkConstants.SpecialFieldNames.KeepTileSprite)]
        public bool? KeepTileSprite { get; private set; }

        public IReadOnlyDictionary<string, object?> EntityFields { get; private set; }

        [JsonExtensionData]
        private JObject _RawEntityFields = null!;

        [OnDeserialized]
        private void Init(StreamingContext context)
        {
            EntityFields = new Dictionary<string, object?>();

            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Populate(_RawEntityFields.CreateReader(), EntityFields);
        }
    }
}

#endif