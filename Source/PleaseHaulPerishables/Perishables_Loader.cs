using System.Linq;
using UnityEngine;
using Verse;

namespace PleaseHaulPerishables;

internal class Perishables_Loader : Mod
{
    public static Perishables_Settings settings;

    public Perishables_Loader(ModContentPack content)
        : base(content)
    {
        settings = GetSettings<Perishables_Settings>();
        if (ModsConfig.ActiveModsInLoadOrder.All(m => m.Name != "Pick Up And Haul"))
        {
            return;
        }

        settings.compatPickUpAndHaul = true;
        if (settings.debug)
        {
            Log.Message("Please Haul Perishables: Turned on compatibility for Pick Up and Haul.");
        }
    }

    public override string SettingsCategory()
    {
        return Content.Name;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Standard = new Listing_Standard
        {
            ColumnWidth = inRect.width / 3f
        };
        listing_Standard.Begin(inRect);
        listing_Standard.CheckboxLabeled("Perishables_Debug".Translate(), ref settings.debug);
        listing_Standard.End();
    }

    public class Perishables_Settings : ModSettings
    {
        public bool compatPickUpAndHaul;
        public bool debug;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref debug, "mode_debug", false, true);
            Scribe_Values.Look(ref compatPickUpAndHaul, "compatibility_PickUpAndHaul", false, true);
        }
    }
}