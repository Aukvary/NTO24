using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class InteractingBehaviour : EntityComponent
{
    [SerializeField]
    private UnityEvent<Entity> _onChangeTargetEvent;

    [SerializeField]
    private UnityEvent _onInteractFailedEvent;

    [SerializeField]
    private UnityEvent<bool> _onPossibilityInteractChangeEvent;

    [SerializeField]
    private float _interactRange;

    private EntityStat _rangeStat;

    private Entity _target;

    private bool _canInteract;

    public Entity Target
    {
        get => _target;

        set
        {
            _target = value;

            _onChangeTargetEvent.Invoke(value);
        }
    }

    public Vector3 TargetPosition => Target.transform.position;

    public bool CanInteract
    {
        get
        {
            if (!Target)
            {
                if (_canInteract)
                {
                    _canInteract = false;
                    _onPossibilityInteractChangeEvent.Invoke(false);
                }
                return false;
            }

            Ray ray = new(transform.position, TargetPosition - transform.position);

            var hit = Physics.RaycastAll(ray).First(h => h.transform == Target.transform);

            bool can = Vector3.Distance(transform.position, hit.point) < Range;

            if (can != _canInteract)
            { 
                _canInteract = can; 
                _onPossibilityInteractChangeEvent.Invoke(can); 
            }

            return can;
        }
    }

    public float Range 
        => Target.TryGetComponent<IHealthable>(out _) ? _rangeStat.StatValue : _interactRange;

    protected override void Awake()
    {
        base.Awake();
        _rangeStat = Entity.GetComponent<IStatsable>()[EntityStatsType.AttackRange];

        if (Entity is Unit unit)
            unit.BehaviourAnimation.OnPunchAnimationEvent += TryInteract;
    }

    public void TryInteract()
    {
        if (!Target.Interactable)
        {
            Target = null;
            return;
        }
        else if (!CanInteract)
        {
            Entity.InteractBy(Target);
            _onInteractFailedEvent.Invoke();
            return;
        }
        Target.InteractBy(Entity);
    }

    public void AddOnTargetChangeAction(UnityAction<Entity> action)
        => _onChangeTargetEvent.AddListener(action);
    public void RemoveOnTargetChangeAction(UnityAction<Entity> action)
        => _onChangeTargetEvent.RemoveListener(action);

    public void AddOnInteractFailedAction(UnityAction action)
        => _onInteractFailedEvent.AddListener(action);
    public void RemoveOnInteractFailedAction(UnityAction action)
        => _onInteractFailedEvent.RemoveListener(action);

    public void AddnPossibilityInteractChangeAction(UnityAction<bool> action)
        => _onPossibilityInteractChangeEvent.AddListener(action);
    public void RemoveOnInteractFailedAction(UnityAction<bool> action)
        => _onPossibilityInteractChangeEvent.RemoveListener(action);
}