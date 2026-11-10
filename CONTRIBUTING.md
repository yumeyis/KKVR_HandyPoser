# Contributing

Thanks for your interest in improving KKVR_HandyPoser!

## Workflow

1. Fork the repository and create a branch from `main` (`feature/...`, `fix/...`).
2. Set up the build as described in [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md).
3. Keep commits focused; write messages in the imperative mood ("Add knee handles").
4. Test in game (official VR H scene) and describe what you tested in the PR.
5. Add an entry under `## [Unreleased]` in [CHANGELOG.md](CHANGELOG.md).

## Code style

- Follow `.editorconfig` (4 spaces, `_camelCase` private fields, braces on new lines).
- Target **.NET 3.5 / C# latest**: no `Enum.HasFlag`, no `ValueTuple`, no `async`/`Task`.
- Use explicit `!= null` checks for `UnityEngine.Object` (not `?.` / `??`), because Unity overrides
  equality for destroyed objects.
- Keep VR-runtime-specific code in `Controls/` and `Patches/`; `Core/` should stay runtime-agnostic.

## Never commit

- Game assemblies (`Assembly-CSharp.dll`, `UnityEngine*.dll`, ...): they are proprietary.
- `Directory.Build.user.props` (personal paths).
