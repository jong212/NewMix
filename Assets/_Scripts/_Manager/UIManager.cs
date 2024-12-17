using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public enum UIType
{
    BackEndName,
    CharaterUI,
    NickPanel,
    BtnAttack
}

public class UIManager : MonoBehaviour
{
    [Header("------ Common UI Prefabs -----")]

    [SerializeField] private AlertUI _alertUI;
    [SerializeField] private ConfirmUI _confirmUI;
    [SerializeField] private InventoryManager _inventoryUI;    
    [SerializeField] private MonsterInventoryManager _monsterInvenManager;
    [SerializeField] private GameObject _loading;
    [SerializeField] private EnemyInfoUI _enemyInfoUI;
    [SerializeField] private DamageTextPoolManager _damagePoolManager;
    [SerializeField] private DropItemPoolManager _dropItemPoolMaanger;
    [SerializeField] private ExpHpMpContainer _ExpHpMpContainer;
    [SerializeField] private VariableJoystick _joystick;
    [SerializeField] private MainUI _mainUI;


    private Dictionary<UIType, GameObject> _createdUIDic = new Dictionary<UIType, GameObject>(); // _createdUIDic 딕셔너리에 있으면 하이어라키에 존재한단 뜻    
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>(); // _openedUIDic 여기 담겨있으면 SetActive True인 것임
    
    [Header("------ UI Prefabs -----")]
    [SerializeField] private WorldNickname _worldNicknameUI;

    // Popup
    public AlertUI AlertUI
    {
        get
        {
            return _alertUI;
        }
    }
    public ConfirmUI ConfirmUI
    {
        get
        {
            return _confirmUI;
        }
    }
    public MainUI MainUI
    {
        get
        {
            return _mainUI;
        }
    }
    public DamageTextPoolManager DamagePoolUI
    {
        get
        {
            return _damagePoolManager;
        }
    }
    public DropItemPoolManager DropItemPoolManager
    {
        get
        {
            return _dropItemPoolMaanger;
        }
    }    
    public ExpHpMpContainer ExpHpMpContainer
    {
        get
        {
            return _ExpHpMpContainer;
        }
    }
    public InventoryManager ContentsInventoryUI
    {
        get
        {
            return _inventoryUI;
        }
    }
    public MonsterInventoryManager MonsterInventoryManagerUI
    {
        get
        {
            return _monsterInvenManager;
        }
    }
    public VariableJoystick VariableJoystick
    {
        get
        {
            return _joystick;
        }
    }
    public GameObject Loading
    {
        get
        {
            return _loading;
        }
    }
    public WorldNickname WorldNickNameUI
    {
        get
        {
            return _worldNicknameUI;
        }
    }
    public EnemyInfoUI EnemyInfoUI
    {
        get
        {
            return _enemyInfoUI;
        }
    }
    private void Awake()
    {
        if(AlertUI == null || ConfirmUI == null || MainUI == null || ContentsInventoryUI == null || WorldNickNameUI == null || EnemyInfoUI == null)
        {
            Debug.LogError("UIManager Awake : Component 누락");
        }
    }
    // Init
    public void Init()
    {
        AlertUI.gameObject.SetActive(false);
        ConfirmUI.gameObject.SetActive(false);
        _enemyInfoUI.gameObject.SetActive(false);
    }

    // Common ( Try UI Open )
    public void CommonOpen(UIType uiType , Transform parent, bool worldPositionStays, UnityAction callback = null)           
    {
        var gObj = GetCreatedUI(uiType,  parent, worldPositionStays);

        if (gObj != null)
        {
            OpenUI(uiType, gObj);
            if (callback != null)
            {
                RegisterButtonCallback(gObj, callback);
            }
        }
    }

    // Common ( Try UI Open ) 오픈한 오브젝트 반환
    public GameObject CommonOpen(UIType uiType, Transform parent, bool worldPositionStays, bool returnObj , UnityAction callback = null)
    {
        var gObj = GetCreatedUI(uiType, parent, worldPositionStays);

        if (gObj != null)
        {
            OpenUI(uiType, gObj);
            if (callback != null)
            {
                RegisterButtonCallback(gObj, callback);
            }
        }
        return returnObj ? gObj : null;
    }

    private GameObject GetCreatedUI(UIType uiType, Transform parent, bool worldPositionStays)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiType, parent, worldPositionStays);
        }
        return _createdUIDic[uiType];
    }

    private void CreateUI(UIType uiType, Transform parent, bool worldPositionStays)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = GetUIPath(uiType);

            GameObject loadedObj = (GameObject)Resources.Load(path);
            GameObject gObj = Instantiate(loadedObj, parent, worldPositionStays);

            gObj.transform.localPosition = loadedObj.transform.localPosition;
            gObj.transform.localRotation = loadedObj.transform.localRotation;

            if (gObj != null)
            {
                _createdUIDic.Add(uiType, gObj);
            }
        }
    }

    private string GetUIPath(UIType uiType)
    {
        string path = string.Empty; 
        switch (uiType)
        {
            case UIType.BackEndName:
                path = "Prefabs/LoginScene/UI/BackEndSetName";
                break;
            case UIType.CharaterUI:
                path = "Prefabs/LoginScene/UI/CharaterUI";
                break;            
            case UIType.NickPanel:
                path = "Prefabs/LoginScene/UI/WorldNickname";
                break;            
            case UIType.BtnAttack:
                path = "Prefabs/LoginScene/UI/BtnAttack";
                break;
        }
        return path;
    }

    private void OpenUI(UIType uiType, GameObject uiObject)
    {
        if (_openedUIDic.Contains(uiType) == false)
        {
            uiObject.SetActive(true);
            _openedUIDic.Add(uiType);
        }
    }

    public void CloseUI(UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var uiObject = _createdUIDic[uiType];
            uiObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }

    void RegisterButtonCallback (GameObject obj, UnityAction action)
    {
        string baseName = obj.name.Replace("(Clone)", "").Trim();

        switch (baseName)
        {
            case nameof(UIType.BtnAttack) : {
                Button attackButton = obj.GetComponent<Button>();
                if (attackButton != null)
                {
                    attackButton.onClick.AddListener(action);
                }
                    break;
                }
        }

    }
}
