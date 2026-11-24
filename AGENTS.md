# Working on MW2Patch_CombatExtended

These instructions apply throughout the repository unless a closer AGENTS.md supplies more specific guidance. English is the default for repository documentation, issues, milestones, pull requests, and code comments.

## Project and sources

This mod connects the ModularWeapons 2 (MW2) gunsmith workbench and parts system to Combat Extended (CE). Follow the load order in README.md: ModularWeapons 2, Combat Extended, then this patch. The [compatibility roadmap](https://github.com/FennexFox/MW2Patch_CombatExtended/issues/4) records the RimWorld 1.6 targets and phase dependencies.

- Inspect the matching RimWorld version and add-on definitions before editing. Use exact defName values, including upstream spelling. Read inherited definitions and conditional patches when determining the effective configuration.
- The C# solution contains the integration code. Start with MW2Patch_CombatExtended.cs and the relevant MW2PartsExtension_CE* file for changes to ammunition, fire modes, bipods, or underbarrel behavior. For weapon-specific slots and graphics, inspect the XML shipped with the matching MW2 and [patch release](https://github.com/kaitorisenkou/MW2Patch_CombatExtended/releases). Release archives can contain assets absent from the source checkout.
- Make changes in the repository. Treat installed Workshop copies as reference inputs. Keep optional add-on definitions conditional so each supported add-on can load independently. Preserve existing weapon identifiers and saved configurations when extending support.
- Keep edits scoped to the issue. Preserve unrelated worktree changes and keep local binaries, workshop dependencies, build output, and player logs out of commits.

## Verification

- For C# changes, build MW2Patch_CombatExtended.sln with MSBuild and the .NET Framework 4.7.2 targeting pack. Check the project reference paths for Harmony, RimWorld, CE, and MW2 before diagnosing a build failure.
- For XML changes, check that each target and part resolves with the intended mods loaded. Inspect startup logs for patch failures. Test the base configuration, the affected add-on, and any add-on combination whose behavior the change touches.
- At the gunsmith workbench, fit and remove the affected parts, inspect sprite alignment, and save and reload a modified weapon. Exercise the CE features that the weapon uses. For the KSG, this includes switching and reloading both magazines after customization.
- Record the game and mod versions, configurations tested, and observed results. Identify checks still pending, especially when only source inspection or a build was possible.

## Coordinator and subagent models

Use delegation when a bounded investigation or implementation can proceed independently. The coordinator defines the question, assigns ownership, resolves conflicting evidence, and verifies the integrated result. Use the following routing preferences when the runtime offers model and reasoning-effort selection.

| Role | Model and effort | Suitable work |
| --- | --- | --- |
| Coordinator | GPT-6 Astra, low | Plan the change, divide work, integrate findings, and perform final checks. |
| Inventory worker | GPT-5.6 Luna, low | Locate definitions and usages, compare weapon lists, or classify log entries. |
| Bounded analysis worker | GPT-5.6 Luna, medium | Trace a specific XML target or make a small, mechanically specified edit. |
| Default implementation worker | GPT-5.6 Luna, xhigh | Implement scoped patches, debug localized behavior, and add relevant regression coverage. |
| Deeper bounded investigation | GPT-5.6 Luna, max | Revisit an incomplete xhigh result when several local causes need comparison or verification. |
| Integration worker | GPT-5.6 Sol, medium | Resolve interactions spanning MW2 parts, CE components, XML inheritance, or save state. Escalate directly here when context breadth is the difficulty. |
| Difficult implementation | GPT-5.6 Sol, high | Handle persistent cross-component bugs or state transitions that Sol medium could not resolve. |
| Coordinator escalation | GPT-6 Astra, medium | Reassess a disproved assumption, reconcile conflicting source and runtime evidence, or decide an architectural change. |
| Exceptional coordinator escalation | GPT-6 Astra, high | Resolve a high-impact decision that remains open after Astra medium. |

Give each worker a fresh, narrow context, a concrete question, and an expected deliverable. Select both model and effort explicitly when supported. Assign separate write scopes to concurrent workers. Ask for inspected paths or symbols, observed results, checks run, and unresolved assumptions.

Choose escalation by the cause of the difficulty. The coordinator checks consequential claims against the source or a reproducible game observation and resolves disagreements by examining the evidence. Return routine work to the least expensive reliable worker once the difficult decision is settled. If a preferred model is unavailable, preserve the role and choose an available model with suitable capability. Handle small tasks directly when delegation would add more overhead than it saves.
