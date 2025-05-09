﻿Public Class IdCollection
    Public Shared ReadOnly DefaultAttributeIDs As New Dictionary(Of Integer, String) From {
        {210, "Bravery"},
        {212, "Zest"},
        {213, "Intelligence"},
        {214, "Preception"}
   }

    Public Shared ReadOnly DefaultSkillIDs As New Dictionary(Of Integer, String) From {
        {2, "Mining"},
        {3, "Botany"},
        {4, "Construct"},
        {5, "Industry"},
        {6, "Medical"},
        {7, "Gunner"},
        {8, "Shielding"},
        {9, "Operations"},
        {10, "Weapons"},
        {12, "Logistics"},
        {13, "Maintenance"},
        {14, "Navigation"},
        {16, "Research"},
        {1, "Piloting"}
    }


    Public Shared ReadOnly DefaultTraitIDs As New Dictionary(Of Integer, String) From {
        {191, "Hero"},
        {655, "Wimp"},
        {656, "Clumsy"},
        {1034, "Suicidal"},
        {1035, "Smart"},
        {1036, "Bloodlust"},
        {1037, "Antisocial"},
        {1038, "Needy"},
        {1039, "Fast Learner"},
        {1040, "Lazy"},
        {1041, "Hard Working"},
        {1042, "Psychopath"},
        {1043, "Peace-loving"},
        {1044, "Iron-willed"},
        {1045, "Spacefarer"},
        {1046, "Confident"},
        {1047, "Neurotic"},
        {1048, "Charming"},
        {1533, "Iron Stomach"},
        {1534, "Nyctophilia"},
        {1535, "Minimalist"},
        {1560, "Talkative"},
        {1562, "Gourmand"},
        {2082, "Alien lover"}
}


    Public Shared ReadOnly DefaultStorageIDs As New Dictionary(Of Integer, String) From {
    {15, "Root vegetables"},
    {16, "Water"},
    {40, "Ice"},
    {71, "Bio Matter"},
    {127, "Rubble"},
    {157, "Base Metals"},
    {158, "Energium"},
    {162, "Infrablock"},
    {169, "Noble Metals"},
    {170, "Carbon"},
    {171, "Raw Chemicals"},
    {172, "Hyperium"},
    {173, "Electronic Component"},
    {174, "Energy Rod"},
    {175, "Plastics"},
    {176, "Chemicals"},
    {177, "Fabrics"},
    {178, "Hyperfuel"},
    {179, "Processed Food"},
    {706, "Fruits"},
    {707, "Artificial Meat"},
    {712, "Space Food"},
    {725, "Assault Rifle"},
    {728, "SMG"},
    {729, "Shotgun"},
    {760, "Five-Seven Pistol"},
    {930, "Techblock"},
    {984, "Monster Meat"},
    {985, "Human Meat"},
    {1152, "Sentry Gun X1"},
    {1759, "Hull Block"},
    {1873, "Infra Scrap"},
    {1874, "Soft Scrap"},
    {1886, "Hull Scrap"},
    {1919, "Energy Block"},
    {1920, "Superblock"},
    {1921, "Soft Block"},
    {1922, "Steel Plates"},
    {1924, "Optronics Component"},
    {1925, "Quantronics Component"},
    {1926, "Energy Cell"},
    {1932, "Fibers"},
    {1946, "Tech Scrap"},
    {1947, "Energy Scrap"},
    {1954, "Human Corpse"},
    {1955, "Monster Corpse"},
    {2053, "Medical Supplies"},
    {2058, "IV Fluid"},
    {2475, "Fertilizer"},
    {2657, "Nuts and Seeds"},
    {2715, "Explosive Ammunition"},
    {3378, "Grain and Hops"},
    {3384, "Armored Vest"},
    {3386, "Remote Control"},
    {3388, "Oxygen Tank"},
    {3419, "Augmentation Parts"},
    {3960, "Flamethrower (Weapon Attachment)"},
    {3961, "Stun Rifle"},
    {3962, "Stun Pistol"},
    {3967, "Explosive Grenade Launcher (Weapon Attachment)"},
    {3968, "Basic Scope (Weapon Attachment)"},
    {3969, "Tactical Grip (Weapon Attachment)"},
    {4005, "Painkillers"},
    {4006, "Combat Stimulant"},
    {4007, "Bandage"},
    {4030, "Nano Wound Dressing"},
    {4040, "Small Breach Charge"},
    {4065, "Space Suit Oxygen Extender"},
    {4076, "Incendiary Grenade Launcher (Weapon Attachment)"},
    {3069, "Laser Rifle"},
    {3070, "Laser Pistol"},
    {3071, "Plasma Cuttergun"},
    {3072, "Plasma Rifle"}
}




    Public Shared ReadOnly ConditionsIDs As New Dictionary(Of Integer, String) From {
    {193, "Panicked"},
    {194, "Scared"},
    {713, "Frostbite"},
    {714, "First-degree burn"},
    {715, "Wound"},
    {751, "Blast injury"},
    {1003, "Crawler bite"},
    {1033, "Ate without table"},
    {1053, "Feeling a little hungry"},
    {1058, "Feeling a little unsafe"},
    {1059, "Slept on the floor"},
    {1060, "Holding it in"},
    {1061, "It's so dark on this spaceship"},
    {1062, "Ate the meat of a human being"},
    {1063, "Wearing spacesuit"},
    {1064, "Feeling adventurous"},
    {1065, "Feeling meaningful"},
    {1066, "Feeling loved"},
    {1096, "Shat pants"},
    {1108, "Unconscious"},
    {1109, "Starvation"},
    {1112, "Low oxygen"},
    {1118, "CO2 condition"},
    {1119, "Hazardous gas"},
    {1120, "Smoke condition"},
    {1121, "Low body temperature"},
    {1122, "High body temperature"},
    {1123, "Ate too much"},
    {1124, "Ate monster meat"},
    {1125, "Ate spoiled food"},
    {1127, "Lonely"},
    {1430, "Cocooned"},
    {1550, "Did something I love"},
    {1561, "Black eye"},
    {1563, "Did something I dislike"},
    {1581, "Minor discomfort"},
    {1582, "Moderate discomfort"},
    {1583, "Major discomfort"},
    {1584, "Feeling a little unsafe"},
    {1585, "Fearful"},
    {1586, "Terrified"},
    {1587, "Feeling left out"},
    {1588, "Feeling isolated and lonely"},
    {1589, "Feeling completely alone and unloved"},
    {1590, "Low energy"},
    {1591, "Feeling fatigued"},
    {1592, "Extremely fatigued"},
    {1593, "Feeling slightly hungry"},
    {1594, "Feeling hungry"},
    {1595, "Feeling very hungry"},
    {1596, "Some health problems"},
    {1597, "Concerning health problems"},
    {1598, "Serious health problems"},
    {1600, "Having a mental breakdown"},
    {1622, "Heard a funny joke"},
    {1623, "Someone was mean to me"},
    {1624, "Got comforted"},
    {1625, "Got rejected"},
    {1626, "Someone thanked me"},
    {1648, "Cryo sleep"},
    {1649, "Wall cocoon"},
    {1739, "Spacesuit fatigue"},
    {2055, "Resting"},
    {2056, "Resting in medical bed"},
    {2057, "Open wound"},
    {2080, "Aliens haunt me in my dreams"},
    {2081, "I was held as a prisoner"},
    {2083, "Stockholm syndrome"},
    {2246, "Working comfortably"},
    {2247, "Resting comfortably"},
    {2248, "Working uncomfortably"},
    {2417, "Prisoner recruitment"},
    {2482, "Concussion"},
    {2490, "Disorientation"},
    {2491, "Lost appetite"},
    {2492, "Insomnia"},
    {2493, "Unable to work"},
    {2494, "Psychosis"},
    {2495, "Aggressive behavior"},
    {2496, "Schizophrenia"},
    {2497, "Urge to destroy"},
    {2498, "Moody"},
    {2499, "Attempted suicide"},
    {2500, "Poisoned"},
    {2512, "Saw a captive crew member"},
    {2664, "Nausea"},
    {2667, "Protein deficiency"},
    {2668, "Fatty acids deficiency"},
    {2669, "Craving for carbohydrates"},
    {2670, "Vitamin deficiency anemia"},
    {2728, "Wound"},
    {2729, "Hauler slash"},
    {2798, "Received an electric shock"},
    {2843, "I was held as a slave"},
    {3090, "Burned hands"},
    {3091, "I messed up"},
    {3092, "Injured"},
    {3093, "Vision loss"},
    {3094, "Inhaled toxic fumes"},
    {3095, "Exposed to loud noise"},
    {3118, "Spore infection"},
    {3120, "Spore eruption"},
    {3121, "Feeling ill"},
    {3133, "Chronic wound"},
    {3136, "Broken arm"},
    {3137, "Knocked unconscious"},
    {3160, "Unconscious"},
    {3164, "Interstellar travel sickness"},
    {3194, "Refusing to work (Uncomfortable environment)"},
    {3195, "I rebelled (Uncomfortable environment)"},
    {3307, "Minor comfort"},
    {3308, "Moderate comfort"},
    {3309, "Major comfort"},
    {3310, "Uncomfortable sleep"},
    {3311, "Sleeping comfortably"},
    {3312, "Uncomfortable leisure space"},
    {3313, "Good leisure space"},
    {3314, "Ate a great meal"},
    {3315, "Feeling sad"},
    {3321, "Feeling aggressive (Minor mental break)"},
    {3322, "Destructive behavior (Major mental break)"},
    {3323, "Pyromania (Major mental break)"},
    {3324, "Interrupted sleep (No privacy)"},
    {3325, "Sleeping with privacy"},
    {3327, "No entertainment"},
    {3328, "Feeling claustrophobic (No hull windows)"},
    {3329, "Feeling energetic"},
    {3330, "Extremely frustrated"},
    {3332, "Had a good chat with a friend"},
    {3333, "Shared my feelings with a good friend"},
    {3334, "Connected with my best friend"},
    {3335, "Sleep deprived"},
    {3337, "In a good mood"},
    {3338, "In a great mood"},
    {3339, "In a fantastic mood"},
    {3340, "No one to talk to"},
    {3341, "Missing my friend"},
    {3342, "Missing my lover"},
    {3343, "My friend died"},
    {3344, "My best friend died"},
    {3345, "My lover died"},
    {3346, "I think my friend might be dead"},
    {3347, "I think my lover might be dead"},
    {3348, "My enemy is not bothering me"},
    {3349, "I think my enemy might be dead"},
    {3350, "My enemy is dead"},
    {3351, "Panicking"},
    {3352, "Refusing to work (Minor mental break)"},
    {3353, "Unconscious (Major mental break)"},
    {3354, "Saboteur (Extreme mental break)"},
    {3361, "No decorations"},
    {3368, "Drinking binge (Minor mental break)"},
    {3369, "Gaming binge (Minor mental break)"},
    {3370, "Intoxicated (Alcohol)"},
    {3371, "Hungover (Alcohol)"},
    {3380, "Lost my leader"},
    {3385, "Sleeping together with my lover"},
    {3440, "Post-surgery fatigue"},
    {3442, "Post-surgery rest"},
    {3445, "Wound"},
    {3446, "Wound"},
    {3447, "Burn wound"},
    {3448, "Wound"},
    {3465, "Healing boosted"},
    {3467, "Groggy"},
    {3481, "Had an unpleasant conversation"},
    {3660, "Wound"},
    {3699, "Wound"},
    {3700, "Wound"}
}


    Public Shared ReadOnly ResearchIDs As New Dictionary(Of Integer, String) From {
    {2532, "Scanner"},
    {2533, "Shield Generator"},
    {2534, "Energy Turret"},
    {2538, "Large Storage"},
    {2539, "Autopsy Table"},
    {2559, "Medical Bed"},
    {2560, "Growbed with Light"},
    {2561, "CO2 Producer"},
    {2563, "Arcade Machine"},
    {2564, "Jukebox"},
    {2565, "Solar Panel"},
    {2566, "X2 Power Generator"},
    {2567, "X3 Power Generator"},
    {2568, "Power Capacity Node"},
    {2569, "Item Fabricator"},
    {2570, "Micro-Weaver"},
    {2571, "Assembler"},
    {2572, "Energy Refinery"},
    {2573, "Chemical Refinery"},
    {2574, "Water Collector"},
    {2575, "Advanced Assembler"},
    {2576, "Composter"},
    {2577, "Hypersleep Chamber"},
    {2581, "Basic"},
    {2583, "Hyperium Hyperdrive"},
    {2584, "Chemical"},
    {2585, "Advanced"},
    {2586, "Optronic"},
    {2587, "Quantum"},
    {2589, "Weapons Console"},
    {2590, "Shields Console"},
    {2591, "Missile Turret"},
    {2592, "Unknown"},
    {2594, "X1 Power Generator"},
    {2595, "X1 Hyperdrive"},
    {2596, "Unknown"},
    {2597, "Unknown"},
    {2598, "Unknown"},
    {2599, "Unknown"},
    {2600, "Unknown"},
    {2601, "Targeting Jammer"},
    {2602, "Unknown"},
    {2604, "Unknown"},
    {2605, "Unknown"},
    {2606, "Unknown"},
    {2607, "Unknown"},
    {2609, "Unknown"},
    {2610, "Unknown"},
    {2611, "Unknown"},
    {2612, "Metal Refinery"},
    {2613, "Unknown"},
    {2614, "Unknown"},
    {2617, "Unknown"},
    {2618, "Fabrics"},
    {2619, "Fibers"},
    {2622, "Unknown"},
    {2623, "Botany"},
    {2626, "Advanced Nutrition"},
    {2627, "Space Food"},
    {2628, "Artificial Meat"},
    {2629, "Unknown"},
    {2630, "Unknown"},
    {2635, "Unknown"},
    {2694, "Optronics Fabricator"},
    {2696, "X1 Couch"},
    {2847, "Enslavement Facility"},
    {3024, "Logistics Robot Station"},
    {3025, "Salvage Robot Station"}
}

    Public Shared ReadOnly CraftsIDs As New Dictionary(Of Integer, String) From {
    {20, "Shuttle"},
    {39, "Miner"},
    {2786, "Fighter"}
}


    'Public Shared ReadOnly KnownStorageObjIds As New List(Of String) From {"1740"} ' Example: Basic Storage


End Class
