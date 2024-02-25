using System.Collections;
using UnityEngine;

public class ChairController : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private float _desintegrationSpeed = 0.5f;

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

    public void DestroyChair()
    {
        StartCoroutine(Desintegramos());
    }

    private IEnumerator Desintegramos()
    {
        float value = 0;

        do
        {
            yield return null;

            value += _desintegrationSpeed * Time.deltaTime;
            foreach (Renderer renderer in _renderers)
                renderer.material.SetFloat("_dissolveamount", value);

        } while (value <= 1);

        Destroy(gameObject);
    }
}
