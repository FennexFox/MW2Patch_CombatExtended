using Verse;

namespace MW2Patch_CombatExtended
{
    public class CompM72StackCompatibility : ThingComp
    {
        public override bool AllowStackWith(Thing other) => false;

        public override void PostExposeData()
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit && parent.stackCount > parent.def.stackLimit)
            {
                // Map loading truncates oversized stacks. Keep the original launcher and its saved parts.
                Thing surplus = parent.SplitOff(parent.stackCount - parent.def.stackLimit);
                LongEventHandler.ExecuteWhenFinished(() => DistributeLaunchers(surplus));
            }
        }

        private void DistributeLaunchers(Thing surplus)
        {
            Map map = parent.MapHeld;
            IntVec3 position = parent.PositionHeld;
            int count = surplus.stackCount;
            for (int i = 0; i < count; i++)
            {
                Thing launcher = surplus.SplitOff(1);
                if (TryStoreLauncher(launcher))
                    continue;

                if (map != null && position.InBounds(map))
                {
                    if (!GenPlace.TryPlaceThing(launcher, position, map, ThingPlaceMode.Near))
                    {
                        // The source cell already held the stack. Preserve the launcher when the nearby cells are full.
                        launcher.Position = position;
                        launcher.SpawnSetup(map, respawningAfterLoad: false);
                    }
                    continue;
                }

                parent.stackCount += count - i;
                Log.Warning("[MW2Patch_CE] Could not separate all launchers in a loaded stack at " + parent.ParentHolder);
                return;
            }
        }

        private bool TryStoreLauncher(Thing launcher)
        {
            for (IThingHolder holder = parent.ParentHolder; holder != null && !(holder is Map); holder = holder.ParentHolder)
            {
                ThingOwner destination = holder switch
                {
                    Pawn_EquipmentTracker equipment => equipment.pawn.inventory?.innerContainer,
                    Pawn_CarryTracker carry => carry.pawn.inventory?.innerContainer,
                    _ => holder.GetDirectlyHeldThings()
                };
                if (destination?.TryAdd(launcher, canMergeWithExistingStacks: false) == true)
                    return true;
            }
            return false;
        }
    }
}
