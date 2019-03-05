using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;

[RequireComponent(typeof(Button))]
public class OpenJsonFileBrowser : MonoBehaviour, IPointerDownHandler {

    public GameObject JsonManager;
    JsonDataImport JDI;

    public void OnPointerDown(PointerEventData eventData) { }

	// Use this for initialization
	void Start () {
        JDI = JsonManager.GetComponent<JsonDataImport>();
        var button = GetComponent<Button>();
        button.onClick.AddListener(onClick);
	}
	
	private void onClick()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "json", false);
        string FileLocation = new System.Uri(paths[0]).AbsoluteUri;
        JDI.localpath = FileLocation;
    }
}
