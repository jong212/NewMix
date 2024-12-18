using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Transform _layout_TopLeft;
    [SerializeField] private Transform _layout_TopRight;
    [SerializeField] private Transform _layout_BottomLeft;
    [SerializeField] private Transform _layout_BottomRight;
    [SerializeField] private List<MonExpHpMpContainer> _MonUIList;
    public Transform Layout_TopLeft => _layout_TopLeft;
    public Transform Layout_TopRight => _layout_TopRight;
    public Transform Layout_BottomLeft => _layout_BottomLeft;
    public Transform Layout_BottomRight => _layout_BottomRight;
    public List<MonExpHpMpContainer> MonUIList => _MonUIList;


}
