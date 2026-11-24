using CombatExtended;
using HarmonyLib;
using ModularWeapons2;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Verse;

namespace MW2Patch_CombatExtended {
    public static class MW2CE_BreakpointActions {
        private static readonly AccessTools.FieldRef<CompFireModes, Verb> CachedFireModeVerb =
            AccessTools.FieldRefAccess<CompFireModes, Verb>("verbInt");

        public static void PostOpenGunsmith(Thing thing) {
            /* What to code here is:
             *   Unload magazine 
             *     then drop ammo on ground or put ammo to inventory
             *   
             *   If using underbarrel, switch to main gun
             *   
             *   DO NOT FORGET make both(B and UB) guns unloaded
             *   
             */
            var compAmmo = thing.TryGetComp<CompAmmoUser>();
            var compUB = thing.TryGetComp<CompUnderBarrel>();
            if (compAmmo != null) {
                if (compUB != null && compUB.Props?.propsUnderBarrel != null) {
                    if (!compUB.usingUnderBarrel) {
                        compUB.SwitchToUB();
                    }
                    compAmmo.TryUnload(true);
                    compUB.SwithToB();
                }
                compAmmo.TryUnload(true);
            }
        }
        public static void RefleshParts(CompModularWeapon comp) {
            if (comp?.parent == null) {
                return;
            }

            var thing = comp.parent;
            var compAmmo = thing.TryGetComp<CompAmmoUser>();
            var compFireModes = thing.TryGetComp<CompFireModes>();
            var compBipod = thing.TryGetComp<BipodComp>();
            var compUnderBarrel = thing.TryGetComp<CompUnderBarrel>();
            var usingSecondary = compUnderBarrel?.usingUnderBarrel == true;

            // MW2 has rebuilt the weapon's verbs, even when only an optic changed.
            if (compFireModes != null) {
                CachedFireModeVerb(compFireModes) = null;
            }

            var baseAmmoProps = thing.def.comps?.FirstOrDefault(x => x is CompProperties_AmmoUser) as CompProperties_AmmoUser;
            var desiredAmmoProps = comp.GetCompProps_AmmoUser();
            var hasAmmoExtensions = desiredAmmoProps != null && !ReferenceEquals(desiredAmmoProps, baseAmmoProps);
            var currentPrimaryAmmoProps = compUnderBarrel?._compPropsAmmo
                ?? (usingSecondary ? baseAmmoProps : compAmmo?.props as CompProperties_AmmoUser);
            var shouldRefreshAmmo = desiredAmmoProps != null
                && (hasAmmoExtensions || !ReferenceEquals(currentPrimaryAmmoProps, baseAmmoProps));

            if (shouldRefreshAmmo) {
                if (compUnderBarrel != null) {
                    compUnderBarrel._compPropsAmmo = desiredAmmoProps;
                }

                if (compAmmo != null) {
                    if (usingSecondary) {
                        if (compUnderBarrel.mainGunLoadedAmmo != null
                            && !AmmoSetContains(desiredAmmoProps, compUnderBarrel.mainGunLoadedAmmo)) {
                            compUnderBarrel.mainGunLoadedAmmo = FirstAmmo(desiredAmmoProps);
                            compUnderBarrel.mainGunMagCount = 0;
                        }
                        compUnderBarrel.mainGunMagCount = Math.Min(
                            compUnderBarrel.mainGunMagCount,
                            Math.Max(0, desiredAmmoProps.magazineSize));
                    }
                    else {
                        var currentAmmo = compAmmo.CurrentAmmo;
                        var selectedAmmo = compAmmo.SelectedAmmo;
                        compAmmo.Initialize(desiredAmmoProps);

                        if (currentAmmo != null && AmmoSetContains(desiredAmmoProps, currentAmmo)) {
                            compAmmo.CurrentAmmo = currentAmmo;
                        }
                        else if (currentAmmo != null) {
                            compAmmo.CurrentAmmo = FirstAmmo(desiredAmmoProps);
                            compAmmo.CurMagCount = 0;
                        }

                        if (selectedAmmo != null && AmmoSetContains(desiredAmmoProps, selectedAmmo)) {
                            compAmmo.SelectedAmmo = selectedAmmo;
                        }
                        else {
                            compAmmo.SelectedAmmo = compAmmo.CurrentAmmo;
                        }

                        compAmmo.CurMagCount = Math.Min(
                            compAmmo.CurMagCount,
                            Math.Max(0, desiredAmmoProps.magazineSize));
                    }
                }
            }

            var baseFireModeProps = thing.def.comps?.FirstOrDefault(x => x is CompProperties_FireModes) as CompProperties_FireModes;
            var desiredFireModeProps = comp.GetCompProps_FireModes();
            var hasFireModeExtensions = desiredFireModeProps != null && !ReferenceEquals(desiredFireModeProps, baseFireModeProps);
            var currentPrimaryFireModeProps = compUnderBarrel?._compPropsFireModes
                ?? (usingSecondary ? baseFireModeProps : compFireModes?.props as CompProperties_FireModes);
            var shouldRefreshFireModes = desiredFireModeProps != null
                && (hasFireModeExtensions || !ReferenceEquals(currentPrimaryFireModeProps, baseFireModeProps));

            if (shouldRefreshFireModes) {
                if (compUnderBarrel != null) {
                    compUnderBarrel._compPropsFireModes = desiredFireModeProps;
                }
                if (compFireModes != null && !usingSecondary) {
                    compFireModes.Initialize(desiredFireModeProps);
                }
            }

            var baseBipodProps = thing.def.comps?.FirstOrDefault(x => x is CompProperties_BipodComp) as CompProperties_BipodComp;
            var bipodExtension = comp.GetFirstModExtensionsFromParts<MW2PartsExtension_CEBipod>();
            if (compBipod != null
                && (bipodExtension != null || !ReferenceEquals(compBipod.props, baseBipodProps))) {
                if (bipodExtension == null) {
                    compBipod.props = baseBipodProps;
                }
                else {
                    var category = bipodExtension.categoryDef ?? baseBipodProps?.catDef ?? MW2CE_DefOf.MW2CE_noBipod;
                    if (category != null) {
                        compBipod.props = new CompProperties_BipodComp {
                            catDef = category,
                            swayMult = category.swayMult,
                            swayPenalty = category.swayPenalty,
                            additionalrange = category.ad_Range,
                            recoilMulton = category.recoil_mult_setup,
                            recoilMultoff = category.recoil_mult_NOT_setup,
                            ticksToSetUp = category.setuptime,
                            warmupMult = category.warmup_mult_setup,
                            warmupPenalty = category.warmup_mult_NOT_setup
                        };
                    }
                }
            }

            var baseUnderBarrelProps = thing.def.comps?.FirstOrDefault(x => x is CompProperties_UnderBarrel) as CompProperties_UnderBarrel;
            var underBarrelExtension = comp.GetFirstModExtensionsFromParts<MW2PartsExtension_CEUnderbarrel>();
            var desiredUnderBarrelProps = underBarrelExtension?.underBarrelProps ?? baseUnderBarrelProps;
            var shouldRefreshUnderBarrel = compUnderBarrel != null
                && (underBarrelExtension != null || !ReferenceEquals(compUnderBarrel.props, baseUnderBarrelProps));
            if (shouldRefreshUnderBarrel) {
                compUnderBarrel.props = desiredUnderBarrelProps;
            }

            if (compUnderBarrel != null) {
                // The DefVerbProps prefix repopulates this from MW2's latest verb override.
                // Do not switch barrels here: an active KSG secondary magazine must stay active.
                compUnderBarrel._defVerbProps = null;

                if (usingSecondary) {
                    var secondaryProps = compUnderBarrel.Props;
                    if (compAmmo != null && secondaryProps?.propsUnderBarrel != null) {
                        compAmmo.props = secondaryProps.propsUnderBarrel;
                    }
                    if (compFireModes != null && secondaryProps?.propsFireModesUnderBarrel != null) {
                        compFireModes.props = secondaryProps.propsFireModesUnderBarrel;
                    }

                    var primaryVerb = compUnderBarrel.CompEq?.PrimaryVerb;
                    if (primaryVerb != null && secondaryProps?.verbPropsUnderBarrel != null) {
                        primaryVerb.verbProps = secondaryProps.verbPropsUnderBarrel;
                        AccessTools.Field(typeof(Verb), "cachedTicksBetweenBurstShots")?.SetValue(primaryVerb, null);
                    }

                    if (compFireModes != null) {
                        compFireModes.InitAvailableFireModes();
                    }
                }
            }
        }

