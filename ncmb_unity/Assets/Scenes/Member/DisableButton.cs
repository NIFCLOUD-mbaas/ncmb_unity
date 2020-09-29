using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{

	public Button disableAdsButton;
	bool check = false;
	// Use this for initialization
	void Start()
	{
		disableAdsButton.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void MenuOpen()
	{
		if (!check)
		{
			disableAdsButton.gameObject.SetActive(true);
			check = true;
		}
		else
		{
			disableAdsButton.gameObject.SetActive(false);
			check = false;
		}
	}
}
