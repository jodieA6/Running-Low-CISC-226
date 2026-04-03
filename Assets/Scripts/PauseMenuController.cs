using Unity.VisualScripting;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] FadeOutController fadeController;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMainMenu()
    {
        fadeController.ReturnToMainMenu();
    }

    public void ResumeGame()
    {
        gameManager.TogglePause();
    }

    public void RestartLevel()
    {
        fadeController.FadeToScene(STATIC_DATA.CURRENT_LEVEL);
    }
}
