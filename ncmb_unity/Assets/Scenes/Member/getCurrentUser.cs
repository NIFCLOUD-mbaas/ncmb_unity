using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NCMB;

public class getCurrentUser : MonoBehaviour {

    public GameObject CurrentUser_Text = null; // Textオブジェクト
    NCMBUser currentUser;
    // 初期化
    void Start()
    {
    }

    // 更新
    void Update()
    {
        currentUser = NCMBUser.CurrentUser;
        // オブジェクトからTextコンポーネントを取得
        Text currentuser_text = CurrentUser_Text.GetComponent<Text>();
        if (currentUser != null)
        {
            // テキストの表示を入れ替える
            currentuser_text.text = "CurrentUser:" + currentUser.UserName;
        }
        else
        {
            // 未ログインまたは取得に失敗
            currentuser_text.text = "CurrentUser: nothing";
        }


    }
}


		
