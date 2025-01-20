using Il2CppSLZ.Marrow.Interaction;

using System.Linq;

namespace FluidLab;

public static class BodyUtilities
{
    public static bool HasNullColliders(this MarrowBody marrowBody)
    {
        if (marrowBody.Colliders == null)
        {
            return true;
        }

        if (marrowBody.Colliders.Any((c) => c == null))
        {
            return true;
        }

        if (marrowBody.Triggers == null)
        {
            return true;
        }


        if (marrowBody.Triggers.Any((c) => c == null))
        {
            return true;
        }

        return false;
    }
}
