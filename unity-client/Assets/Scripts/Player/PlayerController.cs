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


    private Vector2 _movement = Vector2.zero;
    private bool _sit = false;
    private float _sqrMaxDistance;

    private bool _isSitted = false;
    public ChairController _chairOccupied { get; private set; } = null;

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

    public void OnSit(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _sit = true;
       
    }

    public void Init(GameManager gameManager, Action<PlayerController> onReadyCallback)
    {
        _gameManager = gameManager;
        _onReadyCallback = onReadyCallback;
    }

    public void Eliminate()
    {
        if (_chairOccupied != null)
        {
            _chairOccupied.LeaveChair(this);
            _chairOccupied = null;
        }

        // Animate me instead...
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        HandleSitting();

        HandleMovement();
    }

    private void HandleSitting()
    {
        if (!_sit) return;

        _isSitted = !_isSitted;

        if (_isSitted)
        {
            TrySitOnChair();
        }
        else
        {
            GetUp();
        }

        _sit = false;
    }

    private void TrySitOnChair()
    {
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

            _chairOccupied = chair.x;
            _sonReact.Sit();
            Vector3 position = transform.position;
            position.x = chair.x.transform.position.x;
            position.z = chair.x.transform.position.z;
            transform.position = position;

            _gameManager.OnPlayerSit(this);

            break;
        }
    }

    private void GetUp()
    {
        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        if (_chairOccupied != null)
        {
            _chairOccupied.LeaveChair(this);
            _sonReact.GetUP();
            _chairOccupied = null;
         
        }

    
    }

    private void HandleMovement()
    {
        if (_isSitted) return;

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

    private void OnDestroy()
    {
        
    }
}
