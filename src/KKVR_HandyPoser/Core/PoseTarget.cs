using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace KKVR_HandyPoser.Core
{
    /// <summary>
    /// A character that can be posed, together with its grabbable body parts.
    /// </summary>
    internal sealed class PoseTarget
    {
        private readonly List<BodyPartHandle> _handles = new List<BodyPartHandle>();

        private PoseTarget(ChaControl character, FullBodyBipedIK ik)
        {
            Character = character;
            Ik = ik;

            var solver = ik.solver;
            AddEffector(BodyPart.Hips, solver.bodyEffector);
            AddEffector(BodyPart.LeftShoulder, solver.leftShoulderEffector);
            AddEffector(BodyPart.RightShoulder, solver.rightShoulderEffector);
            AddEffector(BodyPart.LeftThigh, solver.leftThighEffector);
            AddEffector(BodyPart.RightThigh, solver.rightThighEffector);
            AddEffector(BodyPart.LeftHand, solver.leftHandEffector);
            AddEffector(BodyPart.RightHand, solver.rightHandEffector);
            AddEffector(BodyPart.LeftFoot, solver.leftFootEffector);
            AddEffector(BodyPart.RightFoot, solver.rightFootEffector);
            AddBendGoal(BodyPart.LeftElbow, solver.leftArmChain);
            AddBendGoal(BodyPart.RightElbow, solver.rightArmChain);
            AddBendGoal(BodyPart.LeftKnee, solver.leftLegChain);
            AddBendGoal(BodyPart.RightKnee, solver.rightLegChain);
        }

        public ChaControl Character { get; }

        public FullBodyBipedIK Ik { get; }

        public IList<BodyPartHandle> Handles => _handles;

        public bool IsMale => Character.sex == 0;

        public string Name => Character.name;

        /// <summary>False once the character has been destroyed (e.g. scene change).</summary>
        public bool IsValid => Character != null && Ik != null;

        /// <summary>Whether the character is currently rendered (the male is often hidden in VR).</summary>
        public bool IsVisible => Character.visibleAll;

        /// <summary>Transform pinned anchors are parented to in <see cref="PinSpace.Character"/> mode.</summary>
        public Transform PinRoot => Character.transform;

        public static PoseTarget TryCreate(ChaControl character)
        {
            if (character == null || character.objAnim == null)
                return null;

            var ik = character.objAnim.GetComponent<FullBodyBipedIK>();
            if (ik == null || ik.solver == null)
            {
                HandyPoserPlugin.Log.LogDebug($"{character.name} has no FullBodyBipedIK, skipping");
                return null;
            }

            return new PoseTarget(character, ik);
        }

        public void ResetAll()
        {
            foreach (var handle in _handles)
                handle.Reset();
        }

        private void AddEffector(BodyPart part, IKEffector effector)
        {
            if (effector != null && effector.bone != null)
                _handles.Add(new EffectorHandle(this, part, effector));
        }

        private void AddBendGoal(BodyPart part, FBIKChain chain)
        {
            if (chain != null && chain.nodes != null && chain.nodes.Length >= 3 && chain.bendConstraint != null)
                _handles.Add(new BendGoalHandle(this, part, chain));
        }
    }
}
