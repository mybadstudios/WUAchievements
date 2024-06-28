//WordPress For Unity Bridge: Achievements © 2024 by Ryunosuke Jansen is licensed under CC BY-ND 4.0.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MBS;
using System.Threading.Tasks;

public class WUAModKeys : MonoBehaviour {

    public InputField keyname;
    public WUADisplay demo_panel;
    public Transform toggle_parent;

    private void Start()
    {
        WPServer.OnServerStateChange += InspectState;
        WULogin.OnLoggedIn += FetchAwards;
    }
    
    void InspectState(WPServerState state) => gameObject.SetActive(state == WPServerState.None);
    void FetchAwards( CML response ) => WUAchieve.FetchEverything( GenerateEntries );

    public void GetKey() => demo_panel.UpdateKeys( keyname.text.Trim(), 1 );
    public void LooseKey() => demo_panel.UpdateKeys( keyname.text.Trim(), -1 );

    async void GenerateEntries( CML response )
    {
        //first, let's find all the Achievements
        List<CMLData> entries = response.Children( 0 );

        //make sure the object is active or else we can't use coroutines...
        while(!gameObject.activeSelf)
            await Task.Delay( 50 );

        //now let's create a button for each of them
        foreach ( CMLData entry in entries )
            StartCoroutine( SpawnButton( entry.String( "locked_url" ), entry.Int( "aid" ) ) );
    }

    IEnumerator SpawnButton( string url, int aid)
    {
        Sprite img = null;
        using (var uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (string.IsNullOrEmpty(uwr.error))
            {
                var tex = DownloadHandlerTexture.GetContent(uwr);
                if (tex)
                {
                    Rect size = new Rect(0, 0, tex.width, tex.height);
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    img = Sprite.Create(tex, size, pivot);
                }
                WUAToggleAchievement ta = WUAToggleAchievement.SpawnInstance(aid, demo_panel, toggle_parent);
                ta.SetIcon(img);
            }
        }
    }

}
