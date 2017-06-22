using System.Collections.Generic;

namespace Bot.Custom
{
    public class Research
    {
        #region Attributes

        public List<Technology> Technologies = new List<Technology>();

        #endregion Attributes

        #region Constructor

        public Research()
        {
            InitTechnologies();
        }

        private void InitTechnologies()
        {
            Technologies.Add(new Technology(Enums.Technologies.slinger));
            Technologies.Add(new Technology(Enums.Technologies.archer));
            Technologies.Add(new Technology(Enums.Technologies.town_guard));
            Technologies.Add(new Technology(Enums.Technologies.hoplite));
            Technologies.Add(new Technology(Enums.Technologies.diplomacy));
            Technologies.Add(new Technology(Enums.Technologies.meteorology));
            Technologies.Add(new Technology(Enums.Technologies.espionage));
            Technologies.Add(new Technology(Enums.Technologies.booty));
            Technologies.Add(new Technology(Enums.Technologies.pottery));
            Technologies.Add(new Technology(Enums.Technologies.rider));
            Technologies.Add(new Technology(Enums.Technologies.architecture));
            Technologies.Add(new Technology(Enums.Technologies.instructor));
            Technologies.Add(new Technology(Enums.Technologies.bireme));
            Technologies.Add(new Technology(Enums.Technologies.building_crane));
            Technologies.Add(new Technology(Enums.Technologies.shipwright));
            Technologies.Add(new Technology(Enums.Technologies.chariot));
            Technologies.Add(new Technology(Enums.Technologies.attack_ship));
            Technologies.Add(new Technology(Enums.Technologies.conscription));
            Technologies.Add(new Technology(Enums.Technologies.demolition_ship));
            Technologies.Add(new Technology(Enums.Technologies.catapult));
            Technologies.Add(new Technology(Enums.Technologies.cryptography));
            Technologies.Add(new Technology(Enums.Technologies.democracy));
            Technologies.Add(new Technology(Enums.Technologies.colonize_ship));
            Technologies.Add(new Technology(Enums.Technologies.small_transporter));
            Technologies.Add(new Technology(Enums.Technologies.plow));
            Technologies.Add(new Technology(Enums.Technologies.berth));
            Technologies.Add(new Technology(Enums.Technologies.trireme));
            Technologies.Add(new Technology(Enums.Technologies.phalanx));
            Technologies.Add(new Technology(Enums.Technologies.breach));
            Technologies.Add(new Technology(Enums.Technologies.mathematics));
            Technologies.Add(new Technology(Enums.Technologies.ram));
            Technologies.Add(new Technology(Enums.Technologies.cartography));
            Technologies.Add(new Technology(Enums.Technologies.take_over));
            Technologies.Add(new Technology(Enums.Technologies.stone_storm));
            Technologies.Add(new Technology(Enums.Technologies.temple_looting));
            Technologies.Add(new Technology(Enums.Technologies.divine_selection));
            Technologies.Add(new Technology(Enums.Technologies.combat_experience));
            Technologies.Add(new Technology(Enums.Technologies.strong_wine));
            Technologies.Add(new Technology(Enums.Technologies.set_sail));
        }

        #endregion Constructor
    }
}