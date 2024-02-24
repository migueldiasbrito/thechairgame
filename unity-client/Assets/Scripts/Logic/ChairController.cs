using UnityEngine;

public class ChairController : MonoBehaviour
{
    private PlayerController _occupant = null;

    public bool TrySit(PlayerController player)
    {
        if (_occupant != null) return false;

        _occupant = player;
        return true;
    }

    public void LeaveChair(PlayerController player)
    {
        if (_occupant != player) return;

        _occupant = null;
    }
}
