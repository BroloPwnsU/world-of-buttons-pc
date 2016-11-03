using UnityEngine;
using System.Collections;

public interface IGamePanel
{
    bool Show();
    bool Hide();
}

public abstract class GamePanel : MonoBehaviour
{
    public virtual void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public virtual void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}