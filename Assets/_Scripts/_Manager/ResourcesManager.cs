using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    #region Singleton

    public static ResourcesManager instance;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

	#endregion

	[Header("------ UI Prefabs -----")]
    public BarUI barUIPrefab;
    public ItemCompleteWorldUI cookCompleteSuccessUI;
    public ItemCompleteWorldUI burnFoodUI;
    public FoodOrderIngredientUI ingredientUIPrefab;
	public WorldIngredientItemUI worldIngredientItemUIPrefab;
	public WorldIngredientContainerUI worldIngredientContainerUIPrefab;
    public WorldNickname worldNicknamePrefab;

}
