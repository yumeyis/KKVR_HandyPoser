# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-11-10

### Added
- Initial project scaffolding (BepInEx 5 / HarmonyX plugin targeting Koikatsu official VR).
- Grab, pin and reset character body parts (hands, feet, elbows, knees, shoulders, thighs, hips)
  with VR controllers.
- Grab markers, haptic feedback and game-input blocking while posing.
- Configurable grab button, grab radius, pin space and grabbable part groups.
- "Reset all buttons" setting: reset all parts with a controller button combo such as Grip + Touchpad.
- Markers are hidden by default ("Show markers" = `false`).
