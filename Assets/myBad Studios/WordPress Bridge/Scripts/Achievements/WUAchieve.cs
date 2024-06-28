//WordPress For Unity Bridge: Achievements © 2024 by Ryunosuke Jansen is licensed under CC BY-ND 4.0.

using System;

namespace MBS
{
    public enum WUAActions {
        //working with your own data
        FetchEverything,
        FetchUnlocked,
        FetchLocked,
        FetchUnlockedIDs,
        FetchLockedIDs,
        FetchAchievement,
        UnlockAchievement,
        LockAchievement,
        ToggleAchievement,
        IsAchievementUnlocked,

        //working with someone else's data
        FetchUserUnlockedIDs,
        FetchUserLockedIDs,
        UnlockUserAchievement,
        LockUserAchievement,
        ToggleUserAchievement,
        IsUserAchievementUnlocked
    }

    static public class WUAchieve 
    {
        const string filepath = "wub_achievements/unity_functions.php";
        const string ASSET = "AWARD";

        static public void _multipurpose( WUAActions action, int aid = -1, Action<CML> onSuccess = null, Action<CMLData> onFail = null, int uid = -1)
        {
            CMLData data = new CMLData();
            if ( aid > 0 )
                data.Seti( "aid", aid );
            if (uid >= 0)
                data.Seti( "uid", uid );
            WPServer.ContactServer( action, filepath, ASSET, data, onSuccess, onFail );
        }

        static public void FetchEverything( Action<CML> onSuccess=null, Action<CMLData>onFail=null) => _multipurpose( WUAActions.FetchEverything, -1, onSuccess, onFail );
        static public void FetchUnlocked( Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchUnlocked, -1, onSuccess, onFail );
        static public void FetchLocked( Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchLocked, -1, onSuccess, onFail );
        static public void FetchUnlockedIds( Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchUnlockedIDs, -1, onSuccess, onFail );
        static public void FetchLockedIds( Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchLockedIDs, -1, onSuccess, onFail );
        static public void FetchAchievement( Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchAchievement, -1, onSuccess, onFail );
        static public void UnlockAchievement( int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.UnlockAchievement, aid, onSuccess, onFail );
        static public void LockAchievement( int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.LockAchievement, aid, onSuccess, onFail );
        static public void ToggleAchievement( int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.ToggleAchievement, aid, onSuccess, onFail );
        static public void IsAchievementUnlocked( int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.IsAchievementUnlocked, aid, onSuccess, onFail );

        static public void FetchUserUnlockedIDs( int user, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchUserUnlockedIDs, -1, onSuccess, onFail, user );        
        static public void FetchUserLockedIDs( int user, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.FetchUserLockedIDs, -1, onSuccess, onFail, user );
        static public void UnlockUserAchievement( int user, int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.UnlockUserAchievement, aid, onSuccess, onFail, user );
        static public void LockUserAchievement( int user, int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.LockUserAchievement, aid, onSuccess, onFail, user );
        static public void ToggleUserAchievement( int user, int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.ToggleUserAchievement, aid, onSuccess, onFail, user );
        static public void IsUserAchievementUnlocked( int user, int aid, Action<CML> onSuccess = null, Action<CMLData> onFail = null ) => _multipurpose( WUAActions.IsUserAchievementUnlocked, aid, onSuccess, onFail, user );
    }
}
