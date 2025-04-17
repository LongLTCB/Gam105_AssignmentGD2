using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
  public void PLAY()
    {
        SceneManager.LoadScene(1);
    }    
   public void ExitMenu()
    {
        SceneManager.LoadScene(0);
    }    
    public void Exit()
    {
        Application.Quit();
    }    
}
