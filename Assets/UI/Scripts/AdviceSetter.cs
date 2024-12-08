using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdviceSetter : MonoBehaviour
{
    [SerializeField]
    private BearActivityManager _bearManager;

    [SerializeField]
    private BreakeableObject _apire;

    [SerializeField]
    private ConstructionObject _bridge;

    [SerializeField]
    private Storage _storage;

    Dictionary<Func<bool>, string> CondAdvicePairs;

    private AdviceField _advice;

    private SceneChanger _sceneChanger = new();

    private Vector3 _basePosition;
    private float _baseRotation;

    private bool _layOut;
    private bool _attack;
    private bool _broke;
    private bool _build;

    private void Start()
    {
        _basePosition = _bearManager.transform.position;
        _baseRotation = _bearManager.transform.rotation.y;

        _bearManager.OnHotKeySelect += () => _basePosition = _bearManager.transform.position;

        _apire.AddListerForHit(AttackApire);
        _apire.AddListnerForDeath(BrokeApire);
        _bridge.AddListnerToBuild(BuildBridge);
        _storage.OnLayOutItems.AddListener(LayOut);

        _advice = GetComponent<AdviceField>();

        CondAdvicePairs = new Dictionary<Func<bool>, string>()
        {
            {
                () => _bearManager.SelectedUnits.Any(),
                "������� ��� � ����� �����, ����� ������� ����� �������������"
            },
            {
                () => _bearManager.Bears.Any(b => b.HasPath),
                "������� �� ������� ����� � �������� ����, ����� ���������� ������"
            },
            {
                () => _basePosition != _bearManager.transform.position,
                "����� ALT + ���, ����� ������� ������"
            },
            {
                () => _baseRotation != _bearManager.transform.rotation.y,
                "������ �������, ������� �� ������ ��� ������, ����� ������ ��� ��������"
            },
            {
                () => _bearManager.Bears.Any(b => !b.HasPath && b.Behaviour.Target is ResourceObjectSpawner),
                "��� ������ � ��������� ������� �������� �������, �� ������� ������� �� �� ����� (����������� � ��������� ������), ����� �� ���� ������ ������� ����"
            },
            {
                () => _layOut,
                "������� TAB, ����� ������� ���������� � �������� �� ������ � �������� ��������"
            },
            {
                () => Input.GetKey(KeyCode.Tab),
                "����� ��������� ����, ���� �������� ������� �� �����, � ����� ������� � �������, �� ������� ���������� ������� ��� ������������, � ������ �� ��� ���"
            },
            {
                () => _build,
                "�������� �����-����, ����� �� ����"
            },
            {
                () => _attack,
                "���������, ���� ���� � �� ����� �������, ������ ���� ������� �����-���!"
            },
            {
                () => _broke,
                "����� ����� ������������� �� ������ �������, ����� ����������� ��������. ��� ����� ����� ������ TAB, ������� ������� � ��������������, � ����� ������ \"���������\""
            },
            {
                () => _bearManager.Bears.Any(b =>
                {
                    bool attack = b.AttackLevel > 0;
                    bool strranght = b.StrenghtLevel > 0;
                    bool health = b.HealthLevel > 0;

                    return attack || strranght || health;
                }),
                "������� ����!"
            }

        };

        StartCoroutine(StartGiveTask());
    }

    private System.Collections.IEnumerator StartGiveTask()
    {
        foreach (var pair in CondAdvicePairs)
        {
            while (!pair.Key.Invoke())
                yield return null;
            _advice.SetAdvice(pair.Value);
        }
        yield return new WaitForSeconds(10);
        _sceneChanger.ExitToMenu();
    }

    private void LayOut(Storage _)
        => _layOut = true;

    private void AttackApire()
        => _attack = true;

    private void BrokeApire()
        => _broke = true;

    private void BuildBridge()
        => _build = true;

}