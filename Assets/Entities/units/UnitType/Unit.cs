using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    #region SerializeStats
    [Header("Stats")]

    [SerializeField, Min(0f)]
    private float _speed;

    [SerializeField, Min(0f)]
    private float _attackRange;

    [SerializeField, Min(0f)]
    private float _attackAngle;

    [SerializeField]
    private List<float> _strength;

    [SerializeField, Min(0f)]
    private List<float> _damage;

    [SerializeField, Min(0f)]
    private List<float> _maxHealth;

    [SerializeField, Min(0f)]
    private List<float> _regeneration;

    [SerializeField]
    private float _restoreTime;

    [SerializeField]
    private bool _isBee;
    #endregion

    [SerializeField]
    private Sprite _headSprite;

    private bool _alive = true;
    private float _health;

    #region UnitComponentsAlive
    private Collider _collider;
    private NavMeshAgent _navMeshAgent;
    private MeshRenderer[] _meshRenderers;
    #endregion

    #region AnyComponents
    private Animator _animator;
    private BehaviourAnimation _behaviourAnimation;
    #endregion

    #region UnitBehaviour
    private BuildBehaviour _buildBehaviour;
    private UnitExtractionController _extractionController;
    private UnitMovementController _moveController;
    private AttackBehaviour _attackBehaviour;
    private UnitBehaviour _behavior;
    private Inventory _inventory;
    #endregion

    private Vector3 _spawnPosition;


    #region Levels
    private int _attackLevel;
    private int _strenghtLevel;
    private int _healthLevel;
    #endregion

    public UnitBehaviour Behaviour
    {
        get => _behavior;
        set
        {
            if (Behaviour == value)
                return;

            if (value == null)
                Animator.SetTrigger("idle");
            Behaviour?.BehaviourExit();
            value?.BehaviourEnter();
            _behavior = value;
        }
    }

    #region StatsProperty
    public float Damage => _damage[AttackLevel];
    public float Strength => _strength[StrenghtLevel];
    public float MaxHealth => _maxHealth[HealthLevel];
    public float Regeneration => _regeneration[_healthLevel];

    public float Speed => _speed;
    #endregion 

    public float Health
    {
        get => _health;

        set
        {
            _health = Mathf.Clamp(value, 0, MaxHealth);
            OnHealthChangeEvent?.Invoke(this);
            if (_health == 0f)
            {
                if (IsBee)
                    Destroy(gameObject);
                else
                    StartCoroutine(StartRestore());
            }
        }
    }

    public bool Alive
    {
        get => _alive;

        set
        {
            _alive = value;
            transform.position = _spawnPosition;
            if (value)
                Health = MaxHealth;

            foreach (var item in _meshRenderers)
            {
                item.enabled = value;
            }
            _navMeshAgent.enabled = value;
            _collider.enabled = value;
        }
    }

    public Inventory Inventory => _inventory;
    public Sprite HeadSprite => _headSprite;

    public BehaviourAnimation BehaviourAnimation => _behaviourAnimation;

    public bool IsBee => _isBee;

    public Storage Storage { get; private set; }

    private Vector3 _position => transform.position;

    public int AttackLevel 
    {
        get => _attackLevel;

        set => _attackLevel = Math.Clamp(value, 0, 4);
    }
    public int StrenghtLevel
    {
        get => _strenghtLevel;
        set => _strenghtLevel = Math.Clamp(value, 0, 4);
    }
    public int HealthLevel
    {
        get => _healthLevel;

        set => _healthLevel = Math.Clamp(value, 0, 4);
    }

    public Animator Animator => _animator;


    public event Action<Unit> OnHealthChangeEvent;


    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _behaviourAnimation = GetComponentInChildren<BehaviourAnimation>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        _collider = GetComponent<Collider>();


        _health = MaxHealth;
        _behavior = _moveController;
        _moveController = new(this, _speed);
        _extractionController = new(this);
        _buildBehaviour = new(this);
        _attackBehaviour = new(this, _attackRange, _attackAngle);

        _inventory = new(this);
    }

    public Unit Spawn(Vector3 spawnPostion, Storage storage)
    {
        var newUnit = Instantiate(this, spawnPostion, Quaternion.identity);
        newUnit._spawnPosition = spawnPostion;
        newUnit.Storage = storage;

        return newUnit;
    }

    private void Update()
    {
        Behaviour?.BehaviourUpdate();
        if (Alive)
            Health += Regeneration * Time.deltaTime;
    }

    public void MoveTo(Vector3 newPostion)
    {
        _moveController.TargetPosition = newPostion;
    }

    public void Extract(ResourceObjectSpawner spawner)
    {
        _extractionController.Resource = spawner;
    }

    public void Attack(Unit unit)
    {
        _attackBehaviour.AttackedUnit = unit;
    }

    public void Attack(BreakeableObject throne)
    {

    }

    public void Build(ConstructionObject obj)
    {
        _buildBehaviour.Build = obj;
    }

    public void LayOutItems(Storage storage)
    {
        storage.Interact(this);
    }

    private IEnumerator StartRestore()
    {
        Alive = false;
        yield return new WaitForSeconds(_restoreTime);
        Alive = true;
    }

    public bool CanUpgrade(UpgradeType type) => type switch
    {
        UpgradeType.Damage => AttackLevel != 4,
        UpgradeType.Strenght => StrenghtLevel != 4,
        UpgradeType.Health => HealthLevel != 4,
        _ => false,
    };

    public void Upgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                AttackLevel += 1;
                break;

            case UpgradeType.Strenght:
                StrenghtLevel += 1;
                break;

            case UpgradeType.Health:
                HealthLevel += 1;
                break;
        }
    }
}