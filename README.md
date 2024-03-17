# PleaseHaulPerishables

![Image](https://i.imgur.com/buuPQel.png)

Update of Jellypowereds update
https://steamcommunity.com/sharedfiles/filedetails/?id=2073818889
of MarvinKosh original mod
https://steamcommunity.com/sharedfiles/filedetails/?id=1542424432

![Image](https://i.imgur.com/pufA0kM.png)

	
![Image](https://i.imgur.com/Z4GOv8H.png)

With permission, I've updated this to 1.1 compatibility. The only changes made was to detect [Url=https://steamcommunity.com/sharedfiles/filedetails/?id=1279012058]Pick Up and Haul correctly. Debug mode in the settings gives quite a bit more information as well.
Something I noticed is that this mod makes best effort to haul perishables, but your work priorities still govern when your pawns actually haul. 
Original Mod: https://steamcommunity.com/sharedfiles/filedetails/?id=1542424432]Please Haul Perishables by https://steamcommunity.com/id/marvinkosh/myworkshopfiles/?appid=294100]Marvin

# Original Description Below


Adds new WorkGivers for hauling, which prioritise hauling perishables or food over non-perishables. Items will be considered perishable if they would rot in less than a year or deteriorate to zero hitpoints in ten days or less.

Rain and water, which both cause higher deterioration rates, will be taken into account.

Perishable items will only be given priority for hauling if they are outside.

A check is made to see if the perishable has a large enough stack size. Things which have a maximum stack size of 1 (weapons and apparel for example) pass automatically. Other perishables must have a high enough stack count for that kind of item. The exact threshold depends on the hauler's current carrying capacity and the ideal carrying capacity for their race, but it only goes as high as 40. The perishable can still pass the check if there are other perishables of the same type nearby, or if it would deteriorate to zero hitpoints in ten days or less.

Food will also be hauled if it needs to go from low to high priority storage.

A new general hauling routine prefers valuable items like silver or big stacks of items for hauling, regardless of whether they are perishable or not. It will look at a square grid and a plus-shaped grid of cells to see if a big stack of the same type of item could be made. The normal general hauling routine picks up any leftovers.

New in this version, if Pick Up and Haul is active, the mod will use its hauling routine instead, but the order in which things will be hauled will still be decided by this mod.

Also, to improve performance, the lists of things to be hauled will be cached in memory and only updated when a certain number of in-game ticks have passed.

A debug mode exists and will give feedback when you right-click a haulable, so if it is not considered perishable or food or a big stack, that will show as a reason for not doing the job, in addition to still allowing you to manually prioritise hauling.

[Version 1.5.2]

# Credits - Retained from Marvin's Source


Original code by Marvin.

Preview is originally made by Marvin.

Steam thumbnail image backgrounds are royalty-free images from Pixabay or originally made (badly) by Marvin.

RimWorld unofficial font by Marnador.

![Image](https://i.imgur.com/PwoNOj4.png)



-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using https://steamcommunity.com/workshop/filedetails/?id=818773962]HugsLib or the standalone https://steamcommunity.com/sharedfiles/filedetails/?id=2873415404]Uploader and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.
-  Use https://github.com/RimSort/RimSort/releases/latest]RimSort to sort your mods



https://steamcommunity.com/sharedfiles/filedetails/changelog/2807630952]![Image](https://img.shields.io/github/v/release/emipa606/PleaseHaulPerishables?label=latest%20version&style=plastic&color=9f1111&labelColor=black)

