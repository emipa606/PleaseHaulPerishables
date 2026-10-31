# GitHub Copilot Instructions for Please Haul Perishables (Continued)

## Mod Overview and Purpose

Please Haul Perishables (Continued) is a RimWorld mod that updates and extends the functionality of the original Please Haul Perishables mod by Marvin. The mod adds new work givers that prioritize the hauling of perishable items and food over non-perishables. It aims to help players manage their perishable items more efficiently by ensuring they are hauled when necessary.

## Key Features and Systems

### WorkGivers
- **WorkGiver_HaulPerishables**: Prioritizes hauling perishable items that are likely to rot or deteriorate quickly.
- **WorkGiver_HaulGeneralBigStacks**: Hauls valuable items or large stacks of items regardless of whether they are perishable or not.

### Perishable Detection
- Items are considered perishable if they would rot in less than a year or deteriorate to zero hitpoints in ten days or less.
- Rain and water are factored into the deterioration rate.
- Perishable items are only given priority for hauling if they are outside.

### Stack Size Check
- Items with a maximum stack size of 1 (e.g., weapons, apparel) are automatically considered for hauling.
- Other perishables must have a stack size threshold based on the hauler's carrying capacity.

### General Hauling Routine
- Prefers valuable items like silver or large stacks of items for hauling.
- Uses a square grid and plus-shaped grid to identify potential big stacks.

### Integration with Pick Up and Haul
- If Pick Up and Haul is active, the mod will use its hauling routine, but the order of hauling will still be decided by this mod.

### Caching for Performance
- Caches lists of items to be hauled in memory and updates them only when a certain number of in-game ticks have passed.

### Debug Mode
- Provides feedback when right-clicking a haulable, showing reasons why it may not be considered for hauling.

## Coding Patterns and Conventions

- **C# Code Structure**: 
  - Use clear and descriptive method names.
  - Follow consistent naming conventions for variables, classes, and methods.
  - Implement utility classes for specific functionalities (e.g., `DupeUtil`, `CapacityUtil`).

- **XML Configuration**:
  - Use XML files to define work givers and other mod configurations.
  - Ensure XML files are well-structured and easy to read.

- **Harmony Patching**:
  - Use Harmony patches to modify vanilla RimWorld behavior without altering the original code.
  - Ensure patches are well-documented and maintainable.

## XML Integration

XML files in the mod are used to define work givers and other mod configurations. Key XML files include:

- **WorkGivers_Haul_Food_and_Perishables.xml**: Defines the work giver for hauling perishable items.
- **WorkGivers_Haul_General_BigStacks.xml**: Defines the work giver for hauling general big stacks.

### Example XML Configuration

xml
<WorkGiverDef>
  <defName>WorkGiver_HaulPerishables</defName>
  <label>Haul Perishables</label>
  <priority>4</priority>
  <workType>Hauling</workType>
  <giverClass>WorkGiver_HaulPerishables</giverClass>
</WorkGiverDef>


## Harmony Patching

The mod uses Harmony patches to modify vanilla RimWorld behavior. Key patches include:

- **Compat_PickUpAndHaul**: Ensures compatibility with the Pick Up and Haul mod.
- **HaulToStorageJob**: Customizes the hauling job to prioritize perishables.

### Example Harmony Patch

csharp
[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
public static class Pawn_JobTracker_StartJob_Patch
{
    public static void Prefix(ref Job __result, Pawn ___pawn, LocalTargetInfo target, LocalTargetInfo dest, JobDef def, int priority, ThinkTreeDef thinkTree, JobTag __state)
    {
        if (def == JobDefOf.Haul)
        {
            // Custom logic for hauling job
        }
    }
}


## Suggestions for Copilot

- **Code Completion**: Use Copilot to generate code snippets for new methods or utility functions.
- **XML Configuration**: Use Copilot to suggest XML configurations for new work givers or other mod settings.
- **Harmony Patching**: Use Copilot to generate Harmony patches for modifying vanilla RimWorld behavior.

## Contributing

Contributions to this mod are welcome! Please follow these guidelines:

1. **Fork the Repository**: Create a fork of the repository on GitHub.
2. **Clone the Fork**: Clone your fork to your local machine.
3. **Create a Branch**: Create a new branch for your changes.
4. **Make Changes**: Make your changes and ensure they are well-documented.
5. **Push Changes**: Push your changes to your fork.
6. **Create a Pull Request**: Create a pull request to the main repository.

## Reporting Issues

If you encounter any issues with the mod, please report them on the GitHub repository. Include detailed information about the issue, including any error logs.

## Credits

- **Original Code**: Marvin
- **Preview and Thumbnail Images**: Marvin, Pixabay
- **RimWorld Unofficial Font**: Marnador

---

This document provides a comprehensive overview of the Please Haul Perishables (Continued) mod, including its features, coding conventions, XML integration, and Harmony patching. It also offers suggestions for using GitHub Copilot to enhance the development process.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

