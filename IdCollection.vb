Public Class IdCollection
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
        {1, "Piloting"},
        {22, "Piloting"}
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
    {34, "Power (Resource)"},
    {40, "Ice"},
    {63, "Oxygen (Atmospheric Resource)"},
    {64, "Carbon Dioxide (Atmospheric Resource)"},
    {71, "Bio Matter"},
    {73, "Heat (Atmospheric Resource)"},
    {77, "Smoke (Atmospheric Resource)"},
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
    {184, "Base Metals"},
    {706, "Fruits"},
    {707, "Artificial Meat"},
    {712, "Space Food"},
    {725, "Assault Rifle"},
    {728, "SMG"},
    {729, "Shotgun"},
    {760, "Five-Seven Pistol"},
    {930, "Techblock"},
    {971, "Hazardous Gas (Atmospheric Resource)"},
    {984, "Monster Meat"},
    {985, "Human Meat"},
    {1152, "Sentry Gun X1"},
    {1397, "Food Group (UI Element)"},
    {1445, "Build Tools"},
    {1759, "Hull Block"},
    {1858, "Credits (Trade Resource)"},
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
    {1955, "Crawler Corpse"},
    {1956, "Android Corpse"},
    {2053, "Medical Supplies"},
    {2058, "IV Fluid"},
    {2452, "Water Vapor (Atmospheric Resource)"},
    {2475, "Fertilizer"},
    {2657, "Nuts and Seeds"},
    {2715, "Explosive Ammunition"},
    {2797, "Slave Collar"},
    {2883, "Logistics Bot Corpse"},
    {2899, "Salvage Bot Corpse"},
    {3077, "Hauler Corpse"},
    {3079, "Human Corpse (Player)"},
    {3196, "High Capacity Power (Resource)"},
    {3366, "Mild Alcohol"},
    {3378, "Grain and Hops"},
    {3383, "Bulletproof Vest"},
    {3384, "Armored Vest"},
    {3386, "Remote Control"},
    {3388, "Oxygen Tank"},
    {3419, "Augmentation Parts"},
    {3512, "Exotic Ore"},
    {3513, "Basic Ore"},
    {3711, "Evolving Alien Hive Corpse"},
    {3712, "Alien Hive Corpse"},
    {3713, "Chimp (Walkerbot)"},
    {3714, "Hamster (Flybot)"},
    {3960, "Flamethrower (Weapon Attachment)"},
    {3961, "Stun Rifle"},
    {3962, "Stun Pistol"},
    {3967, "Explosive Grenade Launcher (Weapon Attachment)"},
    {3968, "Basic Scope (Weapon Attachment)"},
    {3969, "Tactical Grip (Weapon Attachment)"},
    {4005, "Painkillers"},
    {4006, "Combat Stimulant"},
    {4007, "Bandage"},
    {4027, "Alien Organ"},
    {4028, "Human Organ"},
    {4030, "Nano Wound Dressing"},
    {4040, "Small Breach Charge"},
    {4065, "Space Suit Oxygen Extender"},
    {4076, "Incendiary Grenade Launcher (Weapon Attachment)"},
    {4078, "Parasite Corpse"},
    {4106, "Infester Corpse"},
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
    {2560, "Grow Bed with Light"},
    {2561, "CO2 Producer"},
    {2563, "Arcade Machine"},
    {2564, "Basic Entertainment"},
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
    {2591, "Rocket Turret"},
    {2592, "Energy Turret (mk2)"},
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
    {2605, "Laser Weapons"},
    {2606, "Plasma Weapons"},
    {2607, "Surgical Enhancement Facility"},
    {2609, "Implanted Rebreather"},
    {2610, "Ocular Implant"},
    {2611, "Synthetic Stomach Lining"},
    {2612, "Metal Refinery"},
    {2613, "Unknown"},
    {2614, "Unknown"},
    {2617, "Unknown"},
    {2618, "Fabrics"},
    {2619, "Fibers"},
    {2622, "Bulletproof Vest"},
    {2623, "Botany"},
    {2626, "Advanced Nutrition"},
    {2627, "Space Food"},
    {2628, "Artificial Meat"},
    {2629, "Alcohol Beverage Machine"},
    {2630, "Grains and Hops"},
    {2635, "Unknown"},
    {2694, "Optronics Fabricator"},
    {2696, "X1 Couch"},
    {2847, "Enslavement Facility"},
    {3024, "Logistics Robot Station"},
    {3025, "Salvage Robot Station"},
    {3112, "Recycler"},
    {3114, "Research Lab"},
    {3115, "Research Workbench"},
    {3116, "Research Experiment Table"},
    {3119, "Navigation Console"},
    {3122, "Operations Console"},
    {3124, "Crawler"},
    {3125, "Hauler"},
    {3127, "Robotics 01"},
    {3128, "Industry 01"},
    {3129, "Industry 02"},
    {3130, "Botany 02"},
    {3417, "Armored Vest"},
    {3420, "Anatomical Augmentation"},
    {3421, "Neural Augmentation"},
    {3422, "Nanotech Augmentation"},
    {3423, "Prefrontal Microcontroller"},
    {3464, "Sentry Gun X1"},
    {3704, "Alien Hive Core"},
    {3705, "Evolving Alien Core"},
    {3706, "Advanced Nutrition 02"},
    {3707, "Hamster (Flybot)"},
    {3708, "Chimp (Walkerbot)"},
    {3709, "X2 Hypersleep Tank"},
    {3710, "Rogue Bot Architecture"},
    {3970, "Advanced Medical Bed"},
    {3973, "Stun Weapons"},
    {3974, "Weapon Attachments 1"},
    {4024, "Alien Enzyme"},
    {4032, "Nano Wound Dressing"},
    {4092, "Stimulants"},
    {4093, "Advanced Disassembly"},
    {4132, "Learning Computer"},
    {4134, "Advanced Learning System"},
    {4529, "Combat Robot Station"}
}

    Public Shared ReadOnly DatalogIDs As New Dictionary(Of Integer, String) From {
    {1817, "Datalog 1817"},
    {1820, "Datalog 1820"},
    {1821, "Datalog 1821"},
    {1822, "Datalog 1822"},
    {1830, "Datalog 1830"},
    {1831, "Datalog 1831"},
    {1836, "Datalog 1836"},
    {1837, "Datalog 1837"},
    {1841, "Datalog 1841"},
    {1842, "Datalog 1842"},
    {1843, "Datalog 1843"},
    {1844, "Datalog 1844"},
    {1845, "Datalog 1845"},
    {1846, "Datalog 1846"},
    {1847, "Datalog 1847"},
    {3771, "Datalog 3771"},
    {3772, "Datalog 3772"},
    {3773, "Datalog 3773"},
    {3776, "Datalog 3776"}
}

    Public Shared ReadOnly CraftsIDs As New Dictionary(Of Integer, String) From {
    {20, "Shuttle"},
    {39, "Miner"},
    {2786, "Fighter"},
    {3491, "Builder"},
    {3641, "Drone Sentry"},
    {3642, "Drone Fighter"},
    {3644, "Drone Scout"},
    {3668, "Drone Dropship"},
    {3683, "Infester Spacecraft"},
    {3750, "Large Asteroid 1"},
    {3751, "Medium Asteroid 1"},
    {3752, "Small Asteroid 1"},
    {3753, "Small Asteroid 2"},
    {3754, "Large Asteroid 2"},
    {3755, "Medium Asteroid 2"}
}

    Public Shared ReadOnly OccupationIDs As New Dictionary(Of Integer, String) From {
    {1763, "Police"},
    {1764, "Nurse"},
    {1765, "Doctor"},
    {1766, "Sales"},
    {1767, "Cook"},
    {1768, "Material Mover"},
    {1769, "Engineer"},
    {1770, "Security Guard"},
    {1771, "Construction Laborer"},
    {1772, "Sheriff"},
    {1773, "Lawyer"},
    {1774, "Medical Assistant"},
    {1775, "Bartender"},
    {1776, "Teacher"},
    {1777, "Computer Programmer"},
    {1778, "Welder"},
    {1779, "Firefighter"},
    {1780, "Telemarketer"},
    {1781, "Lab Tech"},
    {1782, "Chemical Engineer"},
    {1783, "Airline Pilot"},
    {1784, "Fighter Pilot"},
    {1785, "Driller"},
    {1786, "Mining Tech"},
    {1787, "Geologist"},
    {1788, "Farmer"},
    {1789, "Scientist"},
    {1790, "Blacksmith"},
    {1791, "Navigator"},
    {1792, "Florist"}
}

    Public Shared ReadOnly CharacterIDs As New Dictionary(Of Integer, String) From {
    {89, "Human / Android"},
    {989, "Crawler"},
    {994, "Crawler Egg"},
    {1426, "Standard Hive"},
    {1427, "Hauler"},
    {1429, "Hauler Egg"},
    {2874, "Logistics Bot"},
    {2888, "Salvage Bot"},
    {3657, "Fly Bot"},
    {3682, "Chimp Bot"},
    {4108, "Alien Parasite"}
}

    Public Shared ReadOnly ExplosionIDs As New Dictionary(Of Integer, String) From {
    {2187, "Rocket Turret Projectile"},
    {2195, "Energy Turret Projectile"},
    {2886, "Breaching Charge Secondary Component"},
    {3261, "Autoturret Projectile"},
    {3680, "Drone Anti-Ship Weapon"},
    {3681, "Fighter Anti-Ship Weapon"},
    {3765, "Small Asteroid Attack"},
    {3768, "Medium Asteroid Attack"},
    {3769, "Large Asteroid Attack"},
    {4041, "Breaching Charge Main Component"}
}

    Public Shared ReadOnly ProcessIDs As New Dictionary(Of Integer, String) From {
    {17, "Roots and Vegetables Growth Cycle"},
    {35, "Energy Rod to High Capacity Power (X1 Generator)"},
    {41, "Ice to Water"},
    {62, "Water to Oxygen"},
    {65, "Carbon to Carbon Dioxide"},
    {66, "Carbon Dioxide to Carbon"},
    {74, "Unknown Process"},
    {78, "Smoke to Carbon and Raw Chemicals"},
    {181, "Metal Refinery Process Group"},
    {183, "Steel Plates and Electronics to Infrablock"},
    {447, "Unknown Item Fabricator Hook"},
    {703, "Unknown Food Processor Hook"},
    {704, "Fruit Growth Cycle"},
    {705, "Artificial Meat Growth Cycle"},
    {913, "Nothing to Oxygen"},
    {914, "Unknown Process"},
    {959, "Tech Block Production"},
    {960, "Energy Refinery Process Group"},
    {962, "Deprecated Metal Refinery Process Group"},
    {963, "Chemical Refinery Process Group"},
    {964, "Energium to Energy Rod"},
    {965, "Hyperium to Hyperfuel"},
    {966, "Metals to Electronic Components"},
    {983, "Hazardous Gas to Raw Chemicals"},
    {1442, "Solar Power Process (High Efficiency)"},
    {1443, "Solar Power Process"},
    {1446, "Infrablock to Build Tools"},
    {1760, "Steel Plates to Hull Block"},
    {1876, "Soft Scrap to Soft Block"},
    {1877, "Infra Scrap Recycling"},
    {1879, "Recycler Process Group"},
    {1883, "Energy Rod to High Capacity Power (X2 Generator)"},
    {1884, "Energy Rod to High Capacity Power (X3 Generator)"},
    {1887, "Hull Scrap Recycling"},
    {1927, "Soft Block Production"},
    {1928, "Energy Block Production"},
    {1929, "Superblock Production (Unused)"},
    {1930, "Energy Cell Production"},
    {1931, "Steel Plate Production"},
    {1934, "Microweaver Process Group"},
    {1935, "Fiber to Fabric"},
    {1936, "Assembler Process Group"},
    {1937, "Advanced Assembler Process Group"},
    {1938, "Unknown Super Block Process Group"},
    {1939, "Optronics Fabricator Process Group"},
    {1940, "Unknown Quantronics Process Group"},
    {1941, "Deprecated Optronics Process Group"},
    {1942, "Optronics Production"},
    {1943, "Quantronics Production (Unused)"},
    {1944, "Fiber Growth Cycle"},
    {1948, "Energy Scrap Recycling"},
    {1949, "Tech Scrap Recycling"},
    {2011, "Raw Chemicals to Chemicals"},
    {2012, "Water Purifier Process Group"},
    {2054, "Medical Supplies Production"},
    {2059, "IV Fluid Production"},
    {2416, "Plastic Production"},
    {2453, "Water Vapor to Water"},
    {2463, "Fruit Composting"},
    {2464, "Composter Process Group"},
    {2466, "Crawler Corpse Composting"},
    {2467, "Biomass Composting"},
    {2468, "Fiber Composting"},
    {2469, "Human Meat Composting"},
    {2470, "Monster Meat Composting"},
    {2471, "Processed Food Composting"},
    {2472, "Roots and Vegetables Composting"},
    {2473, "Space Food Composting"},
    {2474, "Human Corpse Composting"},
    {2476, "Chemicals to Fertilizer"},
    {2477, "Plant Water and Biomass Process"},
    {2478, "Artificial Meat Composting"},
    {2651, "Algae Dispenser Food Production"},
    {2658, "Nuts Growth Cycle"},
    {2693, "Energium to High Capacity Power"},
    {2727, "Explosive Ammunition Production"},
    {2897, "Nuts Composting"},
    {3078, "Hauler Corpse Composting"},
    {3080, "Player Human Corpse Composting"},
    {3197, "Energium to Power"},
    {3198, "Energy Rod to Power (X1 Generator)"},
    {3365, "Mild Alcohol Production"},
    {3377, "Improved Mild Alcohol Production"},
    {3379, "Hops Growth Cycle"},
    {3468, "Augmentation Parts Production"},
    {3483, "Energy Rod to Power (X2 Generator)"},
    {3484, "Energy Rod to Power (X3 Generator)"},
    {3715, "Alien Hive Corpse Composting"},
    {3716, "Evolving Alien Hive Corpse Composting"},
    {3926, "Medical Supplies to ISP"},
    {4029, "Alien Organs to Alien Enzyme"},
    {4038, "Painkiller Production"},
    {4039, "Bandage Production"},
    {4066, "Alien Organ Composting"},
    {4067, "Human Organ Composting"},
    {4083, "Parasite Corpse Composting"},
    {4087, "Energy Block Recycling"},
    {4094, "Infrablock Recycling"},
    {4095, "Soft Block Recycling"},
    {4096, "Hull Block Recycling"},
    {4098, "Tech Block Recycling"},
    {4099, "Energy Rod Recycling"},
    {4100, "Hyperfuel Recycling"},
    {4105, "Rubble Recycling (Unused)"},
    {4107, "Infester Corpse Composting"}
}


    'Public Shared ReadOnly KnownStorageObjIds As New List(Of String) From {"1740"} ' Example: Basic Storage

    ' Color Sets for uniforms (shirtSet and pantsSet values)
    ' These are the color palettes available in Space Haven
    ' Extracted from haven_annotated.xml CharacterSet definitions
    ' 
    ' Structure:
    ' - ColorSetIDs: Maps set ID -> friendly set name
    ' - ColorSetItems: Maps set ID -> list of color names (in order, index 0-based)
    ' - GetColorName: Helper function to get actual color name from set ID + index
    Public Shared ReadOnly ColorSetIDs As New Dictionary(Of Integer, String) From {
        {142, "Shirt Set: Standard Colors"},
        {143, "Pants Set: 2 Variants"},
        {372, "Shirt Set: Standard Colors"},
        {373, "Pants Set: Variant 4"},
        {375, "Pants Set: Variant 6"},
        {376, "Shirt Set: Color 5"},
        {477, "Pirate Shirts: 5 Variants"},
        {479, "Android Shirts"},
        {484, "Shirt Set: Color 8"},
        {485, "Pants Set: Red"},
        {487, "Android Pants"},
        {491, "Pants Set: Black Variants"},
        {492, "Shirt Set: Black"},
        {493, "Shirt Set: Standard Colors"},
        {730, "Pants Set: Yellow and Yellow Gray"},
        {731, "Shirt Set: Standard Colors"},
        {732, "Pants Set: Variant 5"},
        {733, "Military Shirts: Brown and Green"},
        {734, "Military Shirts: Brown and Green Variants"},
        {735, "Military Pants: Brown and Green"},
        {738, "Pants Set: Brown Variants"},
        {739, "Shirt Set: Brown and Red Variants"},
        {748, "Test Pants: 2 Variants"},
        {749, "Test Shirts"},
        {1851, "Pants Set: 14 Color Variants"},
        {1852, "Shirt Set: 20 Color Variants"},
        {1854, "Pants Set: 8 Color Variants"},
        {1855, "Shirt Set: 9 Color Variants"},
        {2360, "Shirt Set: Multiple Colors and Brown/Blue Gray"},
        {2362, "Shirt Set: 6 Variants"},
        {2363, "Shirt Set: 2 Variants"},
        {2364, "Pants Set: 20 Color Variants"},
        {4351, "Cultist Shirts: 2 Variants"},
        {4352, "Cultist Pants: 2 Variants"}
    }

    ' Maps set ID -> list of color names (in order, 0-based index)
    ' Use GetColorName(setId, index) to get the actual color name
    Private Shared _colorSetItems As New Dictionary(Of Integer, List(Of String))
    Public Shared ReadOnly Property ColorSetItems As Dictionary(Of Integer, List(Of String))
        Get
            Return _colorSetItems
        End Get
    End Property

    ' Maps set ID -> list of original instance names (for texture number extraction)
    Private Shared _colorSetInstanceNames As New Dictionary(Of Integer, List(Of String))
    Public Shared ReadOnly Property ColorSetInstanceNames As Dictionary(Of Integer, List(Of String))
        Get
            Return _colorSetInstanceNames
        End Get
    End Property

    ''' <summary>
    ''' Extracts texture number from an instance name (e.g., "shirt32s1" -> 32, "pants03s1" -> 3)
    ''' </summary>
    Public Shared Function ExtractTextureNumberFromInstance(instanceName As String) As Integer?
        If String.IsNullOrEmpty(instanceName) Then Return Nothing

        ' Pattern: shirt##s# or pants##s# where ## is the texture number
        Dim match = System.Text.RegularExpressions.Regex.Match(instanceName, "(?:shirt|pants)(\d{2})s", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If match.Success Then
            Dim num As Integer
            If Integer.TryParse(match.Groups(1).Value, num) Then
                Return num
            End If
        End If

        ' Pattern: ShirtColor# or PantsColor# where # is the texture number
        Dim colorMatch = System.Text.RegularExpressions.Regex.Match(instanceName, "(?:shirt|pants)color(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If colorMatch.Success Then
            Dim num As Integer
            If Integer.TryParse(colorMatch.Groups(1).Value, num) Then
                Return num
            End If
        End If

        ' Pattern: pants# or shirt# (single digit, no 's' suffix)
        Dim simpleMatch = System.Text.RegularExpressions.Regex.Match(instanceName, "(?:shirt|pants)(\d+)(?!s)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If simpleMatch.Success Then
            Dim num As Integer
            If Integer.TryParse(simpleMatch.Groups(1).Value, num) Then
                Return num
            End If
        End If

        Return Nothing
    End Function

    Shared Sub New()
        ' Initialize ColorSetItems with all color names for each set
        ' This allows looking up the actual color name from set ID + index (sp/sl values)

        ' Shirt Set 142: Standard Colors
        Dim shirt142Instances = New List(Of String) From {
            "104_ShirtColors_shirtColor1", "104_ShirtColors_shirtColor2", "104_ShirtColors_shirtColor3", "104_ShirtColors_shirtcolor5"
        }
        _colorSetItems(142) = shirt142Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 143: 2 Variants
        Dim pants143Instances = New List(Of String) From {
            "106_PantsColors_pants3", "106_PantsColors_pants2"
        }
        _colorSetItems(143) = pants143Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 372: Standard Colors
        Dim shirt372Instances = New List(Of String) From {
            "104_ShirtColors_shirtColor4", "104_ShirtColors_shirtColor1"
        }
        _colorSetItems(372) = shirt372Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 373: Variant 4
        Dim pants373Instances = New List(Of String) From {"106_PantsColors_pants4"}
        _colorSetItems(373) = pants373Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 375: Variant 6
        Dim pants375Instances = New List(Of String) From {"106_PantsColors_pants6"}
        _colorSetItems(375) = pants375Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 376: Color 5
        Dim shirt376Instances = New List(Of String) From {"104_ShirtColors_shirtcolor5"}
        _colorSetItems(376) = shirt376Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 477: Pirate Shirts
        Dim shirt477Instances = New List(Of String) From {
            "104_ShirtColors_pirateShirt1", "104_ShirtColors_pirateShirt5", "104_ShirtColors_pirateShirt2",
            "104_ShirtColors_pirateShirt3", "104_ShirtColors_pirateShirt4"
        }
        _colorSetItems(477) = shirt477Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 479: Android
        Dim shirt479Instances = New List(Of String) From {"2984_ShirtColors1_android01s1"}
        _colorSetItems(479) = shirt479Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 484: Color 8
        Dim shirt484Instances = New List(Of String) From {"104_ShirtColors_shirtcolor8"}
        _colorSetItems(484) = shirt484Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 485: Red
        Dim pants485Instances = New List(Of String) From {"106_PantsColors_redPants1"}
        _colorSetItems(485) = pants485Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 487: Android
        Dim pants487Instances = New List(Of String) From {"2985_PantsColors1_androidPants01s1"}
        _colorSetItems(487) = pants487Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 491: Black
        Dim pants491Instances = New List(Of String) From {"106_PantsColors_blackPants", "106_PantsColors_blackPants2"}
        _colorSetItems(491) = pants491Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 492: Black
        Dim shirt492Instances = New List(Of String) From {"104_ShirtColors_blackShirt1"}
        _colorSetItems(492) = shirt492Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 493: Standard Colors
        Dim shirt493Instances = New List(Of String) From {
            "104_ShirtColors_shirtColor1", "104_ShirtColors_shirtColor2", "104_ShirtColors_shirtColor3", "104_ShirtColors_shirtcolor5"
        }
        _colorSetItems(493) = shirt493Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 730: Yellow and Yellow Gray
        Dim pants730Instances = New List(Of String) From {
            "106_PantsColors_yellowPants", "106_PantsColors_yellowGrayPants", "106_PantsColors_yellowGrayPants2"
        }
        _colorSetItems(730) = pants730Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 731: Standard Colors
        Dim shirt731Instances = New List(Of String) From {
            "104_ShirtColors_shirtColor1", "104_ShirtColors_shirtColor2"
        }
        _colorSetItems(731) = shirt731Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 732: Variant 5
        Dim pants732Instances = New List(Of String) From {"106_PantsColors_pants5"}
        _colorSetItems(732) = pants732Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 733: Military Brown and Green
        Dim shirt733Instances = New List(Of String) From {
            "104_ShirtColors_milBrownShirt1", "104_ShirtColors_milBrownShirt2", "104_ShirtColors_milBrownShirt4", "104_ShirtColors_milBrownShirt5",
            "104_ShirtColors_milGreenShirt1", "104_ShirtColors_milGreenShirt2", "104_ShirtColors_milGreenShirt3", "104_ShirtColors_milGreenShirt4", "104_ShirtColors_milGreenShirt5"
        }
        _colorSetItems(733) = shirt733Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 734: Military Brown and Green Variants
        Dim shirt734Instances = New List(Of String) From {
            "104_ShirtColors_milBrownShirt1", "104_ShirtColors_milBrownShirt2", "104_ShirtColors_milBrownShirt3",
            "104_ShirtColors_milGreenShirt1", "104_ShirtColors_milGreenShirt2"
        }
        _colorSetItems(734) = shirt734Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 735: Military Brown and Green
        Dim pants735Instances = New List(Of String) From {
            "106_PantsColors_milBrown1", "106_PantsColors_milBrown2", "106_PantsColors_milBrown3Pants",
            "106_PantsColors_milGreenPants1", "106_PantsColors_milGreenPants2", "106_PantsColors_milGreenPants3"
        }
        _colorSetItems(735) = pants735Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 738: Brown Variants
        Dim pants738Instances = New List(Of String) From {
            "106_PantsColors_brwnPants1", "106_PantsColors_brwnPants2", "106_PantsColors_brwnPants3", "106_PantsColors_brwnPants4",
            "106_PantsColors_brwnPants5", "106_PantsColors_brwnPants6", "106_PantsColors_brwnPants7", "106_PantsColors_brwnPants8", "106_PantsColors_brwnPants9"
        }
        _colorSetItems(738) = pants738Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 739: Brown and Red Variants
        Dim shirt739Instances = New List(Of String) From {
            "104_ShirtColors_brwnShirt1", "104_ShirtColors_brwnShirt2", "104_ShirtColors_brwnShirt3", "104_ShirtColors_brwnShirt4",
            "104_ShirtColors_brwnShirt5", "104_ShirtColors_brwnShirt6",
            "104_ShirtColors_redShirt1", "104_ShirtColors_redShirt2", "104_ShirtColors_redShirt3", "104_ShirtColors_redShirt4",
            "104_ShirtColors_redShirt5", "104_ShirtColors_redShirt6", "104_ShirtColors_redShirt7", "104_ShirtColors_redShirt8",
            "2984_ShirtColors1_shirtBrown01s1", "2984_ShirtColors1_shirtBrown01s2", "2984_ShirtColors1_shirtBrown01s3", "2984_ShirtColors1_shirtBrown01s4"
        }
        _colorSetItems(739) = shirt739Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 748: Test
        Dim pants748Instances = New List(Of String) From {
            "106_PantsColors_test1", "106_PantsColors_test2"
        }
        _colorSetItems(748) = pants748Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 749: Test
        Dim shirt749Instances = New List(Of String) From {"104_ShirtColors_test1"}
        _colorSetItems(749) = shirt749Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 1851: 14 Color Variants
        Dim pants1851Instances = New List(Of String) From {
            "106_PantsColors_PantsColor1s1", "106_PantsColors_PantsColor2s1", "106_PantsColors_PantsColor3s1", "106_PantsColors_PantsColor3s2",
            "106_PantsColors_PantsColor4s1", "106_PantsColors_PantsColors4s2", "106_PantsColors_PantsColor1s2", "106_PantsColors_PantsColor2s2",
            "106_PantsColors_PantsColor3s3", "106_PantsColors_PantsColor4s3", "106_PantsColors_PantsColor5s1", "106_PantsColors_PantsColor5s2",
            "106_PantsColors_PantsColor6s1", "106_PantsColors_PantsColor6s2"
        }
        _colorSetItems(1851) = pants1851Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 1852: 20 Color Variants
        Dim shirt1852Instances = New List(Of String) From {
            "104_ShirtColors_ShirtColor1s1", "104_ShirtColors_ShirtColor1s2", "104_ShirtColors_ShirtColor2s1", "104_ShirtColors_ShirtColor2s2",
            "104_ShirtColors_ShirtColor2s3", "104_ShirtColors_ShirtColor3s1", "104_ShirtColors_ShirtColor3s2", "104_ShirtColors_ShirtColor3s3",
            "104_ShirtColors_ShirtColor4s1", "104_ShirtColors_ShirtColor4s2", "104_ShirtColors_ShirtColor4s3", "104_ShirtColors_ShirtColor5s1",
            "104_ShirtColors_ShirtColor5s2", "104_ShirtColors_ShirtColor6s1", "104_ShirtColors_ShirtColor6s2", "104_ShirtColors_ShirtColor6s3",
            "104_ShirtColors_ShirtColor7s1", "104_ShirtColors_ShirtColor7s2", "104_ShirtColors_ShirtColor7s3", "104_ShirtColors_ShirtColor1s3",
            "104_ShirtColors_ShirtColor2s3", "104_ShirtColors_ShirtColor3s3"
        }
        _colorSetItems(1852) = shirt1852Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 1854: 8 Color Variants
        Dim pants1854Instances = New List(Of String) From {
            "106_PantsColors_PantsColor1s1", "106_PantsColors_PantsColor1s2", "106_PantsColors_PantsColor1s3", "106_PantsColors_PantsColor2s1",
            "106_PantsColors_PantsColor2s2", "106_PantsColors_PantsColor3s1", "106_PantsColors_PantsColor3s2", "106_PantsColors_PantsColor4s1",
            "106_PantsColors_PantsColor4s3"
        }
        _colorSetItems(1854) = pants1854Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 1855: 9 Color Variants
        Dim shirt1855Instances = New List(Of String) From {
            "104_ShirtColors_ShirtColor5s3", "104_ShirtColors_ShirtColor5s2", "104_ShirtColors_ShirtColor5s1", "104_ShirtColors_ShirtColor4s3",
            "104_ShirtColors_ShirtColor4s2", "104_ShirtColors_ShirtColor3s1", "104_ShirtColors_ShirtColor3s2", "104_ShirtColors_ShirtColor3s3",
            "104_ShirtColors_ShirtColor4s1"
        }
        _colorSetItems(1855) = shirt1855Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 2360: Multiple Colors (most common set)
        ' Store actual instance names and parse them for better color names
        Dim shirt2360Instances = New List(Of String) From {
            "2984_ShirtColors1_shirt32s1", "2984_ShirtColors1_shirt32s2", "2984_ShirtColors1_shirt34s1", "2984_ShirtColors1_shirt34s2",
            "2984_ShirtColors1_shirt00s1", "2984_ShirtColors1_shirt00s2", "2984_ShirtColors1_shirt01s1", "2984_ShirtColors1_shirt01s2",
            "2984_ShirtColors1_shirt02s1", "2984_ShirtColors1_shirt02s2", "2984_ShirtColors1_shirt03s1", "2984_ShirtColors1_shirt03s2",
            "2984_ShirtColors1_shirt04s1", "2984_ShirtColors1_shirt04s2", "2984_ShirtColors1_shirt07s1", "2984_ShirtColors1_shirt07s2",
            "2984_ShirtColors1_shirt12s1", "2984_ShirtColors1_shirt12s2", "2984_ShirtColors1_shirt17s1", "2984_ShirtColors1_shirt17s2",
            "2984_ShirtColors1_shirt20s1", "2984_ShirtColors1_shirt20s2", "2984_ShirtColors1_shirt22s1", "2984_ShirtColors1_shirt22s2",
            "2984_ShirtColors1_shirt24s1", "2984_ShirtColors1_shirt24s2", "2984_ShirtColors1_shirt27s1", "2984_ShirtColors1_shirt27s2",
            "2984_ShirtColors1_shirt30s1", "2984_ShirtColors1_shirt30s2",
            "2984_ShirtColors1_shirtBrown01s1", "2984_ShirtColors1_shirtBrown01s2", "2984_ShirtColors1_shirtBrown01s3",
            "2984_ShirtColors1_shirtBrown01s4", "2984_ShirtColors1_shirtBrown01s5", "2984_ShirtColors1_shirtBrown01s6",
            "2984_ShirtColors1_shirtBlueGray01s1", "2984_ShirtColors1_shirtBlueGray01s2", "2984_ShirtColors1_shirtBlueGray01s3",
            "2984_ShirtColors1_shirtBlueGray01s4", "2984_ShirtColors1_shirtBlueGray01s5", "2984_ShirtColors1_shirtBlueGray01s6"
        }
        _colorSetItems(2360) = shirt2360Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()
        _colorSetInstanceNames(2360) = shirt2360Instances

        ' Shirt Set 2362: 6 Variants
        Dim shirt2362Instances = New List(Of String) From {
            "2984_ShirtColors1_shirt01-1", "2984_ShirtColors1_shirt02-1", "2984_ShirtColors1_shirt03-1",
            "2984_ShirtColors1_shirt04-1", "2984_ShirtColors1_shirt05-1", "2984_ShirtColors1_shirt06-1"
        }
        _colorSetItems(2362) = shirt2362Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Shirt Set 2363: 2 Variants
        Dim shirt2363Instances = New List(Of String) From {
            "2984_ShirtColors1_shirt00", "2984_ShirtColors1_shirt00", "2984_ShirtColors1_shirt01"
        }
        _colorSetItems(2363) = shirt2363Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 2364: 20 Color Variants (most common set)
        Dim pants2364Instances = New List(Of String) From {
            "2985_PantsColors1_pants32s1", "2985_PantsColors1_pants32s2", "2985_PantsColors1_pants34s1", "2985_PantsColors1_pants34s2",
            "2985_PantsColors1_pants00s1", "2985_PantsColors1_pants00s2", "2985_PantsColors1_pants01s1", "2985_PantsColors1_pants01s2",
            "2985_PantsColors1_pants02s1", "2985_PantsColors1_pants02s2", "2985_PantsColors1_pants03s1", "2985_PantsColors1_pants03s2",
            "2985_PantsColors1_pants04s1", "2985_PantsColors1_pants04s2", "2985_PantsColors1_pants07s1", "2985_PantsColors1_pants07s2",
            "2985_PantsColors1_pants12s1", "2985_PantsColors1_pants12s2", "2985_PantsColors1_pants17s1", "2985_PantsColors1_pants17s2",
            "2985_PantsColors1_pants20s1", "2985_PantsColors1_pants20s2", "2985_PantsColors1_pants22s1", "2985_PantsColors1_pants22s2",
            "2985_PantsColors1_pants24s1", "2985_PantsColors1_pants24s2", "2985_PantsColors1_pants27s1", "2985_PantsColors1_pants27s2",
            "2985_PantsColors1_pants30s1", "2985_PantsColors1_pants30s2"
        }
        _colorSetItems(2364) = pants2364Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()
        _colorSetInstanceNames(2364) = pants2364Instances

        ' Shirt Set 4351: Cultist
        Dim shirt4351Instances = New List(Of String) From {
            "2984_ShirtColors1_cultistShirt05s1", "2984_ShirtColors1_cultistShirt05s2"
        }
        _colorSetItems(4351) = shirt4351Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()

        ' Pants Set 4352: Cultist
        Dim pants4352Instances = New List(Of String) From {
            "2985_PantsColors1_cultistPants04s1", "2985_PantsColors1_cultistPants04s2"
        }
        _colorSetItems(4352) = pants4352Instances.Select(Function(inst) ParseInstanceNameToColor(inst)).ToList()
    End Sub

    ''' <summary>
    ''' Maps texture number to actual color name based on Space Haven's color palette
    ''' </summary>
    Public Shared Function GetColorNameFromTexture(textureNumber As Integer) As String
        ' Color mapping from Space Haven's texture system
        ' These correspond to the texture files (0.png, 1.png, etc.) and match the in-game color palette
        Dim colorMap As New Dictionary(Of Integer, String) From {
            {0, "White"}, {1, "Light Gray"}, {2, "Gray"}, {3, "Dark Gray"}, {4, "Black"},
            {5, "Light Blue"}, {6, "Blue"}, {7, "Dark Blue"}, {8, "Light Green"}, {9, "Green"},
            {10, "Dark Green"}, {11, "Light Red"}, {12, "Red"}, {13, "Dark Red"}, {14, "Light Yellow"},
            {15, "Yellow"}, {16, "Orange"}, {17, "Light Purple"}, {18, "Purple"}, {19, "Dark Purple"},
            {20, "Light Brown"}, {21, "Brown"}, {22, "Dark Brown"}, {23, "Pink"}, {24, "Cyan"},
            {25, "Teal"}, {26, "Maroon"}, {27, "Navy"}, {28, "Olive"}, {29, "Lime"},
            {30, "Coral"}, {31, "Gold"}, {32, "Silver"}, {33, "Bronze"}, {34, "Copper"},
            {35, "Beige"}, {36, "Tan"}, {37, "Khaki"}, {38, "Cream"}, {39, "Ivory"},
            {40, "Mint"}, {41, "Lavender"}
        }

        If colorMap.ContainsKey(textureNumber) Then
            Return colorMap(textureNumber)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Parses an instance name to extract a friendly color description
    ''' </summary>
    Private Shared Function ParseInstanceNameToColor(instanceName As String) As String
        If String.IsNullOrEmpty(instanceName) Then Return "Unknown"

        ' Extract variant suffix first (before processing)
        Dim variantMatch = System.Text.RegularExpressions.Regex.Match(instanceName, "s(\d+)$")
        Dim variantText = ""
        If variantMatch.Success Then
            variantText = $" Variant {variantMatch.Groups(1).Value}"
        End If

        ' Remove common prefixes
        Dim name = instanceName
        If name.Contains("_") Then
            Dim parts = name.Split("_"c)
            ' Get the last meaningful part
            name = parts(parts.Length - 1)
        End If

        ' Store original for variant checking
        Dim originalName = name

        ' Remove common suffixes like s1, s2, etc. for processing
        name = System.Text.RegularExpressions.Regex.Replace(name, "s\d+$", "")

        ' Extract color keywords
        Dim colorKeywords As New Dictionary(Of String, String) From {
            {"brown", "Brown"}, {"brwn", "Brown"}, {"brwnShirt", "Brown"}, {"brwnPants", "Brown"},
            {"red", "Red"}, {"redShirt", "Red"}, {"redPants", "Red"},
            {"yellow", "Yellow"}, {"yellowPants", "Yellow"}, {"yellowGray", "Yellow Gray"},
            {"blue", "Blue"}, {"blueGray", "Blue Gray"},
            {"green", "Green"}, {"milGreen", "Military Green"},
            {"black", "Black"}, {"blackShirt", "Black"}, {"blackPants", "Black"},
            {"gray", "Gray"}, {"grey", "Gray"},
            {"white", "White"},
            {"orange", "Orange"},
            {"purple", "Purple"},
            {"pink", "Pink"},
            {"pirate", "Pirate"},
            {"android", "Android"},
            {"cultist", "Cultist"},
            {"milBrown", "Military Brown"},
            {"test", "Test"}
        }

        Dim nameLower = name.ToLower()
        For Each kvp In colorKeywords
            If nameLower.Contains(kvp.Key.ToLower()) Then
                ' Found a color keyword
                If nameLower.Contains("shirt") Then
                    Return $"{kvp.Value} Shirt"
                ElseIf nameLower.Contains("pants") Then
                    Return $"{kvp.Value} Pants"
                Else
                    Return kvp.Value
                End If
            End If
        Next

        ' For numbered items (shirt00, shirt01, pants32, ShirtColor1, PantsColor1, etc.), extract the number
        ' These are color shades/variants without explicit color names
        ' Handle patterns like "ShirtColor1s1" or "shirt03s1" or "PantsColor2s2"
        Dim colorNumberMatch = System.Text.RegularExpressions.Regex.Match(originalName, "(?:shirt|pants)?color(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
        If colorNumberMatch.Success Then
            Dim textureNumber As Integer
            If Integer.TryParse(colorNumberMatch.Groups(1).Value, textureNumber) Then
                ' Map texture number to actual color name
                Dim colorName = GetColorNameFromTexture(textureNumber)
                If Not String.IsNullOrEmpty(colorName) Then
                    Return $"{colorName}{variantText}"
                End If
            End If

            ' Fallback if texture number not found
            Dim number = colorNumberMatch.Groups(1).Value
            If nameLower.Contains("shirt") Then
                Return $"Color #{number}{variantText}"
            ElseIf nameLower.Contains("pants") Then
                Return $"Color #{number}{variantText}"
            Else
                Return $"Color #{number}{variantText}"
            End If
        End If

        ' Handle simple numbered patterns (shirt03, pants32, etc.)
        ' Extract number from the original name (before removing suffix)
        ' These numbers correspond to texture IDs which map to actual colors
        Dim numberMatch = System.Text.RegularExpressions.Regex.Match(originalName, "(\d+)")
        If numberMatch.Success Then
            Dim textureNumber As Integer
            If Integer.TryParse(numberMatch.Groups(1).Value, textureNumber) Then
                ' Map texture number to actual color name
                Dim colorName = GetColorNameFromTexture(textureNumber)
                If Not String.IsNullOrEmpty(colorName) Then
                    Return $"{colorName}{variantText}"
                End If
            End If

            ' Fallback if texture number not found
            Dim number = numberMatch.Groups(1).Value
            If nameLower.Contains("shirt") Then
                Return $"Shade #{number}{variantText}"
            ElseIf nameLower.Contains("pants") Then
                Return $"Shade #{number}{variantText}"
            Else
                Return $"Color #{number}{variantText}"
            End If
        End If
        
        ' Fallback: clean up the name
        name = name.Replace("Shirt", "").Replace("Pants", "").Replace("shirt", "").Replace("pants", "")
        If Not String.IsNullOrEmpty(name) Then
            Return name.Substring(0, Math.Min(name.Length, 30))
        End If
        
        Return "Unknown Color"
    End Function

    ''' <summary>
    ''' Gets the actual color name from a set ID and index (sp/sl value from game.xml)
    ''' </summary>
    ''' <param name="setId">The shirtSet or pantsSet ID</param>
    ''' <param name="index">The sp (shirt) or sl (pants) index value (0-based)</param>
    ''' <returns>The friendly color name, or "Unknown" if not found</returns>
    Public Shared Function GetColorName(setId As Integer, index As Integer) As String
        If ColorSetItems.ContainsKey(setId) Then
            Dim colors = ColorSetItems(setId)
            If index >= 0 AndAlso index < colors.Count Then
                ' The stored name might be generic, try to parse it better
                Dim storedName = colors(index)
                ' If it's a generic name, try to improve it
                If storedName.Contains("Variant") OrElse storedName.Contains("Color") AndAlso Not storedName.Contains("Brown") AndAlso Not storedName.Contains("Red") Then
                    ' This is a generic name, return as-is for now
                    Return storedName
                End If
                Return storedName
            End If
        End If
        Return "Unknown"
    End Function

    ''' <summary>
    ''' Gets a display string showing both the set name and the actual color name
    ''' Example: "Shirt Set: Multiple Colors and Brown/Blue Gray - Brown Shirt 01 Variant 1"
    ''' </summary>
    ''' <param name="setId">The shirtSet or pantsSet ID</param>
    ''' <param name="index">The sp (shirt) or sl (pants) index value (0-based)</param>
    ''' <returns>A friendly display string</returns>
    Public Shared Function GetColorDisplayString(setId As Integer, index As Integer) As String
        Dim setName As String = "Unknown Set"
        If ColorSetIDs.ContainsKey(setId) Then
            setName = ColorSetIDs(setId)
        End If

        Dim colorName As String = GetColorName(setId, index)

        If colorName <> "Unknown" Then
            Return $"{setName} - {colorName}"
        Else
            Return $"{setName} (Index {index})"
        End If
    End Function


End Class
