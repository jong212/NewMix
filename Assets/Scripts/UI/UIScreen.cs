using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CanvasGroup))]
public class UIScreen : MonoBehaviour
{
	public bool isModal = false;
	public Selectable initialSelected;
	public UnityEvent onFocused;
	public UnityEvent onDefocused;
	[ReadOnly] public UIScreen previousScreen = null;

	CanvasGroup _group = null;
	public CanvasGroup Group
	{
		get
		{
			if (_group) return _group;
			return _group = GetComponent<CanvasGroup>();
		}
	}

	public static UIScreen activeScreen;

	// Class Methods

	public static void HideActive()
	{
		activeScreen.gameObject.SetActive(false);
	}

	public static void ShowActive()
	{
		activeScreen.gameObject.SetActive(true);
	}

    public static void Focus(UIScreen screen) {
        if ( activeScreen )
            activeScreen.FocusScreen(screen);
        else
            screen.Focus();
    }

    public static void BackToInitial()
	{
		while (activeScreen?.previousScreen)
		{
			activeScreen.Back();
		}
	}

	public static void CloseAll()
	{
		while (activeScreen)
		{
			activeScreen.BackOrClose();
		}
	}


	// Instance Methods

	private void Awake()
	{
		if (activeScreen == null) Focus();
	}

	private void OnDestroy()
	{
		
		if (activeScreen == this)
		{
#if UNITY_EDITOR
			if (EditorApplication.isPlayingOrWillChangePlaymode == false) return;
#endif
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isLoaded == false)
			{
				activeScreen = null;
				return;
			}
			Debug.LogWarning($"Active UIScreen {this} is being destroyed");
			Back();
		}
	}

	public void FocusScreen(UIScreen screen)
	{
		if (screen == this)
		{
			Focus();
			return;
		}
        
		screen.previousScreen = this;
		screen.Focus();
		if (!screen.isModal)
		{
			Defocus();
		}
		else
		{
			Group.interactable = false;
		}
	}

	public void Focus()
	{
		Group.interactable = true;
		gameObject.SetActive(true);
		activeScreen = this;
		if (initialSelected)
		{
			initialSelected.Select();
		}
		if (onFocused != null) onFocused.Invoke();
	}

	public void Defocus()
	{
		gameObject.SetActive(false);
		if (onDefocused != null) onDefocused.Invoke();
	}

    // Equivalent to Back without enabling the previous screen
    public void Close()
    {
        Defocus();
        if (previousScreen)
        {
            Defocus();
            activeScreen = previousScreen;
            previousScreen = null;
        }
		else
		{
			activeScreen = null;
		}
    }

    public void Back()
	{
		if (previousScreen)
		{
			//Do some checks for if in-game?
				Defocus();
				previousScreen.Focus();
				previousScreen = null;
		}
	}

	public void BackOrClose()
	{
		Defocus();
		if (previousScreen)
		{
			previousScreen.Focus();
			previousScreen = null;
		}
		else
		{
			activeScreen = null;
		}
	}

	public void BackTo(UIScreen screen)
	{
		while (activeScreen != screen && activeScreen?.previousScreen)
		{
			activeScreen.Back();
		}

		if (activeScreen != screen)
		{
			Focus(screen);
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIScreen))]
public class UIScreenEditor : Editor
{
    private Button[] buttons;
	SerializedProperty _initialSelected;
	SerializedProperty _onFocused;
	SerializedProperty _onDefocused;

	private void OnEnable()
	{
		buttons = ((MonoBehaviour)target).GetComponentsInChildren<Button>();
		_initialSelected = serializedObject.FindProperty("initialSelected");
		_onFocused = serializedObject.FindProperty("onFocused");
		_onDefocused = serializedObject.FindProperty("onDefocused");
	}

	private void OnDisable()
	{
		buttons = null;
	}

	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();
		UIScreen scr = (UIScreen)target;
		EditorGUILayout.BeginHorizontal();
		scr.isModal = EditorGUILayout.ToggleLeft("Is Modal", scr.isModal, GUILayout.ExpandWidth(false));
		GUILayout.FlexibleSpace();
		GUILayout.Label(scr.gameObject.name, EditorStyles.boldLabel);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.ObjectField(_initialSelected);
		EditorGUILayout.PropertyField(_onFocused);
		EditorGUILayout.PropertyField(_onDefocused);
		serializedObject.ApplyModifiedProperties();
		if (buttons.Length > 0)
		{
			foreach (Button btn in buttons)
			{
				if (GUILayout.Button(btn.gameObject.name))
				{
					Selection.activeObject = btn;
				}
			}
		}
	}
}
#endif