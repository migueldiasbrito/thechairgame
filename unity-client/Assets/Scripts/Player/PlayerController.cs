using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] private float _speed = 100;
    [SerializeField] private float _rotationSpeed = 100;
    [SerializeField] private float _maxDistanceToSitOnChair = 1;
    [SerializeField] private Animation_reaction _sonReact ;
    [field: SerializeField] public ChairController InitialChair { get; private set; }


    private Vector2 _movement = Vector2.zero;
    private bool _action = false;
    private float _sqrMaxDistance;

    public bool IsSitted { get; private set; } = false;
    public ChairController ChairOccupied { get; private set; } = null;

    private GameManager _gameManager;

    private Action<PlayerController> _onReadyCallback;

    public void OnPlayerReady(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _onReadyCallback.Invoke(this);
    }

    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        _movement = callbackContext.ReadValue<Vector2>();
    }

    public void OnActionButtonClicked(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _action = true;
       
    }

    public void Init(GameManager gameManager, Action<PlayerController> onReadyCallback)
    {
        _gameManager = gameManager;
        _onReadyCallback = onReadyCallback;

        _gameManager.AddChair(GetComponentInChildren<ChairController>());
    }

    public void Eliminate()
    {
        if (ChairOccupied != null)
        {
            ChairOccupied.LeaveChair(this);
            ChairOccupied = null;
        }

        // Animate me instead...
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        HandleChair();

        HandleSitting();

        HandleMovement();
    }

    private void HandleChair()
    {
        if (!_action || InitialChair == null || _gameManager.State != GameState.Setup) return;

        Vector3 chairPosition = InitialChair.transform.localPosition;
        chairPosition.y = 0;
        InitialChair.transform.localPosition = chairPosition;

        InitialChair.transform.parent = null;
        InitialChair = null;

        _gameManager.ChairPlaced();

        _action = false;
    }

    private void HandleSitting()
    {
        if (!_action) return;

        if (InitialChair != null)
        {
            _action = false;
            return;
        }

        if (IsSitted)
        {
            GetUp();
        }
        else
        {
            TrySitOnChair();
        }

        _action = false;
    }

    private void TrySitOnChair()
    {
        IsSitted = true;

        _rigidbody.isKinematic = true;
        _collider.enabled = false;

        var chairs = _gameManager.Chairs
            .Select(x => (x, (x.transform.position - transform.position).sqrMagnitude))
            .Where(x => x.sqrMagnitude <= _sqrMaxDistance)
            .ToList();
        chairs.Sort((x, y) => x.sqrMagnitude.CompareTo(y.sqrMagnitude));

        foreach(var chair in chairs)
        {
            if (!chair.x.TrySit(this)) break;

            ChairOccupied = chair.x;
            _sonReact.Sit();
            Vector3 position = transform.position;
            position.x = chair.x.transform.position.x;
            position.z = chair.x.transform.position.z;
            transform.position = position;

            _gameManager.OnPlayerSit(this);

            break;
        }
    }

    public void GetUp()
    {
        IsSitted = false;

        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        if (ChairOccupied != null)
        {
            ChairOccupied.LeaveChair(this);
            _sonReact.GetUP();
            ChairOccupied = null;
        }
    }

    private void HandleMovement()
    {
        if (IsSitted) return;

        Vector3 velocity = _rigidbody.velocity;
        velocity.x = _movement.x * _speed * Time.fixedDeltaTime;
        velocity.z = _movement.y * _speed * Time.fixedDeltaTime;

        Vector3 inputDirection = new Vector3(_movement.x, 0, _movement.y).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // Calculate the rotation to look towards the movement direction
            Quaternion toRotation = Quaternion.LookRotation(inputDirection, Vector3.up);

            // Smoothly interpolate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.fixedDeltaTime * _rotationSpeed);
        }

        _rigidbody.velocity = velocity;
    }

    private void Awake()
    {
        _sqrMaxDistance = Mathf.Pow(_maxDistanceToSitOnChair, 2);
    }

    private void Start()
    {
        InitialChair = GetComponentInChildren<ChairController>();
    }

    private void OnDestroy()
    {
        
    }
}
