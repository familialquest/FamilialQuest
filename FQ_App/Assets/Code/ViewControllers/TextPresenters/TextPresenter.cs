using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITextPresenter 
{
    Dictionary<string, string> Present(Dictionary<string, object> data);
}
