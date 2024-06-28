using UnityEngine;
using MBS;

public class DemoScript : MonoBehaviour {

    public string username, password;

    void Start () {
        CMLData credentials = new CMLData();
        credentials.Set( "username", username );
        credentials.Set( "password", password );
        WULogin.AttemptToLogin( credentials );
	}	
}
