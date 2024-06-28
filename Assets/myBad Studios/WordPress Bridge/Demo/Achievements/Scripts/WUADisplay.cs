//WordPress For Unity Bridge: Achievements © 2024 by Ryunosuke Jansen is licensed under CC BY-ND 4.0.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MBS {
	public class WUADisplay : MonoBehaviour {

        [SerializeField] RectTransform content_area;
        [SerializeField] WUAView	view_prefab;
        [SerializeField] bool destroy_contents_on_load = true;

        //we are going to store our tracking info offline so we can continue
        //tracking achievement award states between games. This has nothing to do with 
        //what is unlocked or not. That info comes from the server. This is you keeping
        //track of what the player has done so that you can determine WHEN to tell the
        //server that an achievement is ready to be unlocked.
        CML Keys;
        CMLData _keys => Keys [0];

        //store all the award info we received from the server
        CML all_awards;

        //we only try to auto award the achievements with requirements set on server side
        List<CMLData> tracked;

		void Start()
		{
            //wait until login was successful then download all keys
            //this is great during the demo but if you spawn your prefab(s) mid game
            //you might have to call it manually. Either way, see the FetchAwards function to see how
            WULogin.OnLoggedIn += FetchAwards;

            //load the achievement tracking info from our previous play session (if any)
            Keys = new CML();
            Keys.Load( "achievements" );
            if ( Keys.Count == 0 )
                Keys.AddNode( "keys" );
            tracked = new List<CMLData>();

            //do you want to destroy any existing loaded child prefabs when you load this prefab?
            //personal taste. If the prefab never gets destroyed then you can choose to keep all
            //loaded achievements in place and on displaying the prefab just do a quick call to check
            //if any achievements need to be updated. Since I do this during my spawning and this is
            //just a single scene demo I prefer to start fresh every time this is spawned...
            WUAView [] all_views = content_area.GetComponentsInChildren<WUAView>();
			if (null != content_area && destroy_contents_on_load)
				foreach(WUAView view in all_views)
					Destroy (view.gameObject);
		}

        void OnDestroy() => WULogin.OnLoggedIn -= FetchAwards;

        //manually decide what achievements to award or take away...
        public void AwardAchievement( int aid ) => WUAchieve.UnlockAchievement( aid, _updateAfterManualAwards );
        public void RevokeAchievement( int aid ) => WUAchieve.LockAchievement( aid, _updateAfterManualAwards );
        public void ToggleAchievement( int aid ) => WUAchieve.ToggleAchievement( aid, _updateAfterManualAwards );

        //fetch all achievements from the server
        //upon receiving the server response spawn a prefab to display each returned result
        void FetchAwards( CML response ) => WUAchieve.FetchEverything( GenerateEntries );
        void GenerateEntries( CML response )
        {
            //store the server results then extract the achievements to work with in this function
            all_awards = response;
            List<CMLData> entries = all_awards.Children( 0 );

            //make sure our scroll region can handle all the entries...
            GridLayoutGroup glg = content_area.GetComponent<GridLayoutGroup>();
            content_area.sizeDelta = new Vector2( content_area.sizeDelta.x, entries.Count * ( view_prefab.GetComponent<RectTransform>().sizeDelta.y + glg.spacing.y ) );
            content_area.sizeDelta = new Vector2( content_area.sizeDelta.x, entries.Count * ( glg.cellSize.y + glg.spacing.y ) );

            //and now spawn them...
            foreach ( CMLData entry in entries )
            {
                //if an entry has server side requirements AND hasn't been unlocked already, track it for auto unlocking
                if ( entry.String( "requirements" ).Trim() != "" && !entry.Bool("unlocked") )
                {
                    tracked.Add( entry );
                    Debug.LogWarning( $"Tracking {entry.String( "name" )} because of it's requirements of {entry.String( "requirements" )}" );
                }
                WUAView view = Instantiate( view_prefab );
                view.transform.SetParent( content_area, false );
                view.Fields = entry;
                view.Initialize();
            }
            ShowHowmanyIAmTracking();
        }

        void ShowHowmanyIAmTracking()
        {
            Debug.LogWarning($"Tracking {(null == tracked ? 0 : tracked.Count)}");
		}

        public void UpdateKeys( string name, int qty )
        {
            //Save the current tracking keys so we are up to date across game sessions
            _keys.Add( qty, name );
            Keys.Save( "achievements" );
            Debug.LogWarning( Keys.ToString() );

            //since the keys have been updated, let's see if anything is now unlocked
            ScanUnlockedStatus();
        }

        public void ScanUnlockedStatus()
        {
            bool achieved = true;
            List<int> new_unlocks = new List<int>();

            foreach ( CMLData entry in tracked )
            {
                //assuming that the test will pass makes it easier to determine when one of the series of requirements caused the entire test to fail
                achieved = true;
                
                //requirements are sent as a comma delimited array so first step is to separate the various requirements
                string [] requirements = entry.String( "requirements" ).Split( ',' );

                //next we test each requirement in turn to see if all of them past their respective tests...
                foreach ( string requirement in requirements )
                {
                    //if one of the requirements failed then there is no point in continuing to test the rest. Move on to the next achievement
                    if ( !achieved )
                        continue;

                    string [] elements = requirement.Split( ' ' );
                    if ( elements.Length < 3 || elements [2].Trim() == "" )
                    {
                        achieved = false;
                        UnityEngine.Assertions.Assert.IsFalse( achieved, $"{requirement} is not a properly formatted requirement" );
                        continue;
                    }

                    //requirements have the format: TEST NAME QTY. Example GT Gold 500. 

                    //let's fetch the current value of the key
                    int value = _keys.Int( elements [1] );

                    //Get a numeric 3rd element
                    int qty = int.Parse( elements [2] );
                    
                    //and see if the current test fails. By default we assume it passed so we only need to check if we are wrong
                    switch ( elements [0] )
                    {
                        case "LT":
                            if ( value >= qty )
                                achieved = false;
                            break;

                        case "GT":
                            if ( value < qty )
                                achieved = false;
                            break;

                        case "EQ":
                            if ( value != qty )
                                achieved = false;
                            break;
                    }
                }

                //if achieved is still true at this point then an achievement needs to be unlocked!
                if (achieved)
                    new_unlocks.Add( entry.Int( "aid" ) );
            }

            //see if we have any achievements to unlock.
            //Unlock them all and wait for the server feedback to come through before stopping to track it
            if ( new_unlocks.Count > 0 )
            {
                foreach ( int aid in new_unlocks )
                    WUAchieve.UnlockAchievement( aid, _updateAchievements );
            }
        }

        void _updateAfterManualAwards( CML response )
        {
            List<CMLData> entries = all_awards.Children( 0 );
            string [] unlocked = response [0].String( "unlocked" ).Split( ',' );
            foreach ( CMLData entry in entries )
            {
                string aid = entry.String( "aid" );
                bool found = false;
                foreach ( string s in unlocked )
                    if ( s == aid )
                        found = true;

                WUAView display = entry.obj as WUAView;
                display.Unlocked = found;
                display.DisplayRelevantVersion();
            }
        }

        void _updateAchievements( CML response )
        {
            //get the complete list of awarded achievements 
            string [] unlocked = response [0].String( "unlocked" ).Split( ',' );
            if ( unlocked.Length == 0 ) return;

            //scan through all unlocked achievements
            foreach ( string aid in unlocked )
            {
                //look through all achievements that we are tracking
                //we want to stop tracking it once it's been unlocked
                //we can't remove a foreach key during a foreach loop
                //so we use a for loop instead but we start from the back
                //to avoid skipping indexes as we remove the entries during the loop
                for ( int i = tracked.Count-1; i >= 0; i-- )
                {
                    //if the entry we are tracking has the same id as an unlocked achievement
                    //we want to update the text and stop tracking it
                    if ( tracked[i].String( "aid" ) == aid )
                    {
                        //inside the gui object we linked the object to this achievement data block
                        //so work backwards and use the data block to determine which gui object to work on
                        WUAView display = tracked [i].obj as WUAView ;

                        //set it to unlocked and then update the text/graphic
                        display.Unlocked = true;
                        display.DisplayRelevantVersion();

                        //now stop tracking it
                        tracked.RemoveAt( i );
                    }
                }
            }
            ShowHowmanyIAmTracking();
        }
	}
}