        private static bool AmmoSetContains(CompProperties_AmmoUser props, AmmoDef ammo) {
            return ammo != null
                && props?.ammoSet?.ammoTypes?.Any(link => link.ammo == ammo) == true;
        }

        private static AmmoDef FirstAmmo(CompProperties_AmmoUser props) {
            return props?.ammoSet?.ammoTypes?.FirstOrDefault()?.ammo;
        }



        public static CompProperties_AmmoUser GetCompProps_AmmoUser(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.FirstOrFallback(t => t.GetType() == typeof(CompProperties_AmmoUser), null) as CompProperties_AmmoUser;
            if (baseProp == null) return null;
            var modExs = compMW.GetAllModExtensionsFromParts<MW2PartsExtension_CEAmmoUser>();
            if (modExs.EnumerableNullOrEmpty()) {
                return baseProp;
            }
            var result = new CompProperties_AmmoUser();
            //変更する奴
            result.magazineSize = (int)Math.Floor((baseProp.magazineSize + modExs.Select(t => t.magazineSizeOffset).Sum())
                * (1f + modExs.Select(t => t.magazineSizeMultiplier).Sum()));
            result.reloadTime =
                (baseProp.reloadTime + modExs.Select(t => t.reloadTimeOffset).Sum())
                * (1f + modExs.Select(t => t.reloadTimeFactor).Sum());
            result.ammoSet =
                modExs.FirstOrFallback(t => t.ammoSetOverride != null)?.ammoSetOverride
                ?? baseProp.ammoSet;
            result.reloadOneAtATime = modExs.FirstOrFallback(t => t.reloadOneAtATimeOverride != null)?.reloadOneAtATimeOverride
                ?? baseProp.reloadOneAtATime;
            //元のままのやつ
            result.AmmoGenPerMagOverride = baseProp.AmmoGenPerMagOverride;
            result.throwMote = baseProp.throwMote;
            result.loadedAmmoBulkFactor = baseProp.loadedAmmoBulkFactor;

            return result;
        }
        public static CompProperties_FireModes GetCompProps_FireModes(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.FirstOrFallback(t => t.GetType() == typeof(CompProperties_FireModes), null) as CompProperties_FireModes;
            if (baseProp == null) return null;
            var modExs = compMW.GetAllModExtensionsFromParts<MW2PartsExtension_CEFireModes>();
            if (modExs.EnumerableNullOrEmpty()) {
                return baseProp;
            }
            var result = new CompProperties_FireModes();

            result.aimedBurstShotCount = baseProp.aimedBurstShotCount + modExs.Sum(t => t.aimedBurstShotCountOffset);

            var ext_aiUsesBurstModeOffset = modExs.Sum(t => t.aiUsesBurstModeOffset);
            result.aiUseBurstMode =
                ext_aiUsesBurstModeOffset > 0 ? true :
                ext_aiUsesBurstModeOffset < 0 ? false :
                baseProp.aiUseBurstMode;

            var ext_useSingleShotOffset = modExs.Sum(t => t.useSingleShotOffset);
            result.noSingleShot =
                ext_useSingleShotOffset > 0 ? false :
                ext_useSingleShotOffset < 0 ? true :
                baseProp.noSingleShot;

            var ext_useSnapshotOffset = modExs.Sum(t => t.useSnapshotOffset);
            result.noSnapshot =
                ext_useSnapshotOffset > 0 ? false :
                ext_useSnapshotOffset < 0 ? true :
                baseProp.noSnapshot;

            result.aiAimMode = baseProp.aiAimMode;

            return result;
        }
        public static CompProperties_UnderBarrel GetCompProps_UnderBarrel(this CompModularWeapon compMW) {
            var result =
                compMW.GetFirstModExtensionsFromParts<MW2PartsExtension_CEUnderbarrel>()?.underBarrelProps ??
                compMW.parent?.def?.comps?.FirstOrFallback(t => t.GetType() == typeof(CompProperties_UnderBarrel), null);
            return result as CompProperties_UnderBarrel;
        }
        public static BipodCategoryDef GetBipodCatDef(this CompModularWeapon compMW) {
            var thing = compMW.parent;
            var baseProp = thing?.def.comps?.FirstOrFallback(t => t.GetType() == typeof(CompProperties_BipodComp), null) as CompProperties_BipodComp;
            if (baseProp == null) return MW2CE_DefOf.MW2CE_noBipod;
            var modExt = compMW.GetFirstModExtensionsFromParts<MW2PartsExtension_CEBipod>();
            return modExt?.categoryDef ?? baseProp.catDef;
        }
    }
}
