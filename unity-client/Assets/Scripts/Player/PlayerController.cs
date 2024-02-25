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

    [SerializeField] private Animation_reaction[] _bonecoPrefab;
    [SerializeField] private ChairController[] _cadeirasPrefabs;

    [SerializeField] private float _desintegrationSpeed = 1;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _dashSound;
    [SerializeField] private AudioClip _hittedSound;
    [SerializeField] private AudioClip _burstedSound;

    private Vector2 _movement = Vector2.zero;
    private bool _action = false;
    private bool _dash = false;
    private float _sqrMaxDistance;
    private bool _canMove = true;

    public bool IsSitted { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    private bool IsPushed = false;

    public ChairController ChairOccupied { get; private set; } = null;

    private GameManager _gameManager;

    private Action<PlayerController> _onReadyCallback;

    private Coroutine _isStoppedCoroutine = null;
    private Coroutine _dashCoroutine = null;

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
    }

    public void Eliminate()
    {
        if (ChairOccupied != null)
        {
            ChairOccupied.LeaveChair(this);
            ChairOccupied = null;
        }

        // Animate me instead...
        //Destroy(gameObject);

        _sonReact.Lose();

        StopAllCoroutines();

        _canMove = false;
        _rigidbody.isKinematic = true;

        StartCoroutine(Desintegramos());
    }

    private IEnumerator Desintegramos()
    {
        float value = 0;

        do
        {
            yield return null;

            value += _desintegrationSpeed * Time.deltaTime;
            foreach (Renderer renderer in _sonReact.Renderers)
            {
                renderer.material.SetFloat("_dissolveamount", value);
            }

        } while (value <= 1);

        Destroy(gameObject);
    }

    public void Win()
    {
        _sonReact.Win();

        StopAllCoroutines();

        _canMove = false;
        _rigidbody.isKinematic = true;
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

        Dash();

        HandleMovement();

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

            Vector3 position = transform.position;
            position.x = chair.x.transform.position.x;
            position.z = chair.x.transform.position.z;
            transform.position = position;

            _gameManager.OnPlayerSit(this);

            break;
        }

        _sonReact.Sit(ChairOccupied != null);
    }

    public void GetUp()
    {
        IsSitted = false;

        _rigidbody.isKinematic = false;
        _collider.enabled = true;

        _sonReact.GetUP(ChairOccupied != null);

        if (ChairOccupied != null)
        {
            ChairOccupied.LeaveChair(this);
            ChairOccupied = null;
        }
    }

    private void HandleMovement()
    {
        if (IsSitted || IsDashing || IsPushed) return;

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
        if (_dash && !IsSitted && !IsDashing && !IsPushed && InitialChair == null)
        {
            _dashCoroutine = StartCoroutine(GoDash());
        }

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
                SetMaterialAlpha(0);
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
        int _sonPrefabIndex = UnityEngine.Random.Range(0, _bonecoPrefab.Length);
        _sonReact = Instantiate(_bonecoPrefab[_sonPrefabIndex], transform);

        foreach (Transform child in _sonReact.transform)
        {
            // Generate a random color with alpha included
            Color randomColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);

            // Set the alpha value to 1
            randomColor.a = 1f;
            if (child.tag == "shirt")
            {
    
                child.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
            }
            foreach (Transform child2 in child.transform)
            {
                if (child2.tag == "shirt")
                {
                    child2.GetComponent<Renderer>().material.SetColor("_Color", randomColor);
                }
            }
        }
        // :)
        //_sonReact.transform.localPosition += new Vector3(0, -1.14f, 0);

        if (_cadeirasPrefabs.Length > 0)
        {
            int _chairPrefabIndex = UnityEngine.Random.Range(0, _cadeirasPrefabs.Length);
            InitialChair = Instantiate(_cadeirasPrefabs[_chairPrefabIndex], transform);
            // :)
            // _sonReact.transform.localPosition += new Vector3(0, 0.5f, 1.5f);
            _gameManager.AddChair(InitialChair);
        }
    }

    private IEnumerator MexeTe()
    {
        float delta = 0;
        do
        {
            yield return null;

            delta += Time.deltaTime;
            SetMaterialAlpha(delta / _maxStopTime);
        }
        while (delta <= _maxStopTime);

        _audioSource.PlayOneShot(_burstedSound);

        Vector3 velocity = _rigidbody.velocity;
        velocity.y = _fogoNoCuForce.y;

        float randomX = UnityEngine.Random.Range(-1f, 1f);
        float randomZ = UnityEngine.Random.Range(-1f, 1f);
        velocity.x = _fogoNoCuForce.x * randomX;
        velocity.z = _fogoNoCuForce.x * randomZ;

        _rigidbody.velocity = velocity;

        SetMaterialAlpha(0);

        StartCoroutine(NaoTeMexesAgora());
    }

    private void SetMaterialAlpha(float alpha)
    {
        foreach (Renderer renderer in _sonReact.Renderers)
        {
            if (renderer.materials.Length >= 2)
            {
                Color color = renderer.materials[1].GetColor("_TintColor");
                color.a = alpha;
                renderer.materials[1].SetColor("_TintColor", color);
            }
        }
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
        if (collision.gameObject.TryGetComponent(out PlayerController other))
        {
            if (IsDashing)
            {
                other.GetPushed(transform.forward);

                if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);
                _sonReact.FinishDashAnimation();
                IsDashing = false;
            }
        }
    }

    private IEnumerator GoDash()
    {
        _audioSource.PlayOneShot(_dashSound);

        Vector3 velocity = transform.forward * _dashForce;
        velocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = velocity;

        _sonReact.DoDashAnimation();
        IsDashing = true;

        yield return new WaitForSeconds(1);

        _sonReact.FinishDashAnimation();
        IsDashing = false;
    }

    public void GetPushed(Vector3 direction)
    {
        if (_dashCoroutine != null) StopCoroutine(_dashCoroutine);
        _dashCoroutine = StartCoroutine(GetPushedRoutine(direction));
    }

    private IEnumerator GetPushedRoutine(Vector3 direction)
    {
        _audioSource.PlayOneShot(_hittedSound);

        Vector3 velocity = direction * _dashForce;

        if (IsDashing)
        {
            _rigidbody.velocity += velocity;
            IsDashing = false;
        }
        else
        {
            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;

            _sonReact.DoDashAnimation();
        }

        IsPushed = true;
        yield return new WaitForSeconds(1);
        _sonReact.FinishDashAnimation();
        IsPushed = false;
    }
}
