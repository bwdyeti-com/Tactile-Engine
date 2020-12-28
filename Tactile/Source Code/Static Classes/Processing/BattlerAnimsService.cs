namespace Tactile
{
    class BattlerAnimsService : IBattlerAnimsService
    {
        public TactileLibrary.Battler.Battle_Animation_Association_Data animation_set(string name, int gender)
        {
            if (Global.data_battler_anims.ContainsKey(name))
                return Global.data_battler_anims[name].anim_set(gender);
            return null;
        }
    }
}
