# GitHub Copilot Instructions for the RimWorld Modding Project

## Mod Overview and Purpose
The "Please Haul Perishables" mod for RimWorld is designed to enhance the game by improving how colonists prioritize and manage the hauling of perishable items. The primary aim of this mod is to reduce spoilage and optimize storage solutions for perishable goods, making the logistics of food and other sensitive goods more effective within the colony. It strives to create a more realistic and efficient mechanism for managing perishables, encouraging players to plan their storage and handling of goods carefully.

## Key Features and Systems
1. **Dynamic Food Hauling Logic**: Colonists will prioritize hauling perishable items to prevent spoilage.
2. **Compatibility with Mods for Storage Solutions**: Integrates smoothly with other mods that introduce new storage options.
3. **Animal Feed Management**: Special handling routines for animal feed to ensure pets and livestock receive the correct feed.
4. **Temperature-Based Decisions**: Items will be hauled to the coldest available storage to extend their freshness.
5. **User-Friendly Settings**: Easily adjustable settings that players can tweak according to their needs.

## Coding Patterns and Conventions
- **Static Utility Classes**: Classes such as `CapacityUtil`, `DupeUtil`, etc., are used to contain static methods that facilitate various calculations and checks throughout the mod.
- **Logic Encapsulation**: Core functionalities are encapsulated in internal classes and methods to separate concerns and maintain clean code.
- **Naming Conventions**: Methods use descriptive names like `CanAnyoneEatThisIfPawnMovesIt` and `TestStoreInColderPlace`, following C# naming standards.

## XML Integration
Features within the mod can be integrated using XML for defining custom rules and tweaks. Although XML configurations aren't detailed in the given summary, typical uses in RimWorld modding include:
- Defining new WorkGiver categories and priorities.
- Specifying settings for mod operations in XML configuration files.

## Harmony Patching
This mod utilizes Harmony for runtime modification of RimWorld classes and methods without altering the game’s core files. Harmony allows for:
- Safely changing the behavior of methods in the RimWorld codebase, particularly in selecting which items to haul.
- Ensuring compatibility and integration with other mods by selectively applying patches where necessary.

## Suggestions for Copilot
- **Assist with Static Methods**: Generate helper functions in utility classes to streamline common calculations and checks.
- **Design Harmony Annotations**: Suggest templates for typical Harmony patch setups, allowing for easy patch additions.
- **Simplify XML Configurations**: Provide templates or samples for XML configuration files, focusing on integration points like new WorkGiver settings.
- **Suggest Debugging Patterns**: Offer debugging patterns and best practices for testing mod changes within RimWorld’s environment.

This document serves as a guideline for contributors who utilize GitHub Copilot to keep development consistent and efficient. By adhering to these specifications, contributors can effectively enhance and maintain the mod's functionality.
