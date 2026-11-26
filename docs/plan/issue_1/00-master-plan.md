# CE core and CE Attachments workbench support

## Issue Target And Scope Summary

- Issue target: #1
- Add workbench support to the three CE core weapons and seven CE Attachments weapons listed in issue #1.

## Strategy

- Extend the upstream release with target-specific mounts on existing sprites. Preserve each native CE verb and component.

## Phase Order

1. [Implement and verify the ten weapon definitions](01-support.md)

## Phase Dependencies

- Build the integration against the RimWorld 1.7, MW2 1.7 and CE 17.0 assemblies before testing the packaged weapon patches.

## Source Of Truth Decisions

- `00-master-plan.md` is the phased implementation plan source of truth.
- Phase files in this directory define phase-local scope and validation.
- Use the pinned CE development revision and the CE Attachments definitions actually selected by RimWorld. Record the game build and mod revisions in the phase report.

## Global Validation Expectations

- Validate the ten XML targets and packaged contents. Launch the game with CE alone, then enable CE Attachments and exercise the native secondary functions.

## Known Risks And Assumptions

- CE uses its underbarrel system for several secondary functions, including the second KSG tube. Check these with parts installed and after loading a save.
