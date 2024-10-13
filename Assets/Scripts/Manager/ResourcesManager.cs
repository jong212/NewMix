using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    #region Singleton

    public static ResourcesManager instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("Instance already exists!");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

	#endregion

	[Header("------ Resource Banks -----")]
	public ResourceBank ingredientBank;

	[Header("------ UI Prefabs -----")]
    public BarUI barUIPrefab;
    public ItemCompleteWorldUI cookCompleteSuccessUI;
    public ItemCompleteWorldUI burnFoodUI;
    public FoodOrderItemUI foodOrderUIPrefab;
    public FoodOrderIngredientUI ingredientUIPrefab;
	public WorldIngredientItemUI worldIngredientItemUIPrefab;
	public WorldIngredientContainerUI worldIngredientContainerUIPrefab;
    public WorldNickname worldNicknamePrefab;

	private void Start()
	{
		IngredientGraph.Prepare();
		AssemblyMap.Prepare();
        ProcessGraph.Prepare();
	}
}
