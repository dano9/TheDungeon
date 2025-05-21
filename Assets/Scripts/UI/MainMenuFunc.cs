using UnityEngine;
using UnityEngine.UI;

public class MainMenuFunc : MonoBehaviour
{
    public bool hasASavedGame;

    public int buttonSpacing;
    public UIForm mainMenuForm;
    public RectTransform startNewGameButton;
    public RectTransform selectSaveButton;
    public RectTransform continueGameButton;
    public RectTransform settingsButton;
    public RectTransform quitGameButton;
    void Start()
    {
        EnterMainMenu();
    }
    public void StartGame()
    {

    }
    public void EnterMainMenu()
    {
        mainMenuForm.ActivateForm();
        int newGameLvl = 0; int selSaveLvl = -1; int contGLvl = -1; int settLvl = 1; int quitLvl = 2;
        if (hasASavedGame) { newGameLvl = -1;  selSaveLvl = 1;  contGLvl = 0;  settLvl = 2;  quitLvl = 3; UIMaster.main.SetSelectedElement(mainMenuForm.elements[2]); }
        else {UIMaster.main.SetSelectedElement(mainMenuForm.elements[0]); }

        startNewGameButton.gameObject.SetActive(newGameLvl != -1);
        startNewGameButton.localPosition = new Vector2(startNewGameButton.localPosition.x, buttonSpacing * -newGameLvl);
        
        selectSaveButton.gameObject.SetActive(selSaveLvl != -1);
        selectSaveButton.localPosition = new Vector2(selectSaveButton.localPosition.x, buttonSpacing * -selSaveLvl);

        continueGameButton.gameObject.SetActive(contGLvl != -1);
        continueGameButton.localPosition = new Vector2(continueGameButton.localPosition.x, buttonSpacing * -contGLvl);

        settingsButton.gameObject.SetActive(settLvl != -1);
        settingsButton.localPosition = new Vector2(settingsButton.localPosition.x, buttonSpacing * -settLvl);

        quitGameButton.gameObject.SetActive(quitLvl != -1);
        quitGameButton.localPosition = new Vector2(quitGameButton.localPosition.x, buttonSpacing * -quitLvl);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void EnterSaveSelectScreen()
    {

    }
}
