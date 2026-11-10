using System;
using System.Collections.Generic;
using KKVR_HandyPoser.Controls;
using KKVR_HandyPoser.Core;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KKVR_HandyPoser.Visual
{
    /// <summary>
    /// Draws small spheres on grabbable body parts near the controllers.
    /// Colors: white = free, cyan = pinned, yellow = hovered, green = grabbed.
    /// </summary>
    internal sealed class MarkerRenderer : IDisposable
    {
        private enum MarkerState { Free, Pinned, Hover, Grabbed }

        private static readonly Color[] StateColors =
        {
            new Color(1f, 1f, 1f),
            new Color(0.2f, 0.8f, 1f),
            new Color(1f, 0.9f, 0.2f),
            new Color(0.3f, 1f, 0.3f),
        };

        private readonly Dictionary<BodyPartHandle, Renderer> _markers = new Dictionary<BodyPartHandle, Renderer>();
        private readonly List<BodyPartHandle> _stale = new List<BodyPartHandle>();
        private readonly GameObject _root;
        private Material[] _materials;

        public MarkerRenderer()
        {
            _root = new GameObject("HandyPoser_Markers");
        }

        public void Update(IList<PoseTarget> targets, IControllerInput input,
            BodyPartHandle hoverLeft, BodyPartHandle hoverRight, bool posingEnabled)
        {
            bool show = posingEnabled && HandyPoserPlugin.ShowMarkers.Value;
            _root.SetActive(show);
            if (!show)
                return;

            float showDistSqr = HandyPoserPlugin.MarkerShowDistance.Value * HandyPoserPlugin.MarkerShowDistance.Value;
            float size = HandyPoserPlugin.MarkerSize.Value;
            var left = input.GetTransform(ControllerSide.Left);
            var right = input.GetTransform(ControllerSide.Right);
            var parts = HandyPoserPlugin.EnabledParts.Value;
            bool allowMale = HandyPoserPlugin.AllowMaleTargets.Value;

            foreach (var target in targets)
            {
                if (!target.IsValid)
                    continue;
                bool targetShown = target.IsVisible && (!target.IsMale || allowMale);

                foreach (var handle in target.Handles)
                {
                    var marker = GetOrCreate(handle);
                    Vector3 pos = handle.Bone.position;
                    bool near = IsNear(left, pos, showDistSqr) || IsNear(right, pos, showDistSqr);
                    bool visible = targetShown && near && parts.Contains(handle.Part.Group());

                    marker.enabled = visible;
                    if (!visible)
                        continue;

                    marker.transform.position = pos;
                    marker.transform.localScale = Vector3.one * size;

                    MarkerState state = handle.IsGrabbed ? MarkerState.Grabbed
                        : handle == hoverLeft || handle == hoverRight ? MarkerState.Hover
                        : handle.IsActive ? MarkerState.Pinned
                        : MarkerState.Free;
                    marker.sharedMaterial = _materials[(int)state];
                }
            }

            RemoveStale();
        }

        public void Dispose()
        {
            if (_root != null)
                Object.Destroy(_root);
            if (_materials != null)
            {
                foreach (var material in _materials)
                    Object.Destroy(material);
            }
            _markers.Clear();
        }

        private static bool IsNear(Transform controller, Vector3 pos, float distSqr)
        {
            return controller != null && (controller.position - pos).sqrMagnitude <= distSqr;
        }

        private Renderer GetOrCreate(BodyPartHandle handle)
        {
            if (_markers.TryGetValue(handle, out var existing) && existing != null)
                return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Marker_{handle.Owner.Name}_{handle.Part}";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(_root.transform, worldPositionStays: false);

            var renderer = go.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            if (_materials == null)
            {
                _materials = new Material[StateColors.Length];
                for (int i = 0; i < StateColors.Length; i++)
                    _materials[i] = new Material(renderer.sharedMaterial) { color = StateColors[i] };
            }

            _markers[handle] = renderer;
            return renderer;
        }

        private void RemoveStale()
        {
            _stale.Clear();
            foreach (var pair in _markers)
            {
                if (!pair.Key.Owner.IsValid || pair.Value == null)
                    _stale.Add(pair.Key);
            }
            foreach (var handle in _stale)
            {
                if (_markers[handle] != null)
                    Object.Destroy(_markers[handle].gameObject);
                _markers.Remove(handle);
            }
        }
    }
}
