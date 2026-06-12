using UnityEngine;

public class CheatUI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiToToggles0;
    [SerializeField] private GameObject[] uiToToggles1;
    [SerializeField] private GameObject[] uiToToggles2;
    [SerializeField] private GameObject[] uiToToggles3;
    [SerializeField] private GameObject[] uiToToggles4;

    void Start()
    {
        gameObject.SetActive(GlobalController.Instance.HaveCheatUI);
    }

    public void ToggleUI(int index)
    {
        switch (index)
        {
            case 0:
                for (int i = 0; i < uiToToggles0.Length; i++)
                {
                    uiToToggles0[i].SetActive(!uiToToggles0[i].activeInHierarchy);
                }
                break;
            case 1:
                for (int i = 0; i < uiToToggles1.Length; i++)
                {
                    uiToToggles1[i].SetActive(!uiToToggles1[i].activeInHierarchy);
                }
                break;
            case 2:
                for (int i = 0; i < uiToToggles2.Length; i++)
                {
                    uiToToggles2[i].SetActive(!uiToToggles2[i].activeInHierarchy);
                }
                break;
            case 3:
                for (int i = 0; i < uiToToggles3.Length; i++)
                {
                    uiToToggles3[i].SetActive(!uiToToggles3[i].activeInHierarchy);
                }
                break;
            case 4:
                for (int i = 0; i < uiToToggles4.Length; i++)
                {
                    uiToToggles4[i].SetActive(!uiToToggles4[i].activeInHierarchy);
                }
                break;
            default:
                break;
        }
    }
}
