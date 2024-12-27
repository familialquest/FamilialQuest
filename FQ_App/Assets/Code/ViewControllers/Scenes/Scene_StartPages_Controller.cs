using Ricimi;
using UnityEngine;

public class Scene_StartPages_Controller : MonoBehaviour
{
    public PopupOpener LoginPopup;
    bool isFirebaseInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        LoginPopup.OpenPopup();        
    }
}
