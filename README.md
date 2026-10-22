# Living Empire — Daggerfall Unity 1.0.0

**Development source, not a playable mod.** An autonomous political-history project confined to the Iliac Bay.

## What is here
- `src/`: C# prototype for DFU initialization, state, daily heartbeat and save data.
- `data/`: 366 canonical faction records and 255 **provisional** geographic influence placements. County divisions are synthetic, not vanilla DFU counties.
- `.github/workflows/validate.yml`: cloud data/source checks (not a Unity build).
- `tools/validate.py`: checks data invariants and the presence of key C# source declarations.

## Verified / not verified
The C# code passed a **net8.0 compile-only** diagnostic against installed DFU 1.0.0 assemblies on Windows (0 errors/warnings). This is **not** Unity runtime compatibility or `.dfmod` validation. No `.dfmod` exists yet. The full PowerShell autonomous political engine has **not** been ported into the C# game mod.

## Actual packaging requirements
A working `.dfmod` requires a compatible DFU Unity project, Unity Editor (DFU 1.0.0 targets Unity 2019.4.40f1), Unity Mod Builder integration, and a tested build runner with appropriate Unity licensing. **GitHub-hosted Actions cannot be assumed to have the Unity Editor or license preinstalled.**

The current workflow deliberately validates repository contents rather than pretending to produce a game-ready `.dfmod`. Next: establish a reproducible licensed Unity build environment, compile in Unity, configure the DFU Mod Builder, then load and save/reload-test the mod.

## Safety
Do not commit commercial game binaries, Unity activation files, private tokens, or personal save games.
