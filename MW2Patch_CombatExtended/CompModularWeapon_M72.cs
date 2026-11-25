using System.Collections.Generic;
using System.Linq;
using ModularWeapons2;
using Verse;

namespace MW2Patch_CombatExtended {
    public class CompModularWeapon_M72 : CompModularWeapon {
        public override void PostExposeData() {
            base.PostExposeData();
            if (Scribe.mode != LoadSaveMode.LoadingVars) return;

            var sight = PartsMounts.FirstOrDefault(mount => mount.mountDef.defName == "MountSight_Mid");
            if (sight == null) return;

            MigrateLongSight(attachHelpers, sight);
            MigrateLongSight(attachHelpers_buffer, sight);
        }

        private static void MigrateLongSight(List<PartsAttachHelper> helpers, MountAdapterClass sight) {
            if (helpers == null) return;

            // MW2 saves the root mount ID even for optics fitted through an adapter.
            bool hasCurrentSight = helpers.Any(helper => helper.attachMountDef == sight.mountDef);
            for (int i = helpers.Count - 1; i >= 0; i--) {
                var helper = helpers[i];
                if (helper.attachMountDef?.defName != "MountSight_Long") continue;

                if (!hasCurrentSight && sight.GetAttatchableParts().Contains(helper.partsDef)) {
                    helpers[i] = sight.GenerateHelper(helper.partsDef);
                    hasCurrentSight = true;
                } else {
                    helpers.RemoveAt(i);
                }
            }
        }
    }
}
