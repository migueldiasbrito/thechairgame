using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider _collider;
    [SerializeField] private float _speed = 100;
    [SerializeField] private float _dashForce = 100;
    [SerializeField] private float _rotationSpeed = 100;
    [SerializeField] private float _maxDistanceToSitOnChair = 1;
    [SerializeField] private Animation_reaction _sonReact;
    [field: SerializeField] public ChairController InitialChair { get; private set; }
    [SerializeField] private Vector2 _fogoNoCuForce = new Vector2(5f, 5f);
    [SerializeField] private float _maxStopTime = 1f;
    [SerializeField] private float _stopedTimeAfterBurst = 1f;


    private Vector2 _movement = Vector2.zero;
    private bool _action = false;
    private bool _dash = false;
    private float _sqrMaxDistance;
    private bool _canMove = true;

    public bool IsSitted { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;

    public ChairController ChairOccupied { get; private set; } = null;

    private GameManager _gameManager;

    private Action<PlayerController> _onReadyCallback;

    private Coroutine _isStoppedCoroutine = null;

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
    public void OnActionButtonClickedDash(InputAction.CallbackContext callbackContext)
    {
        if (!callbackContext.performed) return;

        _dash = true;


    }

    public void Init(GameManager gameManager, Action<PlayerController> onReadyCallback)
    {
        _gameManager = gameManager;
        _onReadyCallback = onReadyCallback;

        if (TryGetComponent(out ChairController chair))
            _gameManager.AddChair(chair);
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
        if (!_canMove)
        {
            _action = false;
            return;
        }

        HandleChair();

        HandleSitting();

        HandleMovement();


        Dash();

        HandleMaxStopTime();

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

        foreach (var chair in chairs)
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

    public void Dash()
    {
        if (!_dash) return;

        // Apply the dash force
        StartCoroutine(GoDash());

        _dash = false;
    }



    private void HandleMaxStopTime()
    {
        if (_isStoppedCoroutine == null)
        {
            if (_gameManager.State == GameState.TurnStarted && _movement.magnitude <= 0.1f)
            {
                _isStoppedCoroutine = StartCoroutine(MexeTe());
            }
        }
        else
        {
            if (_gameManager.State != GameState.TurnStarted || _movement.magnitude >= 0.1f)
            {
                StopCoroutine(_isStoppedCoroutine);
                _isStoppedCoroutine = null;
            }
        }
    }


    private void Awake()
    {
        _sqrMaxDistance = Mathf.Pow(_maxDistanceToSitOnChair, 2);
    }

    private void Start()
    {
        InitialChair = GetComponentInChildren<ChairController>();
    }

    private IEnumerator MexeTe()
    {
        yield return new WaitForSeconds(_maxStopTime);

        Vector3 velocity = _rigidbody.velocity;
        velocity.y = _fogoNoCuForce.y;

        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomZ = UnityEngine.Random.Range(-1f, 1f);
        velocity.x = _fogoNoCuForce.x * randomX;
        velocity.z = _fogoNoCuForce.x * randomZ;

        _rigidbody.velocity = velocity;

        StartCoroutine(NaoTeMexesAgora());
    }

    private IEnumerator NaoTeMexesAgora()
    {
        _canMove = false;
        yield return new WaitForSeconds(_stopedTimeAfterBurst);
        _canMove = true;
    }

    private void OnDestroy()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            if (IsDashing)
                collision.gameObject.GetComponent<Rigidbody>().AddForce(collision.gameObject.transform.forward * _dashForce * 10);
        }
    }

    IEnumerator GoDash()
    {
        _sonReact.DoDashAnimation();
        IsDashing = true;
        yield return new WaitForSeconds(1);
        IsDashing = false;
    }
}
