using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NCMB;

//private InputField UserName;
//private InputField PassWord;

public class Loginsignin : MonoBehaviour
{
	public InputField UserName;
	public InputField PassWord;
    public GameObject Debug_Text = null; // Textオブジェクト

    // Use this for initialization
    void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public void Login ()
	{
		print (UserName.text);
		print (PassWord.text);

		//NCMBUserのインスタンス作成 
		NCMBUser user = new NCMBUser ();
        // オブジェクトからTextコンポーネントを取得
        Text debug_text = Debug_Text.GetComponent<Text>();
        // ユーザー名とパスワードでログイン
        NCMBUser.LogInAsync (UserName.text, PassWord.text, (NCMBException e) => {    
			if (e != null) {
				UnityEngine.Debug.Log ("ログインに失敗: " + e.ErrorMessage);
                debug_text.text = "ログインに失敗: " + e.ErrorMessage;
            }
            else {
				UnityEngine.Debug.Log ("ログインに成功！");
                // テキストの表示を入れ替える
                debug_text.text = "ログインに成功";
                //LogOutの部分は移動d先のScene名
                Application.LoadLevel ("LogOut");
			}
		});

	}

	public void Signin ()
	{
		print (UserName.text);
		print (PassWord.text);

		//NCMBUserのインスタンス作成 
		NCMBUser user = new NCMBUser ();
		
		//ユーザ名とパスワードの設定
		user.UserName = UserName.text;
		user.Password = PassWord.text;
        // オブジェクトからTextコンポーネントを取得
        Text debug_text = Debug_Text.GetComponent<Text>();
        //会員登録を行う
        user.SignUpAsync ((NCMBException e) => { 
			if (e != null) {
				UnityEngine.Debug.Log ("新規登録に失敗: " + e.ErrorMessage);
                debug_text.text = "新規登録に失敗: " + e.ErrorMessage;
            }
            else {
				UnityEngine.Debug.Log ("新規登録に成功");
                // テキストの表示を入れ替える
                debug_text.text = "新規登録に成功";
            }
		});

	}
}
