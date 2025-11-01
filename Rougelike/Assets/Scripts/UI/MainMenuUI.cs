using System.Collections;
using System.Collections.Generic;
using TMPro;
using tuleeeeee.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{

    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject quitButton;

    [SerializeField] private GameObject coopButton;

    [SerializeField] private TextMeshProUGUI versionText;

    private bool isChosenCharacter;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(playButton);

        coopButton.SetActive(false);

        isChosenCharacter = false;

        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        versionText.SetText(Application.version.ToString());
    }
    public void LoadCharacterSelector()
    {
        SetMainMenuButtonsActive(true);

        coopButton.SetActive(true);

        isChosenCharacter = true;

        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }
    public void UnLoadOptions()
    {
        SetMainMenuButtonsActive(true);

        if (isChosenCharacter)
        {
            coopButton.SetActive(true);
            SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
        }
    }
    public void LoadOptions()
    {
        SetMainMenuButtonsActive(false);

        coopButton.SetActive(false);


        if (isChosenCharacter)
        {
            SceneManager.UnloadSceneAsync("CharacterSelectorScene");
        }
    }
    public void PlayGame()
    {
        GlobalState.isCoop = false;

        TextMeshProUGUI playText = playButton.GetComponentInChildren<TextMeshProUGUI>();
        playText.SetText($"Single-Play");

        coopButton.SetActive(true);

        if (isChosenCharacter)
        {
            SceneManager.LoadScene("MainGameScene");
        }
        else
        {
            LoadCharacterSelector();
        }
    }
    public void Coop()
    {
        GlobalState.isCoop = true;
        SceneManager.LoadScene("MainGameScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void SetMainMenuButtonsActive(bool isActive)
    {
        playButton.SetActive(isActive);
        optionsButton.SetActive(isActive);
        quitButton.SetActive(isActive);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playButton), playButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(coopButton), coopButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(optionsButton), optionsButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(quitButton), quitButton);
        HelperUtilities.ValidateCheckNullValue(this, nameof(versionText), versionText);
    }
#endif
    #endregion
}
