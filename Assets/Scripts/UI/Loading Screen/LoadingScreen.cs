using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public LevelManager LevelManager;
    public GameObject loadingScreenUI;
    public BackgroundImageManager imageManager;
    public Image background;
    public Slider loadingbar;
    public TMP_Text lblPercentage;
    public TMP_Text lblLoadingText;
    string percentageFormat;
    string loadingTextFormat;

    private void Start()
    {
        percentageFormat = lblPercentage.text;
        loadingTextFormat = lblLoadingText.text;
        background.sprite = imageManager.GetRandomImage();
    }

    // Update is called once per frame
    void Update()
    {
        loadingbar.value = LevelManager.loadingPercentage;
        lblPercentage.text = string.Format(percentageFormat, LevelManager.loadingPercentage * 100);
        //lblLoadingText.text = string.Format(loadingTextFormat, LevelManager.cityParent.loadingState);
    }
}
