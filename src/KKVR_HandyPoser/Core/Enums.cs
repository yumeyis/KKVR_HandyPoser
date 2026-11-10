using System;

namespace KKVR_HandyPoser.Core
{
    /// <summary>
    /// Body parts that can be grabbed. Each maps to a FinalIK FullBodyBipedIK
    /// effector or to a limb chain's bend goal.
    /// </summary>
    public enum BodyPart
    {
        Hips,
        LeftShoulder,
        RightShoulder,
        LeftThigh,
        RightThigh,
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot,
        LeftElbow,
        RightElbow,
        LeftKnee,
        RightKnee,
    }

    /// <summary>
    /// Config-facing groups of body parts.
    /// </summary>
    [Flags]
    public enum BodyPartGroup
    {
        None = 0,
        HandsAndFeet = 1 << 0,
        ElbowsAndKnees = 1 << 1,
        ShouldersAndThighs = 1 << 2,
        Hips = 1 << 3,
        All = HandsAndFeet | ElbowsAndKnees | ShouldersAndThighs | Hips,
    }

    /// <summary>
    /// What a released (pinned) part stays attached to.
    /// </summary>
    public enum PinSpace
    {
        /// <summary>Pinned parts move along with the character root.</summary>
        Character,

        /// <summary>Pinned parts stay at a fixed spot in the scene.</summary>
        World,
    }

    internal static class BodyPartExtensions
    {
        public static BodyPartGroup Group(this BodyPart part)
        {
            switch (part)
            {
                case BodyPart.Hips:
                    return BodyPartGroup.Hips;
                case BodyPart.LeftShoulder:
                case BodyPart.RightShoulder:
                case BodyPart.LeftThigh:
                case BodyPart.RightThigh:
                    return BodyPartGroup.ShouldersAndThighs;
                case BodyPart.LeftElbow:
                case BodyPart.RightElbow:
                case BodyPart.LeftKnee:
                case BodyPart.RightKnee:
                    return BodyPartGroup.ElbowsAndKnees;
                default:
                    return BodyPartGroup.HandsAndFeet;
            }
        }

        /// <summary>Whether the part's rotation should follow the controller too.</summary>
        public static bool UsesRotation(this BodyPart part)
        {
            return part == BodyPart.LeftHand || part == BodyPart.RightHand
                || part == BodyPart.LeftFoot || part == BodyPart.RightFoot;
        }

        // Enum.HasFlag does not exist on .NET 3.5
        public static bool Contains(this BodyPartGroup flags, BodyPartGroup group)
        {
            return (flags & group) == group;
        }
    }
}
