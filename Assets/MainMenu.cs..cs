using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel; // Kéo thả panel Options vào đây trong Inspector.

    // Hàm gọi khi nhấn nút "Play"
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play Button Clicked!");
        SceneManager.LoadScene("GameScene"); // Đổi "GameScene" thành tên scene bạn muốn load.
    }

    // Hàm gọi khi nhấn nút "Options"
    public void OnOptionsButtonClicked()
    {
        Debug.Log("Options Button Clicked!");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true); // Hiển thị panel Options.
        }
    }

    // Hàm gọi khi nhấn nút "Exit"
    public void OnExitButtonClicked()
    {
        Debug.Log("Exit Button Clicked!");
        Application.Quit(); // Thoát game (chỉ hoạt động khi build game).
    }
}
