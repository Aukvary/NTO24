using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitiesActivityManager : MonoBehaviour
{
    [SerializeField]
    private Color _selectingAreaColor;

    private UnitList _unitList;

    private HashSet<Unit> _controlledUnits = new();


    private Vector3 _startMousePos;
    private Vector3 _endMousePos;

    private Vector2 _zero = new(2000, 0);
    private Ray direction => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Awake()
    {
        _unitList = GetComponent<UnitList>();
    }

    private void Update()
    {
        SetUnitTask();
        SelectUnitGroup();
        SelectAloneUnit();
    }
    private void SetUnitTask()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse1) || !_controlledUnits.Any())
            return;

        if (Physics.Raycast(direction, out RaycastHit hit, LayerMask.GetMask("Ground")))
        {
            foreach (var unit in _controlledUnits)
                unit.MoveTo(hit.point);
        }
    }


    private void SelectAloneUnit()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.LeftShift))
            return;

        if (!Physics.Raycast(direction, out RaycastHit hit))
            return;

        _controlledUnits.Clear();

        if (!hit.transform.TryGetComponent<Unit>(out var unit))
            return;

        _controlledUnits.Add(unit);

    }

    private void SelectUnitGroup()
    {
        if (_startMousePos == _endMousePos) 
            return;
        Vector3 v1 = Camera.main.ScreenToViewportPoint(_startMousePos);
        Vector3 v2 = Camera.main.ScreenToViewportPoint(_endMousePos);

        Vector3 minOffset = Vector3.Min(v1, v2);
        Vector3 maxOffset = Vector3.Max(v1, v2);

        minOffset.z = Camera.main.nearClipPlane;
        maxOffset.z = Camera.main.farClipPlane;

        Bounds bounds = new Bounds();
        bounds.SetMinMax(minOffset, maxOffset);

        foreach (var unit in _unitList.AllUnits)
        {
            if (!bounds.Contains(Camera.main.WorldToViewportPoint(unit.transform.position)))
                continue;

            _controlledUnits.Add(unit);
        }
    }

    private void OnGUI()
    {
        if (_startMousePos != _endMousePos && !Input.GetKey(KeyCode.Mouse0))
        {
            _startMousePos = _endMousePos = Vector3.zero;
            return;
        }
        if (!Input.GetKey(KeyCode.LeftShift))
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _startMousePos = Input.mousePosition;
            _startMousePos.y = Screen.height - _startMousePos.y;

        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            _endMousePos = Input.mousePosition;
            _endMousePos.y = Screen.height - _endMousePos.y;
        }

        var topLeft = Vector3.Min(_startMousePos, _endMousePos);
        var bottomRight = Vector3.Max(_startMousePos, _endMousePos);

        var rect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);


        Color areaColor = _selectingAreaColor;
        areaColor.a = 0.25f;
        UIUtils.DrawSelectArea(rect, areaColor, _selectingAreaColor, 2);
    }
}
