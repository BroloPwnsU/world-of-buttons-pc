using UnityEngine;
using System.Collections;

public interface IGamePanel
{
    bool Show();
    bool Hide();
}

public abstract class GamePanel : MonoBehaviour
{
    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}