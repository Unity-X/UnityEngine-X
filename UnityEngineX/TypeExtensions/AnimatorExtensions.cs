using UnityEngine;

namespace UnityEngineX
{
    public static class AnimatorExtensions
    {
        public static bool HasParameter(this Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        public static bool HasParameter(this Animator animator, int paramNameHash)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.nameHash == paramNameHash)
                    return true;
            }
            return false;
        }
    }

}