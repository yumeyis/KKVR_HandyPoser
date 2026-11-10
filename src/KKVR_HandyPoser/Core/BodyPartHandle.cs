using RootMotion.FinalIK;
using UnityEngine;

namespace KKVR_HandyPoser.Core
{
    /// <summary>
    /// One grabbable body part of a character.
    ///
    /// Lifecycle:
    ///   Free    – driven by the animation (no anchor exists)
    ///   Grabbed – anchor is parented to a controller and follows it
    ///   Pinned  – anchor is released and stays where it was let go
    /// Reset() returns the part to Free and restores the game's IK settings.
    /// </summary>
    internal abstract class BodyPartHandle
    {
        protected BodyPartHandle(PoseTarget owner, BodyPart part)
        {
            Owner = owner;
            Part = part;
        }

        public PoseTarget Owner { get; }

        public BodyPart Part { get; }

        /// <summary>Bone used for proximity tests and marker placement.</summary>
        public abstract Transform Bone { get; }

        /// <summary>Anchor object the IK is pointed at; null while the part is free.</summary>
        protected Transform Anchor { get; private set; }

        public bool IsActive => Anchor != null;

        public bool IsGrabbed => Anchor != null && GrabbedBy != null;

        public Transform GrabbedBy { get; private set; }

        public void Grab(Transform controller)
        {
            if (Anchor == null)
            {
                Anchor = new GameObject($"HandyPoser_{Owner.Name}_{Part}").transform;
                Anchor.position = InitialAnchorPosition();
                Anchor.rotation = Bone.rotation;
                OnActivate();
            }

            // Keep the current offset so the part does not snap to the controller.
            Anchor.SetParent(controller, worldPositionStays: true);
            GrabbedBy = controller;
        }

        public void Release(Transform pinParent)
        {
            if (Anchor == null)
                return;
            Anchor.SetParent(pinParent, worldPositionStays: true);
            GrabbedBy = null;
        }

        public void Reset()
        {
            if (Anchor == null)
                return;
            OnDeactivate();
            Object.Destroy(Anchor.gameObject);
            Anchor = null;
            GrabbedBy = null;
        }

        /// <summary>
        /// Re-apply IK overrides. Called every LateUpdate because the game's
        /// MotionIK rewrites effector weights whenever the animation changes.
        /// </summary>
        public void Apply()
        {
            if (Anchor != null)
                OnApply();
        }

        protected virtual Vector3 InitialAnchorPosition() => Bone.position;

        /// <summary>Remember the game's original IK settings and point the IK at the anchor.</summary>
        protected abstract void OnActivate();

        protected abstract void OnApply();

        /// <summary>Restore the game's original IK settings.</summary>
        protected abstract void OnDeactivate();
    }

    /// <summary>Hands, feet, shoulders, thighs and hips: FBBIK effectors.</summary>
    internal sealed class EffectorHandle : BodyPartHandle
    {
        private readonly IKEffector _effector;
        private Transform _origTarget;
        private float _origPositionWeight;
        private float _origRotationWeight;

        public EffectorHandle(PoseTarget owner, BodyPart part, IKEffector effector) : base(owner, part)
        {
            _effector = effector;
        }

        public override Transform Bone => _effector.bone;

        protected override void OnActivate()
        {
            _origTarget = _effector.target;
            _origPositionWeight = _effector.positionWeight;
            _origRotationWeight = _effector.rotationWeight;
            OnApply();
        }

        protected override void OnApply()
        {
            _effector.target = Anchor;
            _effector.positionWeight = 1f;
            _effector.rotationWeight = Part.UsesRotation() ? 1f : 0f;
        }

        protected override void OnDeactivate()
        {
            _effector.target = _origTarget;
            _effector.positionWeight = _origPositionWeight;
            _effector.rotationWeight = _origRotationWeight;
        }
    }

    /// <summary>Elbows and knees: the bend goal of an FBBIK limb chain.</summary>
    internal sealed class BendGoalHandle : BodyPartHandle
    {
        private const float GoalOffset = 0.1f;

        private readonly FBIKChain _chain;
        private Transform _origGoal;
        private float _origWeight;

        public BendGoalHandle(PoseTarget owner, BodyPart part, FBIKChain chain) : base(owner, part)
        {
            _chain = chain;
        }

        // nodes[0] = upper arm / thigh, nodes[1] = forearm / calf, nodes[2] = hand / foot
        public override Transform Bone => _chain.nodes[1].transform;

        protected override Vector3 InitialAnchorPosition()
        {
            // Place the goal slightly outside the joint, in the direction the limb currently bends,
            // so the limb keeps its current shape on activation.
            Vector3 root = _chain.nodes[0].transform.position;
            Vector3 joint = _chain.nodes[1].transform.position;
            Vector3 end = _chain.nodes[2].transform.position;
            Vector3 bendDir = joint - (root + end) * 0.5f;
            return joint + (bendDir.sqrMagnitude > 1e-6f ? bendDir.normalized * GoalOffset : Vector3.zero);
        }

        protected override void OnActivate()
        {
            _origGoal = _chain.bendConstraint.bendGoal;
            _origWeight = _chain.bendConstraint.weight;
            OnApply();
        }

        protected override void OnApply()
        {
            _chain.bendConstraint.bendGoal = Anchor;
            _chain.bendConstraint.weight = 1f;
        }

        protected override void OnDeactivate()
        {
            _chain.bendConstraint.bendGoal = _origGoal;
            _chain.bendConstraint.weight = _origWeight;
        }
    }
}
