//WordPress For Unity Bridge: Achievements © 2024 by Ryunosuke Jansen is licensed under CC BY-ND 4.0.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace MBS {
	public class WUAView : MonoBehaviour {
        public CMLData Fields { get; set; } = null;
        /*
        Fields contain:
        int aid
        string name
        string text //unlocked text
        string descr //unlocking instructions
        string locked_url //path to icon if not in a Resources folder
        string unlocked_url //path to icon if not in a Resources folder
        string requirements //a comma delimited array of requirement strings in the format: TYPE NAME QTY
        */

        [SerializeField] Image icon;
        [SerializeField] new Text name;
        [SerializeField] Text description;

        Sprite
            LockedImg,
            UnlockedImg;

        bool initialized = false;
        public bool Unlocked { get { return Fields.Bool( "unlocked" ); } set { Fields.Seti( "unlocked", value ? 1 : 0 ); } }

        void Start() => Initialize();

        public void Initialize()
        {
            if ( initialized )
                return;

            initialized = true;
            Fields.obj = this;
            name.text = Fields.String("name");

            //see if this sprite's image is found inside the project and if so, load that.
            //if not found locally, download it from the web
            LockedImg = Resources.Load<Sprite>( ResourceFilename( Fields.String( "locked_url" ) ) );
            UnlockedImg = Resources.Load<Sprite>( ResourceFilename( Fields.String( "unlocked_url" ) ) );
            DisplayRelevantVersion();

            if ( null == LockedImg )
                StartCoroutine( FetchImageOnline( Fields.String( "locked_url" ), true ) );
            if ( null == UnlockedImg )
                StartCoroutine( FetchImageOnline( Fields.String( "unlocked_url" ), false ) );
        }

        public void DisplayRelevantVersion()
        {
            description.text = Unlocked ? Fields.String( "text" ) : Fields.String( "descr" );
            icon.sprite = Unlocked ? UnlockedImg : LockedImg;
        }

        IEnumerator FetchImageOnline(string url, bool locked_sprite)
        {
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
                        if (locked_sprite)
                            LockedImg = Sprite.Create(tex, size, pivot);
                        else
                            UnlockedImg = Sprite.Create(tex, size, pivot);
                    }
                    //see if the currently downloaded image should be displayed
                    if (locked_sprite && !Unlocked)
                        icon.sprite = LockedImg;
                    if (!locked_sprite && Unlocked)
                        icon.sprite = UnlockedImg;
                }
            }
        }

        //extract the filename from the URL so we can see if the image exists inside the project
        string ResourceFilename( string s )
        {
            if ( s.LastIndexOf( "." ) == s.Length - 4 )
                s = s.Substring( 0, s.Length - 4 );
            if ( s.LastIndexOf( '/' ) >= 0 )
                s = s.Substring( s.LastIndexOf( '/' ) + 1 );

            return s;
        }

    }
}