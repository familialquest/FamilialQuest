using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMainRightController : MonoBehaviour
{
    public GameObject Button_MenuExpand;

    private Animator arrowAnimator;
    private Animator menuAnimator;

    private bool isExpanded = false;
    // Start is called before the first frame update
    void Start()
    {
        arrowAnimator = Button_MenuExpand.GetComponent<Animator>();
        menuAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick_ButtonExpand()
    {
        ExpandUnexpandMenu();
    }

    public void OnClick_AnyMenuButton()
    {
        UnexpandMenu();
    }

    private void ExpandUnexpandMenu()
    {
        if (isExpanded)
        {
            UnexpandMenu();
        }
        else
        {
            ExpandMenu();
        }
    }

    private void UnexpandMenu()
    {
        if (!isExpanded)
            return;
        
        arrowAnimator.SetBool("Roll", !isExpanded);
        menuAnimator.SetBool("Roll", !isExpanded);

        isExpanded = !isExpanded;
    }

    private void ExpandMenu()
    {
        if (isExpanded)
            return;

        arrowAnimator.SetBool("Roll", !isExpanded);
        menuAnimator.SetBool("Roll", !isExpanded);

        isExpanded = !isExpanded;
    }
}
