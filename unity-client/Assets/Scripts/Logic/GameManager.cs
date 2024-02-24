using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public List<ChairController> Chairs { get; private set; }
}
