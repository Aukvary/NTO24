using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHUD : MonoBehaviour
{
    [SerializeField]
    private float _closedSpeed;

    [SerializeField]
    private Image _healthBar;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private TextMeshProUGUI _regenerationText;

    [SerializeField]
    private TextMeshProUGUI _damageText;

    [SerializeField]
    private TextMeshProUGUI _extractText;

    [SerializeField]
    private Image _bearHeadRenderer;

    [SerializeField]
    private RectTransform _hud;

    private Vector2 _minAnchor;
    private Vector2 _maxAnchor;

    private InventoryCellUI[] _inventoryCells = new InventoryCellUI[6];

    private Bear _unit;

    public Bear Unit
    {
        get => _unit;

        set
        {
            if (_unit != null)
            {
                _unit.Inventory.OnInventoryChanged -= UpdateHUD;
                _unit.OnHealthChangeEvent -= UpdateHUD;
            }

            if (value == null)
            {
                _healthBar.rectTransform.anchorMax = Vector2.right + Vector2.up;
                _healthText.text = "";
                return;
            }
            _unit = value;
            value.Inventory.OnInventoryChanged += UpdateHUD;
            value.OnHealthChangeEvent += UpdateHUD;
            value.OnLevelUpEvent += UpdateHUD;

            _bearHeadRenderer.sprite = value.HeadSprite;

            UpdateHUD(Unit);
        }
    }

    private void Awake()
    {
        _inventoryCells = GetComponentsInChildren<InventoryCellUI>();

        _minAnchor = _hud.anchorMin;
        _maxAnchor = _hud.anchorMax;

        _hud.anchorMax = _minAnchor;

        Unit = null;
    }

    private void Update()
    {
        if (Unit != null)
            _hud.anchorMax = Vector2.Lerp(
                _hud.anchorMax,
                _maxAnchor,
                Time.deltaTime * _closedSpeed);
        else
            _hud.anchorMax = Vector2.Lerp(
                _hud.anchorMax,
                _minAnchor,
                Time.deltaTime * _closedSpeed);
    }

    private void UpdateHUD(Unit unit)
    {
        if (unit is Bee)
            return;


        int i = 0;
        foreach (var cell in (unit as Bear).Inventory.Resources)
        {
            _inventoryCells[i].Cell = cell;
            i++;
        }

        _healthBar.rectTransform.anchorMax = new(Unit.Health / Unit.MaxHealth, 1);
        _healthText.text = $"{System.Math.Round(unit.Health, 1)} / {unit.MaxHealth}";
        _regenerationText.text = $"+{unit.Regeneration}";

        _damageText.text = Unit.Damage.ToString();
        _extractText.text = Unit.Strength.ToString();
    }
}