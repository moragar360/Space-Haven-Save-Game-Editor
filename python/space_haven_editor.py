#!/usr/bin/env python3
"""
Space Haven Save Editor - Python Version
A cross-platform save editor for Space Haven, optimized for Steam Deck

Original VB.NET version by Moragar
Python port for cross-platform compatibility
"""

import sys
import os
import shutil
import logging
from pathlib import Path
from datetime import datetime
from typing import List, Optional
import xml.etree.ElementTree as ET
from xml.dom import minidom

from PyQt6.QtWidgets import (
    QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, QGridLayout,
    QTabWidget, QLabel, QLineEdit, QPushButton, QComboBox, QCheckBox,
    QTableWidget, QTableWidgetItem, QFileDialog, QMessageBox, QGroupBox,
    QSpinBox, QHeaderView, QMenuBar, QMenu
)
from PyQt6.QtCore import Qt, QSettings
from PyQt6.QtGui import QAction, QColor

from models import Character, Ship, DataProp, RelationshipInfo, StorageContainer, StorageItem


from version_analyzer import SaveFileVersionAnalyzer, SaveFileInfo

def setup_logging():
    """Configure logging to write to a dated log file adjacent to the script"""
    # Get the directory where the script is located
    script_dir = Path(__file__).parent.absolute()
    
    # Create log filename with timestamp
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_filename = f"space_haven_editor_{timestamp}.log"
    log_path = script_dir / log_filename
    
    # Configure logging
    logging.basicConfig(
        level=logging.DEBUG,
        format='%(asctime)s - %(name)s - %(levelname)s - %(funcName)s:%(lineno)d - %(message)s',
        handlers=[
            logging.FileHandler(log_path, mode='w', encoding='utf-8'),
            logging.StreamHandler()  # Also log to console
        ]
    )
    
    logger = logging.getLogger(__name__)
    logger.info("="*80)
    logger.info("Space Haven Save Editor - Logging Started")
    logger.info(f"Log file: {log_path}")
    logger.info(f"Python version: {sys.version}")
    logger.info(f"Script directory: {script_dir}")
    logger.info("="*80)
    
    return logger


class IdCollection:
    """Collection of game item IDs and names"""
    def __init__(self):
        self.load_default_ids()

    def load_default_ids(self):
        """Load default game IDs - based on Space Haven Alpha 20"""

        self.attributes = {
            210: "Bravery",
            212: "Zest",
            213: "Intelligence",
            214: "Perception"
        }

        self.skills = {
            1: "Piloting",
            2: "Mining",
            3: "Botany",
            4: "Construct",
            5: "Industry",
            6: "Medical",
            7: "Gunner",
            8: "Shielding",
            9: "Operations",
            10: "Weapons",
            12: "Logistics",
            13: "Maintenance",
            14: "Navigation",
            16: "Research"
        }

        self.traits = {
            191: "Hero",
            655: "Wimp",
            656: "Clumsy",
            1034: "Suicidal",
            1035: "Smart",
            1036: "Bloodlust",
            1037: "Antisocial",
            1038: "Needy",
            1039: "Fast Learner",
            1040: "Lazy",
            1041: "Hard Working",
            1042: "Psychopath",
            1043: "Peace-loving",
            1044: "Iron-willed",
            1045: "Spacefarer",
            1046: "Confident",
            1047: "Neurotic",
            1048: "Charming",
            1533: "Iron Stomach",
            1534: "Nyctophilia",
            1535: "Minimalist",
            1560: "Talkative",
            1562: "Gourmand",
            2082: "Alien lover"
        }

        self.conditions = {
            193: "Panicked",
            194: "Scared",
            713: "Frostbite",
            714: "First-degree burn",
            715: "Wound",
            751: "Blast injury",
            1003: "Crawler bite",
            1033: "Ate without table",
            1053: "Feeling a little hungry",
            1058: "Feeling a little unsafe",
            1059: "Slept on the floor",
            1060: "Holding it in",
            1061: "It's so dark on this spaceship",
            1062: "Ate the meat of a human being",
            1063: "Wearing spacesuit",
            1064: "Feeling adventurous",
            1065: "Feeling meaningful",
            1066: "Feeling loved",
            # Add more as needed
        }

        # Complete storage items list from VB.NET IdCollection
        self.storage_items = {
            15: "Root vegetables",
            16: "Water",
            40: "Ice",
            71: "Bio Matter",
            127: "Rubble",
            157: "Base Metals",
            158: "Energium",
            162: "Infrablock",
            169: "Noble Metals",
            170: "Carbon",
            171: "Raw Chemicals",
            172: "Hyperium",
            173: "Electronic Component",
            174: "Energy Rod",
            175: "Plastics",
            176: "Chemicals",
            177: "Fabrics",
            178: "Hyperfuel",
            179: "Processed Food",
            706: "Fruits",
            707: "Artificial Meat",
            712: "Space Food",
            725: "Assault Rifle",
            728: "SMG",
            729: "Shotgun",
            760: "Five-Seven Pistol",
            930: "Techblock",
            984: "Monster Meat",
            985: "Human Meat",
            1152: "Sentry Gun X1",
            1759: "Hull Block",
            1873: "Infra Scrap",
            1874: "Soft Scrap",
            1886: "Hull Scrap",
            1919: "Energy Block",
            1920: "Superblock",
            1921: "Soft Block",
            1922: "Steel Plates",
            1924: "Optronics Component",
            1925: "Quantronics Component",
            1926: "Energy Cell",
            1932: "Fibers",
            1946: "Tech Scrap",
            1947: "Energy Scrap",
            1954: "Human Corpse",
            1955: "Monster Corpse",
            2053: "Medical Supplies",
            2058: "IV Fluid",
            2475: "Fertilizer",
            2657: "Nuts and Seeds",
            2715: "Explosive Ammunition",
            3069: "Laser Rifle",
            3070: "Laser Pistol",
            3071: "Plasma Cuttergun",
            3072: "Plasma Rifle",
            3378: "Grain and Hops",
            3384: "Armored Vest",
            3386: "Remote Control",
            3388: "Oxygen Tank",
            3419: "Augmentation Parts",
            3960: "Flamethrower (Weapon Attachment)",
            3961: "Stun Rifle",
            3962: "Stun Pistol",
            3967: "Explosive Grenade Launcher (Weapon Attachment)",
            3968: "Basic Scope (Weapon Attachment)",
            3969: "Tactical Grip (Weapon Attachment)",
            4005: "Painkillers",
            4006: "Combat Stimulant",
            4007: "Bandage",
            4030: "Nano Wound Dressing",
            4040: "Small Breach Charge",
            4065: "Space Suit Oxygen Extender",
            4076: "Incendiary Grenade Launcher (Weapon Attachment)",
        }

    def get_attribute_name(self, attr_id: int) -> str:
        return self.attributes.get(attr_id, f"Attribute {attr_id}")

    def get_skill_name(self, skill_id: int) -> str:
        return self.skills.get(skill_id, f"Skill {skill_id}")

    def get_trait_name(self, trait_id: int) -> str:
        return self.traits.get(trait_id, f"Trait {trait_id}")

    def get_condition_name(self, condition_id: int) -> str:
        return self.conditions.get(condition_id, f"Condition {condition_id}")

    def get_storage_item_name(self, item_id: int) -> str:
        return self.storage_items.get(item_id, f"Item {item_id}")


class SpaceHavenEditor(QMainWindow):
    """Main application window"""
    
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(__name__)
        self.logger.info("Initializing SpaceHavenEditor main window")

        self.setWindowTitle("Space Haven Save Editor - Python Edition")
        self.setMinimumSize(1000, 700)

        # Data storage
        self.current_file_path: str = ""
        self.current_folder_path: Optional[Path] = None  # NEW: Track folder
        self.xml_tree: Optional[ET.ElementTree] = None
        self.xml_root: Optional[ET.Element] = None
        self.characters: List[Character] = []
        self.ships: List[Ship] = []
        self.id_collection = IdCollection()

        # Version analyzer
        self.version_analyzer = SaveFileVersionAnalyzer(self.logger)
        self.current_save_info: Optional[SaveFileInfo] = None

        # Settings (legacy)
        self.settings = QSettings("SpaceHavenEditor", "SaveEditor")
        self.backup_enabled = self.settings.value("backup_on_open", True, type=bool)
        
        # NEW: Save folder and backup management
        from save_manager import SaveFolderConfig, BackupManager, SaveFolderInfo
        from pathlib import Path
        self.save_config = SaveFolderConfig()
        self.backup_manager = BackupManager(
            Path(self.save_config.config["backup_folder"]),
            max_days=self.save_config.config["backup_count"]
        )

        self.logger.info(f"Backup on open: {self.backup_enabled}")
        self.logger.info(f"Steam folder enabled: {self.save_config.config['use_steam_folder']}")

        # Setup UI
        self.init_ui()

        # Show first-run setup if needed
        if self.save_config.is_first_run:
            self.show_first_run_setup()

        self.logger.info("SpaceHavenEditor initialization complete")

        # Storage editing state
        self.current_storage_container: Optional[StorageContainer] = None
        
    def init_ui(self):
        """Initialize the user interface"""
        # Create menu bar
        self.create_menu_bar()
        
        # Create central widget and main layout
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)
        
        # Global settings section
        global_group = self.create_global_settings_group()
        main_layout.addWidget(global_group)
        
        # Ship selection
        ship_layout = QHBoxLayout()
        ship_layout.addWidget(QLabel("Selected Ship:"))
        self.ship_combo = QComboBox()
        self.ship_combo.currentIndexChanged.connect(self.on_ship_selected)
        ship_layout.addWidget(self.ship_combo)
        ship_layout.addStretch()
        main_layout.addLayout(ship_layout)
        
        # Tab widget for different sections
        self.tabs = QTabWidget()
        
        # Ship info tab
        self.ship_tab = self.create_ship_tab()
        self.tabs.addTab(self.ship_tab, "Ship Info")
        
        # Crew tab
        self.crew_tab = self.create_crew_tab()
        self.tabs.addTab(self.crew_tab, "Crew")
        
        # Storage tab
        self.storage_tab = self.create_storage_tab()
        self.tabs.addTab(self.storage_tab, "Storage")
        
        main_layout.addWidget(self.tabs)
        
    def create_menu_bar(self):
        """Create the menu bar"""
        menubar = self.menuBar()
        
        # File menu
        file_menu = menubar.addMenu("&File")
        
        open_action = QAction("&Open Save File...", self)
        open_action.setShortcut("Ctrl+O")
        open_action.triggered.connect(self.open_file)
        file_menu.addAction(open_action)
        
        save_action = QAction("&Save", self)
        save_action.setShortcut("Ctrl+S")
        save_action.triggered.connect(self.save_file)
        file_menu.addAction(save_action)
        
        file_menu.addSeparator()
        
        settings_action = QAction("S&ettings...", self)
        settings_action.triggered.connect(self.show_settings)
        file_menu.addAction(settings_action)
        
        file_menu.addSeparator()
        
        exit_action = QAction("E&xit", self)
        exit_action.setShortcut("Ctrl+Q")
        exit_action.triggered.connect(self.close)
        file_menu.addAction(exit_action)
        
        # Help menu
        help_menu = menubar.addMenu("&Help")
        
        about_action = QAction("&About", self)
        about_action.triggered.connect(self.show_about)
        help_menu.addAction(about_action)
        
    def create_global_settings_group(self) -> QGroupBox:
        """Create the global settings group"""
        from PyQt6.QtWidgets import QFrame
        
        group = QGroupBox("Global Settings")
        layout = QHBoxLayout()

        # Version info
        layout.addWidget(QLabel("Version:"))
        self.version_label = QLabel("Unknown")
        self.version_label.setStyleSheet("font-weight: bold; color: #0066cc;")
        self.version_label.setMinimumWidth(100)
        layout.addWidget(self.version_label)

        # Add separator
        separator = QFrame()
        separator.setFrameShape(QFrame.Shape.VLine)
        separator.setFrameShadow(QFrame.Shadow.Sunken)
        layout.addWidget(separator)

        # Credits
        layout.addWidget(QLabel("Credits:"))
        self.credits_input = QLineEdit()
        self.credits_input.setMaximumWidth(150)
        layout.addWidget(self.credits_input)

        # Prestige Points
        layout.addWidget(QLabel("Prestige Points:"))
        self.prestige_input = QLineEdit()
        self.prestige_input.setMaximumWidth(150)
        layout.addWidget(self.prestige_input)

        # Sandbox mode
        self.sandbox_check = QCheckBox("Sandbox Mode")
        layout.addWidget(self.sandbox_check)

        # Update button
        update_btn = QPushButton("Update Global Settings")
        update_btn.clicked.connect(self.update_global_settings)
        layout.addWidget(update_btn)

        layout.addStretch()
        group.setLayout(layout)
        return group
        
    def create_ship_tab(self) -> QWidget:
        """Create the ship information tab"""
        widget = QWidget()
        layout = QVBoxLayout(widget)
        
        # Ship dimensions
        dim_layout = QHBoxLayout()
        dim_layout.addWidget(QLabel("Ship Dimensions:"))
        
        dim_layout.addWidget(QLabel("Width:"))
        self.ship_width = QSpinBox()
        self.ship_width.setRange(1, 100)
        dim_layout.addWidget(self.ship_width)
        
        dim_layout.addWidget(QLabel("Height:"))
        self.ship_height = QSpinBox()
        self.ship_height.setRange(1, 100)
        dim_layout.addWidget(self.ship_height)
        
        update_size_btn = QPushButton("Update Ship Size")
        update_size_btn.clicked.connect(self.update_ship_size)
        dim_layout.addWidget(update_size_btn)
        
        dim_layout.addStretch()
        layout.addLayout(dim_layout)
        
        # Ship info display
        self.ship_info = QLabel("Select a ship to view details")
        layout.addWidget(self.ship_info)
        
        layout.addStretch()
        return widget
        
    def create_crew_tab(self) -> QWidget:
        """Create the crew management tab"""
        from PyQt6.QtWidgets import QListWidget, QSplitter, QScrollArea
        from PyQt6.QtCore import Qt

        widget = QWidget()
        layout = QVBoxLayout(widget)

        # Create a horizontal splitter for crew list and details
        splitter = QSplitter(Qt.Orientation.Horizontal)

        # Left side: Crew list
        crew_list_widget = QWidget()
        crew_list_layout = QVBoxLayout(crew_list_widget)

        crew_list_layout.addWidget(QLabel("Crew Members:"))

        self.crew_list = QListWidget()
        self.crew_list.currentRowChanged.connect(self.on_crew_selected)
        crew_list_layout.addWidget(self.crew_list)

        add_crew_btn = QPushButton("Add Crew Member")
        add_crew_btn.clicked.connect(self.add_crew_member)
        crew_list_layout.addWidget(add_crew_btn)

        splitter.addWidget(crew_list_widget)

        # Right side: Crew details in scrollable area
        details_widget = QWidget()
        details_layout = QVBoxLayout(details_widget)
        details_layout.setSpacing(10)

        # Header with crew name editor
        header_layout = QHBoxLayout()

        name_label = QLabel("Name:")
        name_label.setStyleSheet("font-size: 14px; font-weight: bold;")
        header_layout.addWidget(name_label)

        self.crew_name_edit = QLineEdit()
        self.crew_name_edit.setPlaceholderText("Select a crew member")
        self.crew_name_edit.setStyleSheet("font-size: 14px; font-weight: bold;")
        self.crew_name_edit.setMaximumWidth(300)
        self.crew_name_edit.setReadOnly(True)  # Start as read-only until crew selected
        self.crew_name_edit.textChanged.connect(self.on_crew_name_changed)
        header_layout.addWidget(self.crew_name_edit)

        header_layout.addStretch()
        details_layout.addLayout(header_layout)

        # Homeship selector
        homeship_layout = QHBoxLayout()
        homeship_label = QLabel("Homeship:")
        homeship_label.setStyleSheet("font-size: 12px; font-weight: bold;")
        homeship_layout.addWidget(homeship_label)

        self.homeship_combo = QComboBox()
        self.homeship_combo.setMinimumWidth(200)
        self.homeship_combo.currentIndexChanged.connect(self.on_homeship_changed)
        homeship_layout.addWidget(self.homeship_combo)

        homeship_layout.addStretch()
        details_layout.addLayout(homeship_layout)

        # Attributes section
        attr_header_layout = QHBoxLayout()
        attr_label = QLabel("Attributes")
        attr_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        attr_header_layout.addWidget(attr_label)
        attr_header_layout.addStretch()

        self.max_all_attr_btn = QPushButton("\u2308 Max All Attributes")
        self.max_all_attr_btn.clicked.connect(self.max_all_attributes)
        attr_header_layout.addWidget(self.max_all_attr_btn)

        details_layout.addLayout(attr_header_layout)

        self.attributes_container = QWidget()
        self.attributes_layout = QVBoxLayout(self.attributes_container)
        self.attributes_layout.setSpacing(2)
        self.attributes_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.attributes_container)

        # Skills section
        skills_header_layout = QHBoxLayout()
        skills_label = QLabel("Skills")
        skills_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        skills_header_layout.addWidget(skills_label)
        skills_header_layout.addStretch()

        # Note: Only "Max All Skills" button - learning values are calculated, not editable
        self.max_all_skills_btn = QPushButton("\u2308 Max All Skills to Learning")
        self.max_all_skills_btn.setToolTip("Set all skills to their maximum learning potential")
        self.max_all_skills_btn.clicked.connect(self.max_all_skills_to_learning)
        skills_header_layout.addWidget(self.max_all_skills_btn)

        details_layout.addLayout(skills_header_layout)

        self.skills_container = QWidget()
        self.skills_layout = QVBoxLayout(self.skills_container)
        self.skills_layout.setSpacing(2)
        self.skills_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.skills_container)

        # Traits section
        traits_label = QLabel("Traits")
        traits_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(traits_label)

        # Add trait controls
        add_trait_widget = QWidget()
        add_trait_layout = QHBoxLayout(add_trait_widget)
        add_trait_layout.setContentsMargins(10, 5, 10, 5)
        self.trait_combo = QComboBox()
        self.populate_trait_combo()
        add_trait_layout.addWidget(QLabel("Add Trait:"))
        add_trait_layout.addWidget(self.trait_combo, 1)
        add_trait_btn = QPushButton("Add")
        add_trait_btn.clicked.connect(self.on_add_trait)
        add_trait_layout.addWidget(add_trait_btn)
        details_layout.addWidget(add_trait_widget)

        self.traits_container = QWidget()
        self.traits_layout = QVBoxLayout(self.traits_container)
        self.traits_layout.setSpacing(2)
        self.traits_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.traits_container)

        # Conditions section
        conditions_label = QLabel("Conditions")
        conditions_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(conditions_label)

        self.conditions_container = QWidget()
        self.conditions_layout = QVBoxLayout(self.conditions_container)
        self.conditions_layout.setSpacing(2)
        self.conditions_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.conditions_container)

        # Relationships section
        relationships_label = QLabel("Relationships")
        relationships_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(relationships_label)

        self.relationships_container = QWidget()
        self.relationships_layout = QVBoxLayout(self.relationships_container)
        self.relationships_layout.setSpacing(2)
        self.relationships_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.relationships_container)

        # Job Priorities section
        job_priorities_label = QLabel("Job Priorities (Occupation)")
        job_priorities_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(job_priorities_label)

        self.job_priorities_container = QWidget()
        self.job_priorities_layout = QVBoxLayout(self.job_priorities_container)
        self.job_priorities_layout.setSpacing(2)
        self.job_priorities_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.job_priorities_container)

        # Appearance section
        appearance_label = QLabel("Appearance")
        appearance_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(appearance_label)

        # Appearance editor controls
        appearance_editor = QWidget()
        appearance_editor_layout = QGridLayout(appearance_editor)
        appearance_editor_layout.setContentsMargins(10, 5, 10, 5)
        appearance_editor_layout.setSpacing(5)

        # Skin tone selector
        appearance_editor_layout.addWidget(QLabel("Skin Tone:"), 0, 0)
        self.skin_combo = QComboBox()
        self.skin_combo.addItem("Light skin", "744")
        self.skin_combo.addItem("Dark skin", "743")
        self.skin_combo.addItem("Medium skin", "745")
        self.skin_combo.currentIndexChanged.connect(self.on_appearance_changed)
        appearance_editor_layout.addWidget(self.skin_combo, 0, 1)

        # Sleeve length selector
        appearance_editor_layout.addWidget(QLabel("Sleeves:"), 1, 0)
        self.sleeve_combo = QComboBox()
        self.sleeve_combo.addItem("Short sleeves", "false")
        self.sleeve_combo.addItem("Long sleeves", "true")
        self.sleeve_combo.currentIndexChanged.connect(self.on_appearance_changed)
        appearance_editor_layout.addWidget(self.sleeve_combo, 1, 1)

        # Gloves selector
        appearance_editor_layout.addWidget(QLabel("Gloves:"), 2, 0)
        self.gloves_combo = QComboBox()
        self.gloves_combo.addItem("With gloves", "false")
        self.gloves_combo.addItem("No gloves", "true")
        self.gloves_combo.currentIndexChanged.connect(self.on_appearance_changed)
        appearance_editor_layout.addWidget(self.gloves_combo, 2, 1)

        # Shirt color (numeric input)
        appearance_editor_layout.addWidget(QLabel("Shirt Color ID:"), 3, 0)
        self.shirt_color_input = QLineEdit()
        self.shirt_color_input.setPlaceholderText("e.g., 2360")
        self.shirt_color_input.textChanged.connect(self.on_color_id_changed)
        appearance_editor_layout.addWidget(self.shirt_color_input, 3, 1)

        # Pants color (numeric input)
        appearance_editor_layout.addWidget(QLabel("Pants Color ID:"), 4, 0)
        self.pants_color_input = QLineEdit()
        self.pants_color_input.setPlaceholderText("e.g., 2364")
        self.pants_color_input.textChanged.connect(self.on_color_id_changed)
        appearance_editor_layout.addWidget(self.pants_color_input, 4, 1)

        details_layout.addWidget(appearance_editor)

        self.appearance_container = QWidget()
        self.appearance_layout = QVBoxLayout(self.appearance_container)
        self.appearance_layout.setSpacing(2)
        self.appearance_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.appearance_container)

        # Equipment/Loadout section
        equipment_label = QLabel("Equipment & Loadout")
        equipment_label.setStyleSheet("font-size: 14px; font-weight: bold; color: #0066cc;")
        details_layout.addWidget(equipment_label)

        self.equipment_container = QWidget()
        self.equipment_layout = QVBoxLayout(self.equipment_container)
        self.equipment_layout.setSpacing(2)
        self.equipment_layout.setContentsMargins(10, 0, 0, 0)
        details_layout.addWidget(self.equipment_container)

        details_layout.addStretch()

        # Wrap details in scroll area
        scroll_area = QScrollArea()
        scroll_area.setWidget(details_widget)
        scroll_area.setWidgetResizable(True)
        splitter.addWidget(scroll_area)

        # Set splitter proportions (30% list, 70% details)
        splitter.setSizes([300, 700])

        layout.addWidget(splitter)

        # Store reference to current character
        self.current_character: Optional[Character] = None
        self.attribute_editors = {}
        self.skill_editors = {}

        return widget
        
    def create_storage_tab(self) -> QWidget:
        """Create the storage management tab"""
        widget = QWidget()
        layout = QVBoxLayout(widget)

        # Container selection
        container_layout = QHBoxLayout()
        container_layout.addWidget(QLabel("Storage Container:"))
        self.container_combo = QComboBox()
        self.container_combo.currentIndexChanged.connect(self.on_container_selected)
        container_layout.addWidget(self.container_combo)

        # Storage info label
        self.storage_info_label = QLabel("")
        self.storage_info_label.setStyleSheet("font-size: 11px;")
        container_layout.addWidget(self.storage_info_label)

        container_layout.addStretch()
        layout.addLayout(container_layout)

        # Storage items table
        self.storage_table = QTableWidget()
        self.storage_table.setColumnCount(4)
        self.storage_table.setHorizontalHeaderLabels(["Item ID", "Item Name", "Quantity", "Actions"])
        self.storage_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.ResizeMode.ResizeToContents)
        self.storage_table.horizontalHeader().setSectionResizeMode(1, QHeaderView.ResizeMode.Stretch)
        self.storage_table.horizontalHeader().setSectionResizeMode(2, QHeaderView.ResizeMode.ResizeToContents)
        self.storage_table.horizontalHeader().setSectionResizeMode(3, QHeaderView.ResizeMode.ResizeToContents)
        self.storage_table.itemChanged.connect(self.on_storage_item_changed)
        layout.addWidget(self.storage_table)

        # Add items section
        add_group = QGroupBox("Add Items")
        add_layout = QVBoxLayout(add_group)

        # Resupply presets
        resupply_layout = QHBoxLayout()
        resupply_layout.addWidget(QLabel("Resupply Presets:"))
        
        infra_1_btn = QPushButton("Infra +1")
        infra_1_btn.setToolTip("Add 1 of each: Infrablock, Soft Block, Techblock, Energy Block, Hull Block, Superblock")
        infra_1_btn.clicked.connect(lambda: self.resupply_preset("infra", 1))
        resupply_layout.addWidget(infra_1_btn)
        
        infra_5_btn = QPushButton("Infra +5")
        infra_5_btn.setToolTip("Add 5 of each: Infrablock, Soft Block, Techblock, Energy Block, Hull Block, Superblock")
        infra_5_btn.clicked.connect(lambda: self.resupply_preset("infra", 5))
        resupply_layout.addWidget(infra_5_btn)
        
        infra_10_btn = QPushButton("Infra +10")
        infra_10_btn.setToolTip("Add 10 of each: Infrablock, Soft Block, Techblock, Energy Block, Hull Block, Superblock")
        infra_10_btn.clicked.connect(lambda: self.resupply_preset("infra", 10))
        resupply_layout.addWidget(infra_10_btn)
        
        life_5_btn = QPushButton("Life Support +5")
        life_5_btn.setToolTip("Add 5 of each: Water, Ice, Root vegetables, Fruits, Artificial Meat, Nuts and Seeds")
        life_5_btn.clicked.connect(lambda: self.resupply_preset("life_support", 5))
        resupply_layout.addWidget(life_5_btn)
        
        life_10_btn = QPushButton("Life Support +10")
        life_10_btn.setToolTip("Add 10 of each: Water, Ice, Root vegetables, Fruits, Artificial Meat, Nuts and Seeds")
        life_10_btn.clicked.connect(lambda: self.resupply_preset("life_support", 10))
        resupply_layout.addWidget(life_10_btn)
        
        # TODO: Add Ship preset when we find Energium/Hyperium IDs
        # ship_btn = QPushButton("Ship")
        # ship_btn.setToolTip("Add Energium and Hyperium")
        # ship_btn.clicked.connect(lambda: self.resupply_preset("ship", 5))
        # resupply_layout.addWidget(ship_btn)
        
        resupply_layout.addStretch()
        add_layout.addLayout(resupply_layout)

        # Item selector
        item_select_layout = QHBoxLayout()
        item_select_layout.addWidget(QLabel("Individual Item:"))
        self.add_item_combo = QComboBox()
        self.populate_add_item_combo()
        item_select_layout.addWidget(self.add_item_combo, 1)
        add_layout.addLayout(item_select_layout)

        # Quick add buttons
        quick_add_layout = QHBoxLayout()
        quick_add_layout.addWidget(QLabel("Quick Add:"))

        add_1_btn = QPushButton("+1")
        add_1_btn.clicked.connect(lambda: self.quick_add_item(1))
        quick_add_layout.addWidget(add_1_btn)

        add_5_btn = QPushButton("+5")
        add_5_btn.clicked.connect(lambda: self.quick_add_item(5))
        quick_add_layout.addWidget(add_5_btn)

        add_10_btn = QPushButton("+10")
        add_10_btn.clicked.connect(lambda: self.quick_add_item(10))
        quick_add_layout.addWidget(add_10_btn)

        quick_add_layout.addStretch()
        add_layout.addLayout(quick_add_layout)

        layout.addWidget(add_group)

        return widget
        
    def open_file(self):
        """Open a save folder"""
        from save_manager import SaveFolderInfo
        from pathlib import Path
        
        self.logger.info("Opening folder dialog")
        
        # Determine initial directory
        initial_dir = self.save_config.get_default_folder()
        if not initial_dir or not initial_dir.exists():
            initial_dir = Path.home()
        
        self.logger.info(f"Initial directory: {initial_dir}")
        
        # Open folder dialog
        folder_path = QFileDialog.getExistingDirectory(
            self,
            "Select Space Haven Save Folder",
            str(initial_dir),
            QFileDialog.Option.ShowDirsOnly
        )
        
        if not folder_path:
            self.logger.info("Folder dialog cancelled by user")
            return
        
        folder_path = Path(folder_path)
        self.logger.info(f"Selected folder: {folder_path}")
        
        # Parse folder info
        folder_info = SaveFolderInfo(folder_path)
        
        if not folder_info.is_valid_save():
            QMessageBox.critical(
                self,
                "Invalid Save Folder",
                f"The selected folder does not contain a valid Space Haven save.\n\n"
                f"Expected files:\n"
                f"  save/game - {'Found' if folder_info.game_file_exists else 'Missing'}\n"
                f"  save/info - {'Found' if folder_info.info_file_exists else 'Missing'}"
            )
            return
        
        # Remember this folder
        self.save_config.set_last_used_folder(str(folder_path))
        
        # Handle backup based on mode
        backup_mode = self.save_config.config.get("auto_backup", False)
        
        if backup_mode == "auto":
            # Automatic backup
            self.logger.info("Creating automatic backup...")
            backup_path = self.backup_manager.create_backup(folder_path)
            if backup_path:
                self.logger.info(f"Backup created: {backup_path.name}")
                
                # Check if we should prune old backups
                dates = self.backup_manager.get_backup_dates()
                max_days = self.save_config.config.get("backup_count", 3)
                
                if len(dates) > max_days:
                    # Ask about pruning
                    to_delete = self.backup_manager.prune_old_backups(max_days, dry_run=True)
                    total_size = sum(p.stat().st_size for p in to_delete)
                    size_mb = total_size / (1024 * 1024)
                    
                    reply = QMessageBox.question(
                        self,
                        "Prune Old Backups?",
                        f"You have backups from {len(dates)} days.\n"
                        f"Delete {len(to_delete)} old backups ({size_mb:.1f} MB)?",
                        QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
                    )
                    
                    if reply == QMessageBox.StandardButton.Yes:
                        deleted = self.backup_manager.prune_old_backups(max_days)
                        self.logger.info(f"Pruned {len(deleted)} old backups")
            else:
                self.logger.warning("Backup creation failed")
                
        elif backup_mode == "manual":
            # Ask user
            reply = QMessageBox.question(
                self,
                "Create Backup?",
                f"Create a backup of this save before opening?\n\n"
                f"Save: {folder_info.get_display_name()}",
                QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
            )
            
            if reply == QMessageBox.StandardButton.Yes:
                # Check if backup exists today
                from datetime import datetime
                today = datetime.now().strftime("%Y%m%d")
                existing = self.backup_manager._find_backups_for_date(today)
                
                force_new = False
                if existing:
                    reply2 = QMessageBox.question(
                        self,
                        "Backup Exists",
                        f"A backup already exists for today:\n{existing[0].name}\n\n"
                        f"Create another backup?",
                        QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
                    )
                    force_new = (reply2 == QMessageBox.StandardButton.Yes)
                    if not force_new:
                        self.logger.info("Using existing backup")
                
                if force_new or not existing:
                    backup_path = self.backup_manager.create_backup(folder_path, force_new=force_new)
                    if backup_path:
                        self.logger.info(f"Manual backup created: {backup_path.name}")
        
        # Reset application state
        self.logger.info("Resetting application state")
        self.reset_application_state()
        
        # Load the folder
        try:
            self.current_folder_path = folder_path
            game_file = folder_path / "save" / "game"
            self.current_file_path = str(game_file)
            
            self.logger.info(f"Loading save from folder: {folder_path}")
            self.logger.info(f"Version: {folder_info.version}")
            
            self.load_save_file(str(game_file))
            self.current_save_info = folder_info  # Store folder info
            
            # Update version display
            if folder_info.version:
                self.version_label.setText(folder_info.version)
            
            self.setWindowTitle(f"Space Haven Save Editor - {folder_path.name} (v{folder_info.version})")
            self.logger.info("Save loaded successfully")
            QMessageBox.information(
                self,
                "Success",
                f"Save loaded successfully!\n\nVersion: {folder_info.version or 'Unknown'}"
            )
        except Exception as e:
            self.logger.error(f"Failed to load save: {str(e)}", exc_info=True)
            QMessageBox.critical(self, "Error", f"Failed to load save:\n{str(e)}")
            self.reset_application_state()
            
    def create_backup(self, file_path: str):
        """Create a backup of the save file"""
        try:
            source_dir = Path(file_path).parent
            parent_dir = source_dir.parent
            savegames_dir = parent_dir.parent
            
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            backup_name = f"{parent_dir.name}_{source_dir.name}_backup_{timestamp}"
            backup_path = savegames_dir / backup_name
            
            self.logger.info(f"Creating backup from {source_dir} to {backup_path}")
            shutil.copytree(source_dir, backup_path)
            self.logger.info("Backup created successfully")
            
        except Exception as e:
            self.logger.error(f"Backup failed: {str(e)}", exc_info=True)
            QMessageBox.warning(
                self,
                "Backup Failed",
                f"Failed to create backup:\n{str(e)}\n\nContinuing to load original file."
            )
            
    def load_save_file(self, file_path: str):
        """Load and parse the save file"""
        self.logger.info("="*60)
        self.logger.info("Starting to load save file")
        self.logger.info(f"File path: {file_path}")
        self.logger.info(f"File size: {Path(file_path).stat().st_size} bytes")

        # Parse XML
        self.logger.info("Parsing XML...")
        try:
            self.xml_tree = ET.parse(file_path)
            self.xml_root = self.xml_tree.getroot()
            self.logger.info(f"XML root tag: {self.xml_root.tag}")
            self.logger.info(f"XML root attributes: {self.xml_root.attrib}")
        except Exception as e:
            self.logger.error(f"XML parsing failed: {str(e)}", exc_info=True)
            raise

        if self.xml_root.tag != "game":
            error_msg = f"Invalid save file: root element is '{self.xml_root.tag}', expected 'game'"
            self.logger.error(error_msg)
            raise ValueError(error_msg)

        # Analyze version information
        self.logger.info("Analyzing save file version...")
        try:
            self.current_save_info = self.version_analyzer.analyze_save_file(Path(file_path))
            version_str = self.current_save_info.version or "Unknown"
            self.version_label.setText(version_str)
            self.logger.info(f"Detected version: {version_str}")

            # Log additional info
            if self.current_save_info.id_counter:
                self.logger.info(f"  ID Counter: {self.current_save_info.id_counter}")
            if self.current_save_info.sector_count:
                self.logger.info(f"  Sector Count: {self.current_save_info.sector_count}")
        except Exception as e:
            self.logger.warning(f"Version analysis failed: {e}")
            self.version_label.setText("Unknown")

        # Log XML structure overview
        self.logger.info("XML structure overview:")
        for child in self.xml_root:
            self.logger.info(f"  - {child.tag} (attributes: {list(child.attrib.keys())})")

        # Load different sections
        self.logger.info("Loading global settings...")
        self.load_global_settings()

        self.logger.info("Loading ships...")
        self.load_ships()

        self.logger.info("Loading characters...")
        self.load_characters()

        # Populate UI
        if self.ships:
            self.logger.info(f"Populating ship combo with {len(self.ships)} ships")
            self.ship_combo.clear()
            for ship in self.ships:
                self.ship_combo.addItem(str(ship), ship)
                self.logger.debug(f"  Added ship: {ship}")
            self.ship_combo.setCurrentIndex(0)
        else:
            self.logger.warning("No ships found in save file!")

        self.logger.info("="*60)
            
    def load_global_settings(self):
        """Load global settings from XML"""
        if self.xml_root is None:
            self.logger.warning("load_global_settings: xml_root is None")
            return
        
        self.logger.info("Loading global settings...")
            
        # Load credits
        bank_elem = self.xml_root.find("playerBank")
        if bank_elem is not None:
            credits = bank_elem.get("ca", "0")
            self.credits_input.setText(credits)
            self.logger.info(f"  Credits: {credits}")
            self.logger.debug(f"  playerBank attributes: {bank_elem.attrib}")
        else:
            self.logger.warning("  playerBank element not found")
        
        # Load prestige points
        try:
            quest_lines1 = self.xml_root.find("questLines")
            if quest_lines1 is not None:
                self.logger.debug(f"  Found questLines (outer)")
                quest_lines2 = quest_lines1.find("questLines")
                if quest_lines2 is not None:
                    self.logger.debug(f"  Found questLines (inner)")
                    exodus_fleet_found = False
                    for elem in quest_lines2.findall("l"):
                        elem_type = elem.get("type")
                        self.logger.debug(f"    Quest line type: {elem_type}")
                        if elem_type == "ExodusFleet":
                            prestige = elem.get("playerPrestigePoints", "0")
                            self.prestige_input.setText(prestige)
                            self.logger.info(f"  Prestige Points: {prestige}")
                            exodus_fleet_found = True
                            break
                    if not exodus_fleet_found:
                        self.logger.warning("  ExodusFleet quest line not found")
                else:
                    self.logger.warning("  Inner questLines element not found")
            else:
                self.logger.warning("  Outer questLines element not found")
        except Exception as e:
            self.logger.error(f"  Error loading prestige points: {str(e)}", exc_info=True)
            self.prestige_input.setText("0")
        
        # Load sandbox mode
        settings_elem = self.xml_root.find("settings")
        if settings_elem is not None:
            self.logger.debug("  Found settings element")
            diff_elem = settings_elem.find("diff")
            if diff_elem is not None:
                sandbox = diff_elem.get("sandbox", "false").lower() == "true"
                self.sandbox_check.setChecked(sandbox)
                self.logger.info(f"  Sandbox Mode: {sandbox}")
                self.logger.debug(f"  diff attributes: {diff_elem.attrib}")
            else:
                self.logger.warning("  diff element not found")
        else:
            self.logger.warning("  settings element not found")
                
    def load_ships(self):
        """Load ships from XML"""
        self.ships.clear()
        
        if self.xml_root is None:
            self.logger.warning("load_ships: xml_root is None")
            return
        
        self.logger.info("Searching for ship elements...")
        ship_elements = self.xml_root.findall(".//ship")
        self.logger.info(f"Found {len(ship_elements)} ship elements")
        
        for idx, ship_elem in enumerate(ship_elements):
            self.logger.info(f"Processing ship {idx + 1}/{len(ship_elements)}")
            self.logger.debug(f"  Ship element attributes: {ship_elem.attrib}")
            
            ship = Ship()
            ship.sid = int(ship_elem.get("sid", "0"))
            ship.sname = ship_elem.get("sname", "Unknown Ship")
            ship.sx = int(ship_elem.get("sx", "0"))
            ship.sy = int(ship_elem.get("sy", "0"))
            
            self.logger.info(f"  Ship ID: {ship.sid}, Name: {ship.sname}, Size: {ship.sx}x{ship.sy}")
            
            # Load storage containers
            self.logger.info(f"  Loading storage for ship {ship.sname}...")
            self.load_ship_storage(ship, ship_elem)
            
            self.ships.append(ship)
            self.logger.info(f"  Ship {ship.sname} loaded with {len(ship.storage_containers)} storage containers")
        
        self.logger.info(f"Total ships loaded: {len(self.ships)}")
            
    def load_ship_storage(self, ship: Ship, ship_elem: ET.Element):
        """Load storage containers and items for a ship

        Storage containers are <feat> elements with an eatAllowed attribute,
        containing an <inv> element with <s> items inside.
        
        Storage size detection:
        - fi="20" → Medical Cabinet (capacity ~50)
        - Parent <l> with ind="0" → Small Storage (capacity 50)
        - Parent <l> with ind="3" → Large Storage (capacity 250)
        - Other → Default to Large Storage (250)
        """
        self.logger.debug(f"    Searching for storage containers (feat with eatAllowed) in ship {ship.sname}")

        # Find all <feat> elements with eatAllowed attribute
        feat_elements = ship_elem.findall(".//feat[@eatAllowed]")
        self.logger.debug(f"    Found {len(feat_elements)} feat elements with eatAllowed")

        container_index = 0
        for feat_elem in feat_elements:
            # Check if this feat has an inv element
            inv_elem = feat_elem.find("inv")
            if inv_elem is None:
                continue

            # Create storage container
            container = StorageContainer()
            container.container_id = container_index

            # Detect storage type from fi attribute and parent ind
            fi_attr = feat_elem.get("fi", "")
            eat_allowed = feat_elem.get("eatAllowed", "0")
            
            # Find parent element to check ind attribute
            parent_elem = None
            parent_ind = None
            for elem in ship_elem.iter():
                if feat_elem in list(elem):
                    parent_elem = elem
                    parent_ind = elem.get("ind", "")
                    break
            
            # Determine storage type and capacity
            if fi_attr == "20":
                # Medical cabinet
                storage_type = "Medical Cabinet"
                container.capacity = 50
            elif parent_ind == "0":
                # Small storage
                storage_type = "Small Storage"
                container.capacity = 50
            elif parent_ind == "3":
                # Large storage
                storage_type = "Large Storage"
                container.capacity = 250
            else:
                # Default to large storage
                storage_type = "Storage"
                container.capacity = 250
            
            # Generate descriptive name
            container.container_name = f"{storage_type} {container_index + 1}"

            self.logger.debug(f"    Processing {container.container_name} (fi={fi_attr}, ind={parent_ind}, capacity={container.capacity})")

            # Load items from <inv> -> <s> elements
            item_count = 0
            for item_elem in inv_elem.findall("s"):
                try:
                    item_id = int(item_elem.get("elementaryId", "0"))
                    quantity = int(item_elem.get("inStorage", "0"))

                    if item_id > 0 and quantity > 0:
                        item = StorageItem()
                        item.item_id = str(item_id)
                        item.item_name = self.id_collection.get_storage_item_name(item_id)
                        item.quantity = quantity
                        container.items.append(item)
                        item_count += 1
                        self.logger.debug(f"      Item: {item.item_name} (ID {item_id}) x{quantity}")
                except (ValueError, AttributeError) as e:
                    self.logger.warning(f"      Failed to parse item: {e}")
                    continue

            if item_count > 0 or True:  # Always add container even if empty
                ship.storage_containers.append(container)
                self.logger.info(f"    Loaded {container.container_name} with {item_count} items (capacity: {container.capacity})")
                container_index += 1

        if container_index == 0:
            self.logger.warning(f"    No storage containers found for ship {ship.sname}")
        
    def load_characters(self):
        """Load characters from XML"""
        self.characters.clear()

        if self.xml_root is None:
            self.logger.warning("load_characters: xml_root is None")
            return

        self.logger.info("Loading characters from ships...")

        # Find all ships
        ships_elem = self.xml_root.find("ships")
        if ships_elem is None:
            self.logger.warning("No ships element found in XML")
            return

        ship_list = list(ships_elem.findall("ship"))
        self.logger.info(f"Found {len(ship_list)} ships to check for characters")

        total_characters = 0
        for ship_elem in ship_list:
            ship_sid = int(ship_elem.get("sid", "0"))
            ship_name = ship_elem.get("sname", "Unknown Ship")
            self.logger.info(f"Checking ship: {ship_name} (SID: {ship_sid})")

            # Find characters element in this ship
            characters_elem = ship_elem.find("characters")
            if characters_elem is None:
                self.logger.debug(f"  No characters element in ship {ship_name}")
                continue

            char_elements = characters_elem.findall("c")
            self.logger.info(f"  Found {len(char_elements)} characters in ship {ship_name}")

            for idx, char_elem in enumerate(char_elements):
                self.logger.debug(f"  Processing character {idx + 1}/{len(char_elements)}")

                character = Character()

                # Get basic attributes from <c> element
                character.character_name = char_elem.get("name", "Unknown")
                character.character_last_name = char_elem.get("lname", "")
                character.character_entity_id = int(char_elem.get("entId", "0"))
                character.ship_sid = ship_sid  # From parent ship

                # Load homeship from <ai> element
                ai_elem = char_elem.find("ai")
                if ai_elem is not None:
                    character.homeship_sid = int(ai_elem.get("hsid", "0"))
                    self.logger.debug(f"    Homeship ID: {character.homeship_sid}")

                # Load appearance data from <c> attributes
                character.appearance = {
                    "bb": char_elem.get("bb", ""),
                    "bs": char_elem.get("bs", ""),
                    "bh": char_elem.get("bh", ""),
                    "bp": char_elem.get("bp", ""),
                    "orgColor": char_elem.get("orgColor", ""),
                    "colorSet": char_elem.get("colorSet", "")
                }

                # Load colors element if present
                colors_elem = char_elem.find("colors")
                if colors_elem is not None:
                    character.colors = dict(colors_elem.attrib)

                full_name = f"{character.character_name} {character.character_last_name}".strip()
                self.logger.info(f"    Loaded: {full_name} (Entity ID: {character.character_entity_id})")

                # Load personality data from <pers> element
                pers_elem = char_elem.find("pers")
                if pers_elem is not None:
                    self.logger.debug(f"    Found <pers> element")

                    # Load attributes from <pers>/<attr>
                    attr_elem = pers_elem.find("attr")
                    if attr_elem is not None:
                        attr_count = 0
                        for a_elem in attr_elem.findall("a"):
                            prop = DataProp()
                            prop.id = int(a_elem.get("id", "0"))
                            prop.value = int(a_elem.get("points", "0"))
                            # Get human-readable name
                            prop.name = self.id_collection.get_attribute_name(prop.id)
                            character.character_attributes.append(prop)
                            attr_count += 1
                            self.logger.debug(f"      Attribute: {prop.name} ({prop.id}) = {prop.value}")
                        self.logger.info(f"    Loaded {attr_count} attributes")

                    # Load skills from <pers>/<skills>
                    skills_elem = pers_elem.find("skills")
                    if skills_elem is not None:
                        skill_count = 0
                        for s_elem in skills_elem.findall("s"):
                            prop = DataProp()
                            # Note: skills use 'sk' attribute, not 'id'!
                            prop.id = int(s_elem.get("sk", "0"))
                            prop.value = int(s_elem.get("level", "0"))
                            prop.max_value = int(s_elem.get("mxn", "0"))
                            # Get human-readable name
                            prop.name = self.id_collection.get_skill_name(prop.id)
                            character.character_skills.append(prop)
                            skill_count += 1
                            self.logger.debug(f"      Skill: {prop.name} ({prop.id}) = {prop.value}/{prop.max_value}")
                        self.logger.info(f"    Loaded {skill_count} skills")

                    # Load traits from <pers>/<traits>
                    traits_elem = pers_elem.find("traits")
                    if traits_elem is not None:
                        trait_count = 0
                        for t_elem in traits_elem.findall("t"):
                            prop = DataProp()
                            prop.id = int(t_elem.get("id", "0"))
                            # Get human-readable name
                            prop.name = self.id_collection.get_trait_name(prop.id)
                            character.character_traits.append(prop)
                            trait_count += 1
                            self.logger.debug(f"      Trait: {prop.name} ({prop.id})")
                        self.logger.info(f"    Loaded {trait_count} traits")

                    # Load conditions from <pers>/<conditions>
                    conditions_elem = pers_elem.find("conditions")
                    if conditions_elem is not None:
                        condition_count = 0
                        for cond_elem in conditions_elem.findall("c"):
                            prop = DataProp()
                            prop.id = int(cond_elem.get("id", "0"))
                            # Get human-readable name
                            prop.name = self.id_collection.get_condition_name(prop.id)
                            character.character_conditions.append(prop)
                            condition_count += 1
                            self.logger.debug(f"      Condition: {prop.name} ({prop.id})")
                        self.logger.info(f"    Loaded {condition_count} conditions")

                    # Load relationships from <pers>/<sociality>/<relationships>
                    sociality_elem = pers_elem.find("sociality")
                    if sociality_elem is not None:
                        relationships_elem = sociality_elem.find("relationships")
                        if relationships_elem is not None:
                            rel_count = 0
                            for rel_elem in relationships_elem.findall("l"):
                                rel = RelationshipInfo()
                                rel.target_entity_id = int(rel_elem.get("targetId", "0"))
                                rel.friendship = int(rel_elem.get("friendship", "0"))
                                rel.attraction = int(rel_elem.get("attraction", "0"))
                                rel.compatibility = int(rel_elem.get("compatibility", "0"))
                                rel.best_friends = rel_elem.get("bestFriends", "false").lower() == "true"
                                rel.lovers = rel_elem.get("lovers", "false").lower() == "true"
                                # We'll resolve target_name later when all characters are loaded
                                character.character_relationships.append(rel)
                                rel_count += 1
                                self.logger.debug(f"      Relationship to entId {rel.target_entity_id}: {rel.get_relationship_type()}")
                            self.logger.info(f"    Loaded {rel_count} relationships")

                    # Load job priorities from <pers>/<jobsetting>
                    jobsetting_elem = pers_elem.find("jobsetting")
                    if jobsetting_elem is not None:
                        for job_elem in jobsetting_elem.findall("j"):
                            profession = job_elem.get("profession", "")
                            priority = job_elem.get("priority", "Normal")
                            character.job_priorities[profession] = priority
                        self.logger.info(f"    Loaded {len(character.job_priorities)} job priorities")
                else:
                    self.logger.warning(f"    No <pers> element found for {character.character_name}")

                # Load loadout/equipment from <loadout> element
                loadout_elem = char_elem.find("loadout")
                if loadout_elem is not None:
                    character.loadout = {
                        "headgear": int(loadout_elem.get("headgear", "0")),
                        "armor": int(loadout_elem.get("armor", "0")),
                        "primary": int(loadout_elem.get("primary", "0")),
                        "attachment": int(loadout_elem.get("attachment", "0")),
                        "secondary": int(loadout_elem.get("secondary", "0")),
                        "pocket1": int(loadout_elem.get("pocket1", "0")),
                        "pocket2": int(loadout_elem.get("pocket2", "0")),
                        "pocket3": int(loadout_elem.get("pocket3", "0")),
                    }
                    equipped_count = sum(1 for v in character.loadout.values() if v > 0)
                    self.logger.info(f"    Loaded loadout: {equipped_count} items equipped")

                # Load augmentations from <aug> element
                aug_elem = char_elem.find("aug")
                if aug_elem is not None:
                    character.augmentations = {
                        "primary": int(aug_elem.get("primary", "0")),
                        "secondary": int(aug_elem.get("secondary", "0")),
                    }
                    if character.augmentations["primary"] > 0 or character.augmentations["secondary"] > 0:
                        self.logger.info(f"    Loaded augmentations")

                self.characters.append(character)
                total_characters += 1

        self.logger.info(f"Total characters loaded: {total_characters}")

        # Resolve relationship target names now that all characters are loaded
        self._resolve_relationship_names()

    def _resolve_relationship_names(self):
        """Resolve target entity IDs to character names for all relationships"""
        # Create a lookup dict of entity_id -> character_name
        entity_name_map = {}
        for char in self.characters:
            entity_name_map[char.character_entity_id] = char.character_name

        # Update all relationships with resolved names
        for char in self.characters:
            for rel in char.character_relationships:
                if rel.target_entity_id in entity_name_map:
                    rel.target_name = entity_name_map[rel.target_entity_id]
                else:
                    rel.target_name = f"Unknown ({rel.target_entity_id})"
                    self.logger.warning(f"Could not resolve relationship target: {rel.target_entity_id}")
            
    def update_global_settings(self):
        """Update global settings in memory"""
        if self.xml_root is None:
            QMessageBox.warning(self, "Error", "No save file loaded")
            return
            
        settings_updated = False
        
        try:
            # Update credits
            bank_elem = self.xml_root.find("playerBank")
            if bank_elem is not None:
                new_credits = self.credits_input.text()
                if new_credits != bank_elem.get("ca", ""):
                    bank_elem.set("ca", new_credits)
                    settings_updated = True
            
            # Update prestige points
            try:
                new_prestige = int(self.prestige_input.text())
                quest_lines1 = self.xml_root.find("questLines")
                if quest_lines1 is not None:
                    quest_lines2 = quest_lines1.find("questLines")
                    if quest_lines2 is not None:
                        for elem in quest_lines2.findall("l"):
                            if elem.get("type") == "ExodusFleet":
                                if elem.get("playerPrestigePoints") != str(new_prestige):
                                    elem.set("playerPrestigePoints", str(new_prestige))
                                    settings_updated = True
                                break
            except ValueError:
                QMessageBox.warning(self, "Error", "Invalid prestige points value")
                return
            
            # Update sandbox mode
            settings_elem = self.xml_root.find("settings")
            if settings_elem is not None:
                diff_elem = settings_elem.find("diff")
                if diff_elem is not None:
                    new_sandbox = "true" if self.sandbox_check.isChecked() else "false"
                    if diff_elem.get("sandbox") != new_sandbox:
                        diff_elem.set("sandbox", new_sandbox)
                        settings_updated = True
            
            if settings_updated:
                QMessageBox.information(
                    self,
                    "Success",
                    "Global settings updated in memory.\nUse File -> Save to make changes permanent."
                )
            else:
                QMessageBox.information(self, "Info", "No changes detected in global settings")
                
        except Exception as e:
            QMessageBox.critical(self, "Error", f"Failed to update settings:\n{str(e)}")
            
    def save_file(self):
        """Save changes to file (preserving XML formatting)"""
        if not self.current_file_path or self.xml_tree is None:
            QMessageBox.warning(self, "Error", "No file loaded to save")
            return

        try:
            # Update XML with any character changes (in-place)
            self.update_characters_to_xml()

            # Update XML with any storage changes
            self.update_storage_to_xml()

            # Write the XML tree to file
            # Note: We update values in-place, so tree.write() should preserve structure
            # Use method='xml' to avoid reformatting, but ElementTree may still change whitespace
            self.logger.info("Writing modified XML to file...")

            # Convert to string to preserve formatting better
            from xml.etree.ElementTree import tostring
            xml_bytes = tostring(self.xml_root, encoding='utf-8')

            # Write directly to avoid ET.write() reformatting
            with open(self.current_file_path, 'wb') as f:
                # Write XML declaration
                f.write(b'<?xml version="1.0" encoding="utf-8"?>\n')
                # Write the XML content
                f.write(xml_bytes)

            self.logger.info("XML written successfully")

            # Update info file's realTimeDate (timestamp of last modification)
            if self.current_folder_path:
                info_file = self.current_folder_path / "save" / "info"
                if info_file.exists():
                    try:
                        import time
                        tree = ET.parse(info_file)
                        root = tree.getroot()
                        # Update real time date to current timestamp (milliseconds)
                        current_time_ms = int(time.time() * 1000)
                        root.set("realTimeDate", str(current_time_ms))

                        # Write info file the same way
                        info_bytes = tostring(root, encoding='utf-8')
                        with open(info_file, 'wb') as f:
                            f.write(b'<?xml version="1.0" encoding="utf-8"?>\n')
                            f.write(info_bytes)

                        self.logger.info(f"Updated info file timestamp: {current_time_ms}")
                    except Exception as e:
                        self.logger.warning(f"Failed to update info file: {e}")

            # Remove [Modified] from title
            title = self.windowTitle().replace(" [Modified]", "")
            self.setWindowTitle(title)

            self.logger.info(f"File saved successfully: {self.current_file_path}")
            QMessageBox.information(self, "Success", "File saved successfully!")
        except Exception as e:
            self.logger.error(f"Failed to save file: {e}", exc_info=True)
            QMessageBox.critical(self, "Error", f"Failed to save file:\n{str(e)}")
            
    def on_ship_selected(self, index: int):
        """Handle ship selection change"""
        self.logger.info(f"Ship selection changed to index {index}")
        
        if index < 0:
            self.logger.debug("Invalid index, returning")
            return
            
        ship = self.ship_combo.itemData(index)
        if ship:
            self.logger.info(f"Selected ship: {ship.sname} (ID: {ship.sid})")
            self.ship_width.setValue(ship.sx)
            self.ship_height.setValue(ship.sy)
            self.ship_info.setText(f"Ship: {ship.sname}\nDimensions: {ship.sx}x{ship.sy}")
            
            # Update crew list for this ship
            self.logger.info(f"Updating crew list for ship {ship.sid}")
            self.update_crew_list(ship.sid)
            
            # Update storage containers
            self.logger.info(f"Updating storage containers for ship {ship.sname}")
            self.update_storage_containers(ship)
        else:
            self.logger.warning(f"No ship data for index {index}")
            
    def update_ship_size(self):
        """Update the selected ship's size"""
        current_ship = self.ship_combo.currentData()
        if not current_ship or self.xml_root is None:
            return
            
        new_width = self.ship_width.value()
        new_height = self.ship_height.value()
        
        # Find and update ship in XML
        for ship_elem in self.xml_root.findall(".//ship"):
            if int(ship_elem.get("sid", "0")) == current_ship.sid:
                ship_elem.set("sx", str(new_width))
                ship_elem.set("sy", str(new_height))
                current_ship.sx = new_width
                current_ship.sy = new_height
                QMessageBox.information(
                    self,
                    "Success",
                    f"Ship size updated to {new_width}x{new_height}\nRemember to save!"
                )
                break
                
    def update_crew_list(self, ship_sid: int):
        """Update the crew list for the selected ship"""
        self.logger.info(f"Updating crew list for ship SID {ship_sid}")
        self.crew_list.clear()

        crew_count = 0
        for char in self.characters:
            if char.ship_sid == ship_sid:
                from PyQt6.QtWidgets import QListWidgetItem
                # Show full name if last name exists
                display_name = f"{char.character_name} {char.character_last_name}".strip()
                item = QListWidgetItem(display_name)
                item.setData(256, char)  # Store character object as item data (Qt.UserRole = 256)
                self.crew_list.addItem(item)
                crew_count += 1
                self.logger.debug(f"  Added crew: {char.character_name}")

        self.logger.info(f"Added {crew_count} crew members to list")

        if crew_count == 0:
            self.logger.warning(f"No crew found for ship SID {ship_sid}")
        elif crew_count > 0:
            # Select the first crew member
            self.crew_list.setCurrentRow(0)
                
    def on_crew_selected(self, row: int):
        """Handle crew member selection"""
        self.logger.info(f"Crew selection changed to row {row}")

        if row < 0:
            self.logger.debug("Invalid row, clearing crew details")
            # Clear crew details display
            self.crew_name_edit.clear()
            self.crew_name_edit.setPlaceholderText("Select a crew member")
            self.crew_name_edit.setReadOnly(True)
            self.clear_editor_layout(self.attributes_layout)
            self.clear_editor_layout(self.skills_layout)
            self.clear_editor_layout(self.traits_layout)
            self.clear_editor_layout(self.conditions_layout)
            self.current_character = None
            return

        item = self.crew_list.item(row)
        if item:
            character = item.data(256)  # Qt.UserRole = 256
            if character:
                self.logger.info(f"Selected crew: {character.character_name}")
                self.display_crew_details(character)
            else:
                self.logger.warning(f"No character data for row {row}")
        else:
            self.logger.warning(f"No item at row {row}")
            
    def display_crew_details(self, character: Character):
        """Display details for a crew member with interactive editors"""
        from crew_editors import NumericValueEditor, SkillEditor, TraitWidget, ConditionWidget

        self.logger.info(f"Displaying editable details for {character.character_name}")

        # Store current character
        self.current_character = character

        # Update header - temporarily disconnect signal to avoid triggering change
        self.crew_name_edit.textChanged.disconnect(self.on_crew_name_changed)
        self.crew_name_edit.setText(character.character_name)
        self.crew_name_edit.setReadOnly(False)  # Enable editing
        self.crew_name_edit.textChanged.connect(self.on_crew_name_changed)

        # Update homeship selector - populate with player ships only
        self.homeship_combo.blockSignals(True)
        self.homeship_combo.clear()
        for ship in self.ships:
            # Only add player ships (not enemy/derelict ships)
            # Player ships typically have side="Player" or are "real" ships
            self.homeship_combo.addItem(ship.sname, ship.sid)

        # Set current homeship
        index = self.homeship_combo.findData(character.homeship_sid)
        if index >= 0:
            self.homeship_combo.setCurrentIndex(index)
        else:
            self.logger.warning(f"Homeship {character.homeship_sid} not found in ship list")
        self.homeship_combo.blockSignals(False)

        # Clear existing editors
        self.clear_editor_layout(self.attributes_layout)
        self.clear_editor_layout(self.skills_layout)
        self.clear_editor_layout(self.traits_layout)
        self.clear_editor_layout(self.conditions_layout)
        self.clear_editor_layout(self.relationships_layout)
        self.clear_editor_layout(self.job_priorities_layout)
        self.clear_editor_layout(self.appearance_layout)
        self.clear_editor_layout(self.equipment_layout)

        self.attribute_editors.clear()
        self.skill_editors.clear()

        # Add attribute editors
        self.logger.debug(f"  Creating {len(character.character_attributes)} attribute editors")
        for attr in character.character_attributes:
            # Attributes range from 1-5 in Space Haven
            editor = NumericValueEditor(
                item_id=attr.id,
                name=attr.name,
                current_value=attr.value,
                min_value=1,
                max_value=5
            )
            editor.valueChanged.connect(self.on_attribute_changed)
            self.attributes_layout.addWidget(editor)
            self.attribute_editors[attr.id] = editor

        # Add skill editors with visual bars
        self.logger.debug(f"  Creating {len(character.character_skills)} skill editors")
        for skill in character.character_skills:
            editor = SkillEditor(
                skill_id=skill.id,
                name=skill.name,
                current_level=skill.value,
                max_learning=skill.max_value if skill.max_value > 0 else 10
            )
            editor.valueChanged.connect(self.on_skill_changed)
            self.skills_layout.addWidget(editor)
            self.skill_editors[skill.id] = editor

        # Add trait widgets
        self.logger.debug(f"  Creating {len(character.character_traits)} trait widgets")
        for trait in character.character_traits:
            widget = TraitWidget(trait.id, trait.name)
            widget.traitRemoved.connect(self.on_trait_removed)
            self.traits_layout.addWidget(widget)

        # Add condition widgets
        self.logger.debug(f"  Creating {len(character.character_conditions)} condition widgets")
        for condition in character.character_conditions:
            widget = ConditionWidget(condition.id, condition.name)
            widget.conditionRemoved.connect(self.on_condition_removed)
            self.conditions_layout.addWidget(widget)

        # Add relationship widgets
        self.logger.debug(f"  Creating {len(character.character_relationships)} relationship widgets")
        for rel in character.character_relationships:
            from PyQt6.QtWidgets import QLabel, QHBoxLayout
            rel_widget = QWidget()
            rel_layout = QHBoxLayout(rel_widget)
            rel_layout.setContentsMargins(0, 2, 0, 2)

            # Relationship type with color coding
            rel_type = rel.get_relationship_type()
            if rel_type == "Lovers":
                color = "#ff1493"  # Deep pink
            elif rel_type == "Best Friends":
                color = "#00cc00"  # Green
            elif rel_type == "Friends":
                color = "#66cc66"  # Light green
            elif rel_type == "Dislike":
                color = "#ff6600"  # Orange
            elif rel_type == "Enemies":
                color = "#cc0000"  # Red
            else:
                color = "#888888"  # Gray

            rel_label = QLabel(f"<b style='color: {color};'>{rel_type}</b>: {rel.target_name}")
            rel_layout.addWidget(rel_label)

            # Friendship score
            score_label = QLabel(f"({rel.friendship})")
            score_label.setStyleSheet("color: gray; font-size: 10px;")
            rel_layout.addWidget(score_label)

            rel_layout.addStretch()
            self.relationships_layout.addWidget(rel_widget)

        # Add job priorities display (highest priority ones first)
        if character.job_priorities:
            self.logger.debug(f"  Displaying {len(character.job_priorities)} job priorities")
            # Sort by priority level (Highest first)
            priority_order = {"Highest": 0, "High": 1, "Normal": 2, "Low": 3, "Lowest": 4, "DontDo": 5}
            sorted_jobs = sorted(character.job_priorities.items(),
                               key=lambda x: priority_order.get(x[1], 99))

            for profession, priority in sorted_jobs:
                from PyQt6.QtWidgets import QLabel, QHBoxLayout
                job_widget = QWidget()
                job_layout = QHBoxLayout(job_widget)
                job_layout.setContentsMargins(0, 2, 0, 2)

                # Color code by priority
                if priority == "Highest":
                    color = "#00cc00"  # Green
                elif priority == "High":
                    color = "#66cc66"  # Light green
                elif priority == "Normal":
                    color = "#888888"  # Gray
                elif priority == "Low":
                    color = "#ff9900"  # Orange
                elif priority == "Lowest":
                    color = "#ff6600"  # Dark orange
                elif priority == "DontDo":
                    color = "#cc0000"  # Red
                else:
                    color = "#888888"

                job_label = QLabel(f"<b style='color: {color};'>{priority}</b>: {profession}")
                job_layout.addWidget(job_label)
                job_layout.addStretch()
                self.job_priorities_layout.addWidget(job_widget)

        # Populate appearance editor controls
        if character.colors:
            # Set skin tone
            if character.colors.get("skinSet"):
                index = self.skin_combo.findData(str(character.colors['skinSet']))
                if index >= 0:
                    self.skin_combo.setCurrentIndex(index)

            # Set sleeve length
            if character.colors.get("longSleeve"):
                index = self.sleeve_combo.findData(character.colors['longSleeve'])
                if index >= 0:
                    self.sleeve_combo.setCurrentIndex(index)

            # Set gloves
            if character.colors.get("glovesOff"):
                index = self.gloves_combo.findData(character.colors['glovesOff'])
                if index >= 0:
                    self.gloves_combo.setCurrentIndex(index)

            # Set shirt color
            if character.colors.get("shirtSet"):
                self.shirt_color_input.setText(str(character.colors['shirtSet']))

            # Set pants color
            if character.colors.get("pantsSet"):
                self.pants_color_input.setText(str(character.colors['pantsSet']))

        # Add appearance display
        if character.appearance or character.colors:
            from PyQt6.QtWidgets import QLabel
            self.logger.debug("  Displaying appearance data")

            # Helper function to translate skin tone
            def get_skin_tone(skin_set):
                skin_map = {
                    "743": "Dark skin",
                    "744": "Light skin",
                    "745": "Medium skin",
                }
                return skin_map.get(str(skin_set), f"Skin #{skin_set}")

            # Helper function to translate body type
            def get_body_type(bb):
                return "Male" if bb == "m" else "Female" if bb == "f" else bb

            # Show basic appearance attributes
            if character.appearance:
                appearance_items = []

                # Body type (Male/Female)
                if character.appearance.get("bb"):
                    body_type = get_body_type(character.appearance['bb'])
                    appearance_items.append(f"<b>Body:</b> {body_type}")

                # Body style
                if character.appearance.get("bs"):
                    appearance_items.append(f"<b>Style:</b> {character.appearance['bs']}")

                # Head style
                if character.appearance.get("bh"):
                    appearance_items.append(f"<b>Head:</b> {character.appearance['bh']}")

                # Pants style
                if character.appearance.get("bp"):
                    appearance_items.append(f"<b>Pants Style:</b> {character.appearance['bp']}")

                if appearance_items:
                    appearance_text = " | ".join(appearance_items)
                    appearance_label = QLabel(appearance_text)
                    appearance_label.setStyleSheet("color: #888888; font-size: 10px;")
                    self.appearance_layout.addWidget(appearance_label)

            # Show color and clothing details if present
            if character.colors:
                color_items = []

                # Skin tone
                if character.colors.get("skinSet"):
                    skin_tone = get_skin_tone(character.colors['skinSet'])
                    color_items.append(f"<b>{skin_tone}</b>")

                # Sleeve length
                if character.colors.get("longSleeve"):
                    sleeve_type = "Long sleeves" if character.colors['longSleeve'] == "true" else "Short sleeves"
                    color_items.append(sleeve_type)

                # Gloves (note: glovesOff="true" means NO gloves!)
                if character.colors.get("glovesOff"):
                    has_gloves = "No gloves" if character.colors['glovesOff'] == "true" else "With gloves"
                    color_items.append(has_gloves)

                if color_items:
                    clothing_text = " | ".join(color_items)
                    clothing_label = QLabel(clothing_text)
                    clothing_label.setStyleSheet("color: #888888; font-size: 10px;")
                    self.appearance_layout.addWidget(clothing_label)

                # Show shirt and pants color IDs on a separate line
                color_ids = []
                if character.colors.get("shirtSet"):
                    color_ids.append(f"<b>Shirt color:</b> #{character.colors['shirtSet']}")
                if character.colors.get("pantsSet"):
                    color_ids.append(f"<b>Pants color:</b> #{character.colors['pantsSet']}")

                if color_ids:
                    color_ids_text = " | ".join(color_ids)
                    color_ids_label = QLabel(color_ids_text)
                    color_ids_label.setStyleSheet("color: #666666; font-size: 9px; font-style: italic;")
                    self.appearance_layout.addWidget(color_ids_label)

        # Add equipment/loadout editor
        from PyQt6.QtWidgets import QLabel, QGridLayout
        self.logger.debug("  Creating equipment/loadout editor")

        # Initialize loadout if not present
        if not character.loadout:
            character.loadout = {
                "headgear": 0, "armor": 0, "primary": 0, "attachment": 0,
                "secondary": 0, "pocket1": 0, "pocket2": 0, "pocket3": 0
            }

        # Create equipment editor grid
        equip_widget = QWidget()
        equip_grid = QGridLayout(equip_widget)
        equip_grid.setSpacing(5)
        equip_grid.setContentsMargins(0, 5, 0, 5)

        # Store equipment combos for later access
        self.equipment_combos = {}

        row = 0
        # Weapons section
        equip_grid.addWidget(QLabel("<b>Primary Weapon:</b>"), row, 0)
        primary_combo = self.create_equipment_combo("weapons", character.loadout.get("primary", 0))
        primary_combo.currentIndexChanged.connect(lambda: self.on_equipment_changed("primary"))
        self.equipment_combos["primary"] = primary_combo
        equip_grid.addWidget(primary_combo, row, 1)
        row += 1

        equip_grid.addWidget(QLabel("<b>Attachment:</b>"), row, 0)
        attachment_combo = self.create_equipment_combo("attachments", character.loadout.get("attachment", 0))
        attachment_combo.currentIndexChanged.connect(lambda: self.on_equipment_changed("attachment"))
        self.equipment_combos["attachment"] = attachment_combo
        equip_grid.addWidget(attachment_combo, row, 1)
        row += 1

        equip_grid.addWidget(QLabel("<b>Secondary Weapon:</b>"), row, 0)
        secondary_combo = self.create_equipment_combo("weapons", character.loadout.get("secondary", 0))
        secondary_combo.currentIndexChanged.connect(lambda: self.on_equipment_changed("secondary"))
        self.equipment_combos["secondary"] = secondary_combo
        equip_grid.addWidget(secondary_combo, row, 1)
        row += 1

        # Armor section
        equip_grid.addWidget(QLabel("<b>Headgear:</b>"), row, 0)
        headgear_combo = self.create_equipment_combo("armor", character.loadout.get("headgear", 0))
        headgear_combo.currentIndexChanged.connect(lambda: self.on_equipment_changed("headgear"))
        self.equipment_combos["headgear"] = headgear_combo
        equip_grid.addWidget(headgear_combo, row, 1)
        row += 1

        equip_grid.addWidget(QLabel("<b>Armor:</b>"), row, 0)
        armor_combo = self.create_equipment_combo("armor", character.loadout.get("armor", 0))
        armor_combo.currentIndexChanged.connect(lambda: self.on_equipment_changed("armor"))
        self.equipment_combos["armor"] = armor_combo
        equip_grid.addWidget(armor_combo, row, 1)
        row += 1

        # Pockets section
        for i in range(1, 4):
            equip_grid.addWidget(QLabel(f"<b>Pocket {i}:</b>"), row, 0)
            pocket_combo = self.create_equipment_combo("utility", character.loadout.get(f"pocket{i}", 0))
            pocket_combo.currentIndexChanged.connect(lambda checked, idx=i: self.on_equipment_changed(f"pocket{idx}"))
            self.equipment_combos[f"pocket{i}"] = pocket_combo
            equip_grid.addWidget(pocket_combo, row, 1)
            row += 1

        self.equipment_layout.addWidget(equip_widget)

        # Show augmentations if present
        if character.augmentations and (character.augmentations.get("primary", 0) > 0 or character.augmentations.get("secondary", 0) > 0):
            aug_items = []
            if character.augmentations.get("primary", 0) > 0:
                aug_items.append(f"Primary Aug #{character.augmentations['primary']}")
            if character.augmentations.get("secondary", 0) > 0:
                aug_items.append(f"Secondary Aug #{character.augmentations['secondary']}")

            aug_label = QLabel(f"<b>Augmentations:</b> {', '.join(aug_items)}")
            aug_label.setStyleSheet("color: #9966ff; margin-top: 5px;")
            self.equipment_layout.addWidget(aug_label)

        self.logger.info(f"Crew details displayed with interactive editors")

    def clear_editor_layout(self, layout: QVBoxLayout):
        """Clear all widgets from a layout"""
        while layout.count():
            child = layout.takeAt(0)
            if child.widget():
                child.widget().deleteLater()

    def on_crew_name_changed(self, new_name: str):
        """Handle crew name change"""
        if self.current_character and new_name:
            self.current_character.character_name = new_name
            self.logger.info(f"Character name changed to: {new_name}")
            # Update the crew list item text
            for row in range(self.crew_list.count()):
                item = self.crew_list.item(row)
                if item.data(256) == self.current_character:
                    item.setText(new_name)
                    break
            # Mark as modified
            self.mark_as_modified()

    def on_attribute_changed(self, attr_id: int, new_value: int):
        """Handle attribute value change"""
        if self.current_character:
            for attr in self.current_character.character_attributes:
                if attr.id == attr_id:
                    attr.value = new_value
                    self.logger.info(f"Attribute {attr.name} changed to {new_value}")
                    # Mark as modified
                    self.mark_as_modified()
                    break

    def on_skill_changed(self, skill_id: int, current_level: int, max_learning: int):
        """Handle skill value change - updates both current level and max learning"""
        if self.current_character:
            for skill in self.current_character.character_skills:
                if skill.id == skill_id:
                    skill.value = current_level
                    skill.max_value = max_learning
                    self.logger.info(f"Skill {skill.name} changed to level={current_level}, learning={max_learning}")
                    # Mark as modified
                    self.mark_as_modified()
                    break

    def on_trait_removed(self, trait_id: int):
        """Handle trait removal"""
        if self.current_character:
            # Find and remove the trait
            for i, trait in enumerate(self.current_character.character_traits):
                if trait.id == trait_id:
                    removed_trait = self.current_character.character_traits.pop(i)
                    self.logger.info(f"Removed trait: {removed_trait.name}")
                    # Refresh display
                    self.display_crew_details(self.current_character)
                    self.mark_as_modified()
                    break

    def populate_trait_combo(self):
        """Populate the trait dropdown with all available traits"""
        self.trait_combo.clear()
        
        # Get all traits from IdCollection, organized by type
        positive_traits = [
            (191, "Hero"),
            (1035, "Smart"),
            (1039, "Fast Learner"),
            (1041, "Hard Working"),
            (1044, "Iron-willed"),
            (1045, "Spacefarer"),
            (1046, "Confident"),
            (1048, "Charming"),
            (1533, "Iron Stomach"),
            (1534, "Nyctophilia"),
            (1535, "Minimalist"),
            (1560, "Talkative"),
            (1562, "Gourmand"),
            (2082, "Alien lover"),
        ]
        
        negative_traits = [
            (655, "Wimp"),
            (656, "Clumsy"),
            (1034, "Suicidal"),
            (1037, "Antisocial"),
            (1038, "Needy"),
            (1040, "Lazy"),
            (1047, "Neurotic"),
        ]
        
        neutral_traits = [
            (1036, "Bloodlust"),
            (1042, "Psychopath"),
            (1043, "Peace-loving"),
        ]
        
        # Add positive traits
        self.trait_combo.addItem("--- POSITIVE TRAITS ---")
        model = self.trait_combo.model()
        item = model.item(self.trait_combo.count() - 1)
        item.setEnabled(False)
        item.setBackground(QColor("#e8f5e9"))
        
        for trait_id, trait_name in positive_traits:
            self.trait_combo.addItem(f"  {trait_name}", trait_id)
        
        # Add negative traits
        self.trait_combo.addItem("--- NEGATIVE TRAITS ---")
        item = model.item(self.trait_combo.count() - 1)
        item.setEnabled(False)
        item.setBackground(QColor("#ffebee"))
        
        for trait_id, trait_name in negative_traits:
            self.trait_combo.addItem(f"  {trait_name}", trait_id)
        
        # Add neutral traits
        self.trait_combo.addItem("--- NEUTRAL TRAITS ---")
        item = model.item(self.trait_combo.count() - 1)
        item.setEnabled(False)
        item.setBackground(QColor("#f0f0f0"))
        
        for trait_id, trait_name in neutral_traits:
            self.trait_combo.addItem(f"  {trait_name}", trait_id)
        
        self.logger.debug("Populated trait combo with all available traits")

    def on_add_trait(self):
        """Handle adding a new trait to the current character"""
        if not self.current_character:
            return
        
        trait_id = self.trait_combo.currentData()
        if not trait_id:
            return
        
        # Check if trait already exists
        for trait in self.current_character.character_traits:
            if trait.id == trait_id:
                QMessageBox.information(
                    self,
                    "Trait Already Exists",
                    f"This character already has the trait: {self.trait_combo.currentText().strip()}"
                )
                return
        
        # Add the trait
        from models import DataProp
        new_trait = DataProp()
        new_trait.id = trait_id
        new_trait.name = self.id_collection.get_trait_name(trait_id)
        self.current_character.character_traits.append(new_trait)
        
        self.logger.info(f"Added trait: {new_trait.name} to {self.current_character.character_name}")
        
        # Refresh display
        self.display_crew_details(self.current_character)
        self.mark_as_modified()

    def create_equipment_combo(self, category: str, current_value: int) -> QComboBox:
        """Create a combo box for equipment selection"""
        combo = QComboBox()
        
        # Add "Empty" option
        combo.addItem("(Empty)", 0)
        
        # Define equipment by category
        equipment_items = {
            "weapons": [
                (760, "Five-Seven Pistol"),
                (728, "SMG"),
                (729, "Shotgun"),
                (725, "Assault Rifle"),
                (3070, "Laser Pistol"),
                (3069, "Laser Rifle"),
                (3071, "Plasma Cuttergun"),
                (3072, "Plasma Rifle"),
                (3962, "Stun Pistol"),
                (3961, "Stun Rifle"),
            ],
            "attachments": [
                (3968, "Basic Scope"),
                (3969, "Tactical Grip"),
                (3960, "Flamethrower"),
                (3967, "Explosive Grenade Launcher"),
                (4076, "Incendiary Grenade Launcher"),
            ],
            "armor": [
                (3384, "Armored Vest"),
                (4065, "Space Suit Oxygen Extender"),
            ],
            "utility": [
                (3386, "Remote Control"),
                (3388, "Oxygen Tank"),
                (4030, "Nano Wound Dressing"),
                (4007, "Bandage"),
                (4005, "Painkillers"),
                (4006, "Combat Stimulant"),
                (4040, "Small Breach Charge"),
                (2715, "Explosive Ammunition"),
            ],
        }
        
        # Add items for this category
        if category in equipment_items:
            for item_id, item_name in equipment_items[category]:
                combo.addItem(item_name, item_id)
        
        # Set current value
        index = combo.findData(current_value)
        if index >= 0:
            combo.setCurrentIndex(index)
        
        return combo

    def on_equipment_changed(self, slot: str):
        """Handle equipment changes"""
        if not self.current_character or slot not in self.equipment_combos:
            return
        
        combo = self.equipment_combos[slot]
        new_value = combo.currentData()
        
        if new_value is not None:
            self.current_character.loadout[slot] = new_value
            self.logger.info(f"Changed {slot} to item ID {new_value}")
            self.mark_as_modified()

    def on_appearance_changed(self):
        """Handle appearance changes (skin, sleeves, gloves)"""
        if not self.current_character:
            return
        
        # Initialize colors dict if not present
        if not self.current_character.colors:
            self.current_character.colors = {}
        
        # Update skin tone
        skin_value = self.skin_combo.currentData()
        if skin_value:
            self.current_character.colors["skinSet"] = skin_value
            self.logger.info(f"Changed skin tone to {self.skin_combo.currentText()}")
        
        # Update sleeve length
        sleeve_value = self.sleeve_combo.currentData()
        if sleeve_value:
            self.current_character.colors["longSleeve"] = sleeve_value
            self.logger.info(f"Changed sleeves to {self.sleeve_combo.currentText()}")
        
        # Update gloves
        gloves_value = self.gloves_combo.currentData()
        if gloves_value:
            self.current_character.colors["glovesOff"] = gloves_value
            self.logger.info(f"Changed gloves to {self.gloves_combo.currentText()}")
        
        self.mark_as_modified()

    def on_color_id_changed(self):
        """Handle shirt/pants color ID changes"""
        if not self.current_character:
            return
        
        # Initialize colors dict if not present
        if not self.current_character.colors:
            self.current_character.colors = {}
        
        # Update shirt color
        shirt_text = self.shirt_color_input.text().strip()
        if shirt_text:
            try:
                shirt_id = int(shirt_text)
                self.current_character.colors["shirtSet"] = str(shirt_id)
                self.logger.debug(f"Changed shirt color to {shirt_id}")
            except ValueError:
                pass  # Invalid input, ignore
        
        # Update pants color
        pants_text = self.pants_color_input.text().strip()
        if pants_text:
            try:
                pants_id = int(pants_text)
                self.current_character.colors["pantsSet"] = str(pants_id)
                self.logger.debug(f"Changed pants color to {pants_id}")
            except ValueError:
                pass  # Invalid input, ignore
        
        self.mark_as_modified()

    def on_homeship_changed(self):
        """Handle homeship assignment changes"""
        if not self.current_character:
            return
        
        new_homeship_id = self.homeship_combo.currentData()
        if new_homeship_id is not None:
            old_homeship_id = self.current_character.homeship_sid
            self.current_character.homeship_sid = new_homeship_id
            
            ship_name = self.homeship_combo.currentText()
            self.logger.info(f"Changed {self.current_character.character_name}'s homeship from {old_homeship_id} to {new_homeship_id} ({ship_name})")
            self.mark_as_modified()

    def on_condition_removed(self, condition_id: int):
        """Handle condition removal"""
        if self.current_character:
            # Find and remove the condition
            for i, condition in enumerate(self.current_character.character_conditions):
                if condition.id == condition_id:
                    removed_condition = self.current_character.character_conditions.pop(i)
                    self.logger.info(f"Removed condition: {removed_condition.name}")
                    # Refresh display
                    self.display_crew_details(self.current_character)
                    self.mark_as_modified()
                    break

    def update_characters_to_xml(self):
        """Update XML tree with current character data"""
        if self.xml_root is None:
            return

        self.logger.info("Updating XML with character changes...")

        ships_elem = self.xml_root.find("ships")
        if ships_elem is None:
            self.logger.warning("No ships element in XML")
            return

        updated_count = 0

        for ship_elem in ships_elem.findall("ship"):
            ship_sid = int(ship_elem.get("sid", "0"))

            characters_elem = ship_elem.find("characters")
            if characters_elem is None:
                continue

            for char_elem in characters_elem.findall("c"):
                char_entity_id = int(char_elem.get("entId", "0"))

                # Find matching character in our data
                character = None
                for char in self.characters:
                    if char.character_entity_id == char_entity_id and char.ship_sid == ship_sid:
                        character = char
                        break

                if character is None:
                    continue

                # Update character name IN-PLACE
                char_elem.set("name", character.character_name)
                self.logger.debug(f"  Updated name: {character.character_name}")

                # Update personality data
                pers_elem = char_elem.find("pers")
                if pers_elem is None:
                    self.logger.warning(f"No pers element for character {character.character_name}")
                    continue

                # Update attributes IN-PLACE (don't remove/recreate to preserve order and formatting)
                attr_elem = pers_elem.find("attr")
                if attr_elem is not None:
                    # Update existing attribute elements by finding by ID
                    for attr in character.character_attributes:
                        found = False
                        for a_elem in attr_elem.findall("a"):
                            if int(a_elem.get("id", "0")) == attr.id:
                                # Update points in-place
                                a_elem.set("points", str(attr.value))
                                self.logger.debug(f"  Updated attribute {attr.name}: points={attr.value}")
                                found = True
                                break

                        # Only add new attribute if it doesn't exist (shouldn't happen normally)
                        if not found:
                            self.logger.warning(f"Attribute {attr.id} not found in XML, skipping")

                # Update skills - both level and max learning
                skills_elem = pers_elem.find("skills")
                if skills_elem is not None:
                    # Update existing skill elements
                    for s_elem in skills_elem.findall("s"):
                        skill_id = int(s_elem.get("sk", "0"))

                        # Find matching skill in character
                        for skill in character.character_skills:
                            if skill.id == skill_id:
                                s_elem.set("level", str(skill.value))
                                s_elem.set("mxn", str(skill.max_value))
                                self.logger.debug(f"  Updated skill {skill.name}: level={skill.value}, mxn={skill.max_value}")
                                break

                # Update traits (remove deleted ones)
                traits_elem = pers_elem.find("traits")
                if traits_elem is not None:
                    # Get current trait IDs
                    current_trait_ids = {trait.id for trait in character.character_traits}

                    # Remove traits that are no longer in the character
                    for t_elem in list(traits_elem.findall("t")):
                        trait_id = int(t_elem.get("id", "0"))
                        if trait_id not in current_trait_ids:
                            traits_elem.remove(t_elem)

                # Update conditions (remove deleted ones)
                conditions_elem = pers_elem.find("conditions")
                if conditions_elem is not None:
                    # Get current condition IDs
                    current_condition_ids = {cond.id for cond in character.character_conditions}

                    # Remove conditions that are no longer in the character
                    for c_elem in list(conditions_elem.findall("c")):
                        cond_id = int(c_elem.get("id", "0"))
                        if cond_id not in current_condition_ids:
                            conditions_elem.remove(c_elem)

                # Update homeship assignment
                ai_elem = char_elem.find("ai")
                if ai_elem is not None and character.homeship_sid:
                    ai_elem.set("hsid", str(character.homeship_sid))
                    self.logger.debug(f"  Updated homeship to {character.homeship_sid}")

                # Update appearance/colors data
                if character.colors:
                    colors_elem = char_elem.find("colors")
                    if colors_elem is not None:
                        # Update color attributes
                        if "skinSet" in character.colors:
                            colors_elem.set("skinSet", str(character.colors["skinSet"]))
                        if "longSleeve" in character.colors:
                            colors_elem.set("longSleeve", character.colors["longSleeve"])
                        if "glovesOff" in character.colors:
                            colors_elem.set("glovesOff", character.colors["glovesOff"])
                        if "shirtSet" in character.colors:
                            colors_elem.set("shirtSet", str(character.colors["shirtSet"]))
                        if "pantsSet" in character.colors:
                            colors_elem.set("pantsSet", str(character.colors["pantsSet"]))
                        self.logger.debug(f"  Updated appearance colors")

                # Update loadout/equipment
                if character.loadout:
                    loadout_elem = char_elem.find("loadout")
                    if loadout_elem is not None:
                        # Update all equipment slots
                        for slot, value in character.loadout.items():
                            loadout_elem.set(slot, str(value))
                        self.logger.debug(f"  Updated equipment loadout")

                # Add new traits if any
                traits_elem = pers_elem.find("traits")
                if traits_elem is not None:
                    existing_trait_ids = {int(t.get("id", "0")) for t in traits_elem.findall("t")}
                    for trait in character.character_traits:
                        if trait.id not in existing_trait_ids:
                            # Add new trait element
                            new_trait = ET.SubElement(traits_elem, "t")
                            new_trait.set("id", str(trait.id))
                            self.logger.debug(f"  Added new trait {trait.name}")

                updated_count += 1

        self.logger.info(f"Updated {updated_count} characters in XML")

    def update_storage_to_xml(self):
        """Update XML tree with current storage data"""
        if self.xml_root is None:
            return
        
        self.logger.info("Updating XML with storage changes...")
        
        ships_elem = self.xml_root.find("ships")
        if ships_elem is None:
            self.logger.warning("No ships element in XML")
            return
        
        updated_count = 0
        
        for ship_elem in ships_elem.findall("ship"):
            ship_sid = int(ship_elem.get("sid", "0"))
            
            # Find matching ship in our data
            ship = None
            for s in self.ships:
                if s.sid == ship_sid:  # FIXED: was s.ship_sid
                    ship = s
                    break
            
            if ship is None or not ship.storage_containers:
                continue
            
            # Find all feat elements with eatAllowed (storage containers)
            feat_elements = ship_elem.findall(".//feat[@eatAllowed]")
            
            # Match feat elements to our containers (by index)
            for container_index, feat_elem in enumerate(feat_elements):
                if container_index >= len(ship.storage_containers):
                    break
                
                container = ship.storage_containers[container_index]
                
                # Find or create inv element
                inv_elem = feat_elem.find("inv")
                if inv_elem is None:
                    inv_elem = ET.SubElement(feat_elem, "inv")
                
                # Clear existing items
                for s_elem in list(inv_elem.findall("s")):
                    inv_elem.remove(s_elem)
                
                # Add current items
                for item in container.items:
                    if item.quantity > 0:  # Only add items with quantity > 0
                        s_elem = ET.SubElement(inv_elem, "s")
                        s_elem.set("elementaryId", item.item_id)
                        s_elem.set("inStorage", str(item.quantity))
                        s_elem.set("onTheWayIn", "0")
                        s_elem.set("onTheWayOut", "0")
                
                updated_count += 1
                self.logger.debug(f"  Updated storage container {container_index + 1} with {len(container.items)} items")
        
        self.logger.info(f"Updated {updated_count} storage containers in XML")

    def max_all_attributes(self):
        """Set all attributes to maximum"""
        if self.current_character:
            self.logger.info("Setting all attributes to maximum")
            for attr_id, editor in self.attribute_editors.items():
                editor.set_to_max()

    def max_all_skills(self):
        """Set all skills to absolute maximum (10)"""
        if self.current_character:
            self.logger.info("Setting all skills to absolute maximum (10)")
            for skill_id, editor in self.skill_editors.items():
                editor.set_to_max()

    def max_all_skills_to_learning(self):
        """Set all skills to their max learning potential"""
        if self.current_character:
            self.logger.info("Setting all skills to max learning potential")
            for skill_id, editor in self.skill_editors.items():
                editor.set_to_max_learning()

    def mark_as_modified(self):
        """Mark the save file as modified (needs saving)"""
        if self.current_file_path and "[Modified]" not in self.windowTitle():
            self.setWindowTitle(self.windowTitle() + " [Modified]")

    def sanitize_clone_character(self, char_elem):
        """Remove negative conditions and reset stats for a cloned character"""
        from PyQt6.QtWidgets import QMessageBox
        import xml.etree.ElementTree as ET
        
        changes_made = []
        
        # Reset stats in <props> element to healthy defaults
        props_elem = char_elem.find("props")
        if props_elem is not None:
            # Health: set to 100
            health_elem = props_elem.find("Health")
            if health_elem is not None:
                old_health = health_elem.get("v", "100")
                health_elem.set("v", "100")
                health_elem.set("ltv", "100")
                if old_health != "100":
                    changes_made.append(f"Health: {old_health} → 100")
            
            # Food: set to 100
            food_elem = props_elem.find("Food")
            if food_elem is not None:
                old_food = food_elem.get("v", "100")
                food_elem.set("v", "100")
                food_elem.set("ltv", "100")
                if old_food != "100":
                    changes_made.append(f"Food: {old_food} → 100")
            
            # Rest/Energy: set to 100
            rest_elem = props_elem.find("Rest")
            if rest_elem is not None:
                old_rest = rest_elem.get("v", "100")
                rest_elem.set("v", "100")
                rest_elem.set("ltv", "100")
                if old_rest != "100":
                    changes_made.append(f"Energy: {old_rest} → 100")
            
            # Comfort: set to 100
            comfort_elem = props_elem.find("Comfort")
            if comfort_elem is not None:
                old_comfort = comfort_elem.get("v", "100")
                comfort_elem.set("v", "100")
                comfort_elem.set("ltv", "100")
                if old_comfort != "100":
                    changes_made.append(f"Comfort: {old_comfort} → 100")
            
            # Mood: set to high positive value
            mood_elem = props_elem.find("Mood")
            if mood_elem is not None:
                old_mood = mood_elem.get("v", "50")
                mood_elem.set("v", "50")
                mood_elem.set("ltv", "50")
                if old_mood != "50":
                    changes_made.append(f"Mood: {old_mood} → 50")
        
        # Remove all conditions (safest approach - removes negative and temporary positive conditions)
        pers_elem = char_elem.find("pers")
        if pers_elem is not None:
            conditions_elem = pers_elem.find("conditions")
            if conditions_elem is not None:
                condition_count = len(conditions_elem.findall("c"))
                if condition_count > 0:
                    conditions_elem.clear()
                    changes_made.append(f"Removed {condition_count} condition(s)")
        
        # Clear relationships (clone starts fresh)
        if pers_elem is not None:
            sociality_elem = pers_elem.find("sociality")
            if sociality_elem is not None:
                relationships_elem = sociality_elem.find("relationships")
                if relationships_elem is not None:
                    rel_count = len(relationships_elem.findall("l"))
                    if rel_count > 0:
                        relationships_elem.clear()
                        changes_made.append(f"Cleared {rel_count} relationship(s)")
        
        return changes_made

    def add_crew_member(self):
        """Add a new crew member by cloning an existing one"""
        from PyQt6.QtWidgets import QInputDialog, QMessageBox
        import copy
        
        if not self.characters:
            QMessageBox.warning(
                self,
                "No Crew Members",
                "There are no crew members to clone. Please load a save file first."
            )
            return
        
        # Step 1: Select crew member to clone
        crew_options = []
        for char in self.characters:
            ship_name = "Unknown Ship"
            for ship in self.ships:
                if ship.sid == char.ship_sid:
                    ship_name = ship.sname
                    break
            crew_options.append(f"{char.character_name} (Ship: {ship_name})")
        
        clone_source_str, ok = QInputDialog.getItem(
            self,
            "Clone Crew Member",
            "Select crew member to clone:",
            crew_options,
            0,
            False
        )
        
        if not ok:
            return
        
        # Find the selected crew member
        clone_index = crew_options.index(clone_source_str)
        source_character = self.characters[clone_index]
        
        # Step 2: Ask for new crew member name
        new_name, ok = QInputDialog.getText(
            self,
            "New Crew Member Name",
            "Enter name for the new crew member:",
            text=f"{source_character.character_name} (Clone)"
        )
        
        if not ok or not new_name.strip():
            return
        
        new_name = new_name.strip()
        
        # Step 3: Select target ship (if multiple ships)
        if len(self.ships) > 1:
            ship_options = [f"{ship.sname} (SID: {ship.sid})" for ship in self.ships]
            target_ship_str, ok = QInputDialog.getItem(
                self,
                "Assign to Ship",
                "Select ship to assign the new crew member:",
                ship_options,
                0,
                False
            )
            
            if not ok:
                return
            
            target_ship_index = ship_options.index(target_ship_str)
            target_ship = self.ships[target_ship_index]
        else:
            # Only one ship, use it
            target_ship = self.ships[0]
        
        # Step 4: Generate unique entity ID
        existing_entity_ids = set()
        if self.xml_root is not None:
            ships_elem = self.xml_root.find("ships")
            if ships_elem is not None:
                for ship_elem in ships_elem.findall("ship"):
                    characters_elem = ship_elem.find("characters")
                    if characters_elem is not None:
                        for char_elem in characters_elem.findall("c"):
                            entity_id = int(char_elem.get("entId", "0"))
                            existing_entity_ids.add(entity_id)
        
        # Find next available entity ID
        new_entity_id = max(existing_entity_ids) + 1 if existing_entity_ids else 1
        
        # Step 5: Clone the character in memory
        new_character = Character()
        new_character.character_name = new_name
        new_character.character_entity_id = new_entity_id
        new_character.ship_sid = target_ship.sid

        # Copy attributes, skills, and traits (but not conditions/relationships - we'll add fresh ones)
        new_character.character_attributes = copy.deepcopy(source_character.character_attributes)
        new_character.character_skills = copy.deepcopy(source_character.character_skills)
        new_character.character_traits = copy.deepcopy(source_character.character_traits)
        # Don't copy conditions/relationships - clone starts fresh
        new_character.character_conditions = []
        new_character.character_relationships = []

        # Step 6: Max out all skills (set current to max learning)
        for skill in new_character.character_skills:
            skill.value = skill.max_value
        
        # Step 7: Clone the XML element from source character
        if self.xml_root is not None:
            ships_elem = self.xml_root.find("ships")
            if ships_elem is not None:
                # Find source character's XML element
                source_char_elem = None
                for ship_elem in ships_elem.findall("ship"):
                    if int(ship_elem.get("sid", "0")) == source_character.ship_sid:
                        characters_elem = ship_elem.find("characters")
                        if characters_elem is not None:
                            for char_elem in characters_elem.findall("c"):
                                if int(char_elem.get("entId", "0")) == source_character.character_entity_id:
                                    source_char_elem = char_elem
                                    break
                            break
                
                if source_char_elem is not None:
                    # Log all attributes of source character for debugging
                    self.logger.info(f"Source character XML attributes: {dict(source_char_elem.attrib)}")

                    # Find target ship's characters element
                    for ship_elem in ships_elem.findall("ship"):
                        if int(ship_elem.get("sid", "0")) == target_ship.sid:
                            characters_elem = ship_elem.find("characters")
                            if characters_elem is None:
                                # Create characters element if it doesn't exist
                                import xml.etree.ElementTree as ET
                                characters_elem = ET.SubElement(ship_elem, "characters")
                            
                            # Clone the XML element
                            import xml.etree.ElementTree as ET
                            new_char_elem = ET.fromstring(ET.tostring(source_char_elem))

                            # Check source character's health status and offer sanitization
                            source_issues = []
                            props_elem = source_char_elem.find("props")
                            if props_elem is not None:
                                # Check Health
                                health_elem = props_elem.find("Health")
                                if health_elem is not None:
                                    health_val = float(health_elem.get("v", "100"))
                                    if health_val < 100:
                                        source_issues.append(f"Health: {health_val:.0f}/100")

                                # Check Food
                                food_elem = props_elem.find("Food")
                                if food_elem is not None:
                                    food_val = float(food_elem.get("v", "100"))
                                    if food_val < 80:
                                        source_issues.append(f"Food: {food_val:.0f}/100")

                                # Check Rest/Energy
                                rest_elem = props_elem.find("Rest")
                                if rest_elem is not None:
                                    rest_val = float(rest_elem.get("v", "100"))
                                    if rest_val < 80:
                                        source_issues.append(f"Energy: {rest_val:.0f}/100")

                                # Check Mood
                                mood_elem = props_elem.find("Mood")
                                if mood_elem is not None:
                                    mood_val = float(mood_elem.get("v", "0"))
                                    if mood_val < 0:
                                        source_issues.append(f"Mood: {mood_val:.0f}")

                            # Check for conditions
                            pers_elem = source_char_elem.find("pers")
                            condition_count = 0
                            if pers_elem is not None:
                                conditions_elem = pers_elem.find("conditions")
                                if conditions_elem is not None:
                                    condition_count = len(conditions_elem.findall("c"))
                                    if condition_count > 0:
                                        source_issues.append(f"{condition_count} active condition(s)")

                            # If issues detected, offer to sanitize
                            should_sanitize = False
                            if source_issues:
                                issue_text = "\n• ".join(source_issues)
                                reply = QMessageBox.question(
                                    self,
                                    "Clone Character Issues Detected",
                                    f"The source character '{source_character.character_name}' has the following issues:\n\n• {issue_text}\n\n"
                                    f"Would you like to reset the clone's stats to healthy defaults?\n\n"
                                    f"YES: Reset Health/Energy/Food/Comfort/Mood to 100\n"
                                    f"NO: Keep current stats (may be injured/tired)\n\n"
                                    f"Note: Relationships and conditions are ALWAYS cleared for clones.",
                                    QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No | QMessageBox.StandardButton.Cancel,
                                    QMessageBox.StandardButton.Yes
                                )

                                if reply == QMessageBox.StandardButton.Cancel:
                                    self.logger.info("User cancelled crew addition")
                                    return
                                elif reply == QMessageBox.StandardButton.Yes:
                                    should_sanitize = True

                            # Update the cloned element's attributes
                            new_char_elem.set("name", new_name)
                            new_char_elem.set("entId", str(new_entity_id))

                            # ALWAYS clear relationships and conditions (clones start fresh socially)
                            changes = []
                            pers_elem = new_char_elem.find("pers")
                            if pers_elem is not None:
                                # Clear relationships
                                sociality_elem = pers_elem.find("sociality")
                                if sociality_elem is not None:
                                    relationships_elem = sociality_elem.find("relationships")
                                    if relationships_elem is not None:
                                        rel_count = len(relationships_elem.findall("l"))
                                        if rel_count > 0:
                                            relationships_elem.clear()
                                            changes.append(f"Cleared {rel_count} relationship(s)")
                                            self.logger.info(f"Cleared {rel_count} relationships from clone")

                                # Clear conditions
                                conditions_elem = pers_elem.find("conditions")
                                if conditions_elem is not None:
                                    condition_count = len(conditions_elem.findall("c"))
                                    if condition_count > 0:
                                        conditions_elem.clear()
                                        changes.append(f"Removed {condition_count} condition(s)")
                                        self.logger.info(f"Cleared {condition_count} conditions from clone")

                            # Reset stats if sanitization requested or no issues detected
                            if should_sanitize or not source_issues:
                                # Reset stats to healthy defaults
                                props_elem = new_char_elem.find("props")
                                if props_elem is not None:
                                    for stat_name, elem_name in [("Health", "Health"), ("Food", "Food"),
                                                                   ("Energy", "Rest"), ("Comfort", "Comfort"),
                                                                   ("Mood", "Mood")]:
                                        stat_elem = props_elem.find(elem_name)
                                        if stat_elem is not None:
                                            old_val = stat_elem.get("v", "100")
                                            target_val = "100" if elem_name != "Mood" else "50"
                                            if old_val != target_val:
                                                stat_elem.set("v", target_val)
                                                stat_elem.set("ltv", target_val)
                                                changes.append(f"{stat_name}: {old_val} → {target_val}")
                                self.logger.info("Reset stats to healthy defaults")

                            if changes:
                                self.logger.info(f"Sanitized clone: {', '.join(changes)}")

                            # Find a safe spawn position on the target ship
                            spawn_x = None
                            spawn_y = None

                            # Try to find an existing crew member on target ship to spawn near
                            for existing_char_elem in characters_elem.findall("c"):
                                existing_x = existing_char_elem.get("x")
                                existing_y = existing_char_elem.get("y")
                                if existing_x and existing_y:
                                    try:
                                        # Place new crew member offset from existing one
                                        spawn_x = float(existing_x) + 2.0
                                        spawn_y = float(existing_y) + 2.0
                                        self.logger.info(f"Found existing crew at ({existing_x}, {existing_y}), spawning new crew at ({spawn_x}, {spawn_y})")
                                        break
                                    except ValueError:
                                        continue

                            # If no crew found on target ship, use ship center as fallback
                            if spawn_x is None:
                                spawn_x = target_ship.sx / 2.0
                                spawn_y = target_ship.sy / 2.0
                                self.logger.warning(f"No existing crew on target ship, spawning at ship center ({spawn_x}, {spawn_y})")

                                # Warn user
                                reply = QMessageBox.warning(
                                    self,
                                    "No Crew on Target Ship",
                                    f"There are no existing crew members on {target_ship.sname}.\n\n"
                                    f"The new crew member will be placed at the ship center ({spawn_x:.1f}, {spawn_y:.1f}). "
                                    f"This location may not be a valid floor tile.\n\n"
                                    f"Do you want to continue?",
                                    QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No,
                                    QMessageBox.StandardButton.Yes
                                )

                                if reply == QMessageBox.StandardButton.No:
                                    self.logger.info("User cancelled crew addition due to spawn location concern")
                                    return

                            # Update position coordinates
                            new_char_elem.set("x", str(spawn_x))
                            new_char_elem.set("y", str(spawn_y))
                            new_char_elem.set("insx", str(int(spawn_x)))
                            new_char_elem.set("insy", str(int(spawn_y)))
                            self.logger.info(f"Set spawn position to x={spawn_x}, y={spawn_y}, insx={int(spawn_x)}, insy={int(spawn_y)}")

                            # Update skills in XML to be maxed
                            pers_elem = new_char_elem.find("pers")
                            if pers_elem is not None:
                                skills_elem = pers_elem.find("skills")
                                if skills_elem is not None:
                                    for s_elem in skills_elem.findall("s"):
                                        max_learning = s_elem.get("mxn", "0")
                                        s_elem.set("level", max_learning)

                            # Add the new character element to the target ship
                            characters_elem.append(new_char_elem)
                            break
        
        # Step 8: Add to characters list
        self.characters.append(new_character)
        
        # Step 9: Update UI - refresh crew list for the target ship
        self.update_crew_list(target_ship.sid)
        self.mark_as_modified()
        
        QMessageBox.information(
            self,
            "Crew Member Added",
            f"Successfully added {new_name} to {target_ship.sname}!"
        )
        
        self.logger.info(f"Added new crew member: {new_name} (Entity ID: {new_entity_id}) to ship {target_ship.sname}")
        
    def update_storage_containers(self, ship: Ship):
        """Update storage container list for the selected ship"""
        self.logger.info(f"Updating storage containers for ship {ship.sname}")
        self.container_combo.clear()
        
        container_count = 0
        for container in ship.storage_containers:
            self.container_combo.addItem(container.container_name, container)
            container_count += 1
            self.logger.debug(f"  Added container: {container.container_name} with {len(container.items)} items")
        
        self.logger.info(f"Added {container_count} storage containers to list")
        
        if container_count == 0:
            self.logger.warning(f"No storage containers found for ship {ship.sname}")
            
    def on_container_selected(self, index: int):
        """Handle storage container selection"""
        if index < 0:
            self.current_storage_container = None
            return
            
        container = self.container_combo.itemData(index)
        if container:
            self.current_storage_container = container
            self.display_storage_items(container)
            
    def display_storage_items(self, container: StorageContainer):
        """Display items in a storage container with editable quantities"""
        from PyQt6.QtWidgets import QPushButton
        from PyQt6.QtCore import Qt
        
        # Block signals during population to avoid triggering on_storage_item_changed
        self.storage_table.blockSignals(True)
        self.storage_table.setRowCount(0)
        
        # Calculate total quantity and update info label with capacity
        total_quantity = sum(item.quantity for item in container.items)
        capacity = container.capacity
        percentage = (total_quantity / capacity * 100) if capacity > 0 else 0
        
        # Color code based on capacity usage
        if percentage >= 90:
            color = "red"
        elif percentage >= 75:
            color = "orange"
        else:
            color = "gray"
        
        self.storage_info_label.setText(
            f'<span style="color: {color};">Total: {total_quantity}/{capacity} items ({percentage:.1f}% full)</span>'
        )
        
        for item in container.items:
            row = self.storage_table.rowCount()
            self.storage_table.insertRow(row)
            
            # ID column (read-only)
            id_item = QTableWidgetItem(item.item_id)
            id_item.setFlags(id_item.flags() & ~Qt.ItemFlag.ItemIsEditable)
            self.storage_table.setItem(row, 0, id_item)
            
            # Name column (read-only)
            name_item = QTableWidgetItem(item.item_name)
            name_item.setFlags(name_item.flags() & ~Qt.ItemFlag.ItemIsEditable)
            self.storage_table.setItem(row, 1, name_item)
            
            # Quantity column (editable)
            quantity_item = QTableWidgetItem(str(item.quantity))
            quantity_item.setData(256, item)  # Store reference to item
            self.storage_table.setItem(row, 2, quantity_item)
            
            # Actions column (delete button)
            delete_btn = QPushButton("Delete")
            delete_btn.clicked.connect(lambda checked, i=item: self.delete_storage_item(i))
            self.storage_table.setCellWidget(row, 3, delete_btn)
        
        # Re-enable signals
        self.storage_table.blockSignals(False)

    def populate_add_item_combo(self):
        """Populate the add item dropdown with all items organized by category"""
        self.add_item_combo.clear()
        
        # Organize items by category from IdCollection
        categories = {
            "--- BLOCKS ---": [
                (162, "Infrablock"),
                (1921, "Soft Block"),
                (930, "Techblock"),
                (1919, "Energy Block"),
                (1759, "Hull Block"),
                (1920, "Superblock"),
            ],
            "--- CONSTRUCTION MATERIALS ---": [
                (1922, "Steel Plates"),
                (173, "Electronic Component"),
                (1924, "Optronics Component"),
                (1925, "Quantronics Component"),
                (175, "Plastics"),
                (177, "Fabrics"),
            ],
            "--- RAW RESOURCES ---": [
                (157, "Base Metals"),
                (169, "Noble Metals"),
                (170, "Carbon"),
                (171, "Raw Chemicals"),
                (158, "Energium"),
                (172, "Hyperium"),
            ],
            "--- PROCESSED RESOURCES ---": [
                (174, "Energy Rod"),
                (1926, "Energy Cell"),
                (176, "Chemicals"),
                (178, "Hyperfuel"),
                (1932, "Fibers"),
            ],
            "--- SCRAP ---": [
                (127, "Rubble"),
                (1873, "Infra Scrap"),
                (1874, "Soft Scrap"),
                (1886, "Hull Scrap"),
                (1946, "Tech Scrap"),
                (1947, "Energy Scrap"),
            ],
            "--- FOOD ---": [
                (16, "Water"),
                (40, "Ice"),
                (15, "Root vegetables"),
                (706, "Fruits"),
                (707, "Artificial Meat"),
                (2657, "Nuts and Seeds"),
                (3378, "Grain and Hops"),
                (712, "Space Food"),
                (179, "Processed Food"),
            ],
            "--- WEAPONS (Ballistic) ---": [
                (760, "Five-Seven Pistol"),
                (728, "SMG"),
                (729, "Shotgun"),
                (725, "Assault Rifle"),
            ],
            "--- WEAPONS (Energy) ---": [
                (3070, "Laser Pistol"),
                (3069, "Laser Rifle"),
                (3071, "Plasma Cuttergun"),
                (3072, "Plasma Rifle"),
            ],
            "--- WEAPONS (Other) ---": [
                (3962, "Stun Pistol"),
                (3961, "Stun Rifle"),
            ],
            "--- WEAPON ATTACHMENTS ---": [
                (3968, "Basic Scope (Weapon Attachment)"),
                (3969, "Tactical Grip (Weapon Attachment)"),
                (3960, "Flamethrower (Weapon Attachment)"),
                (3967, "Explosive Grenade Launcher (Weapon Attachment)"),
                (4076, "Incendiary Grenade Launcher (Weapon Attachment)"),
            ],
            "--- ARMOR & CLOTHING ---": [
                (3384, "Armored Vest"),
                (4065, "Space Suit Oxygen Extender"),
            ],
            "--- MEDICAL ---": [
                (2053, "Medical Supplies"),
                (2058, "IV Fluid"),
                (4007, "Bandage"),
                (4030, "Nano Wound Dressing"),
                (4005, "Painkillers"),
                (4006, "Combat Stimulant"),
            ],
            "--- EQUIPMENT & TOOLS ---": [
                (3386, "Remote Control"),
                (3388, "Oxygen Tank"),
                (1152, "Sentry Gun X1"),
                (3419, "Augmentation Parts"),
                (2715, "Explosive Ammunition"),
                (4040, "Small Breach Charge"),
            ],
            "--- ORGANIC MATTER ---": [
                (71, "Bio Matter"),
                (2475, "Fertilizer"),
                (984, "Monster Meat"),
                (985, "Human Meat"),
                (1954, "Human Corpse"),
                (1955, "Monster Corpse"),
            ],
        }
        
        # Add items organized by category
        for category_name, items in categories.items():
            # Add category header (non-selectable)
            self.add_item_combo.addItem(category_name)
            # Disable the header item
            model = self.add_item_combo.model()
            index = self.add_item_combo.count() - 1
            item = model.item(index)
            item.setEnabled(False)
            item.setBackground(QColor("#f0f0f0"))
            
            # Add items in this category
            for item_id, item_name in items:
                self.add_item_combo.addItem(f"  {item_name}", item_id)
        
        self.logger.info(f"Populated add item combo with categorized items")
    
    def quick_add_item(self, quantity: int):
        """Add the selected item with the specified quantity to current storage"""
        if not self.current_storage_container:
            self.logger.warning("No storage container selected")
            return
        
        # Get selected item from combo
        item_id = self.add_item_combo.currentData()
        item_name = self.add_item_combo.currentText()
        
        if not item_id:
            self.logger.warning("No item selected in combo")
            return
        
        # Check capacity
        total_quantity = sum(item.quantity for item in self.current_storage_container.items)
        if total_quantity + quantity > self.current_storage_container.capacity:
            from PyQt6.QtWidgets import QMessageBox
            QMessageBox.warning(
                self,
                "Storage Capacity",
                f"Adding {quantity} items would exceed capacity!\n\n"
                f"Current: {total_quantity}/{self.current_storage_container.capacity}\n"
                f"After: {total_quantity + quantity}/{self.current_storage_container.capacity}\n\n"
                "Note: You can still add items, but the game may reject excess."
            )
        
        self.logger.info(f"Adding {quantity}x {item_name} (ID: {item_id}) to storage")
        
        # Check if item already exists in container
        existing_item = None
        for item in self.current_storage_container.items:
            if item.item_id == str(item_id):
                existing_item = item
                break
        
        if existing_item:
            # Update existing item quantity
            existing_item.quantity += quantity
            self.logger.info(f"Updated existing item to quantity {existing_item.quantity}")
        else:
            # Create new item
            from models import StorageItem
            new_item = StorageItem()
            new_item.item_id = str(item_id)
            new_item.item_name = item_name
            new_item.quantity = quantity
            self.current_storage_container.items.append(new_item)
            self.logger.info(f"Added new item with quantity {quantity}")
        
        # Refresh display
        self.display_storage_items(self.current_storage_container)
        self.mark_as_modified()

    def resupply_preset(self, preset_type: str, quantity: int):
        """Add multiple items based on preset type
        
        Args:
            preset_type: 'infra', 'life_support', or 'ship'
            quantity: Amount of each item to add
        """
        if not self.current_storage_container:
            self.logger.warning("No storage container selected")
            return
        
        # Define preset item sets
        presets = {
            "infra": [
                (162, "Infrablock"),
                (1921, "Soft Block"),
                (930, "Techblock"),
                (1919, "Energy Block"),
                (1759, "Hull Block"),
                (1920, "Superblock"),
            ],
            "life_support": [
                (16, "Water"),
                (40, "Ice"),
                (15, "Root vegetables"),
                (706, "Fruits"),
                (707, "Artificial Meat"),
                (2657, "Nuts and Seeds"),
            ],
            "ship": [
                # TODO: Add when we find item IDs
                # (???, "Energium"),
                # (???, "Hyperium"),
            ]
        }
        
        if preset_type not in presets:
            self.logger.error(f"Unknown preset type: {preset_type}")
            return
        
        items_to_add = presets[preset_type]
        if not items_to_add:
            self.logger.warning(f"Preset '{preset_type}' has no items defined yet")
            return
        
        # Calculate total space needed
        total_quantity = sum(item.quantity for item in self.current_storage_container.items)
        space_needed = len(items_to_add) * quantity
        space_available = self.current_storage_container.capacity - total_quantity
        
        if space_needed > space_available:
            from PyQt6.QtWidgets import QMessageBox
            reply = QMessageBox.question(
                self,
                "Storage Capacity Warning",
                f"This will add {space_needed} items, but only {space_available} slots are available.\n\n"
                f"Current: {total_quantity}/{self.current_storage_container.capacity}\n"
                f"After: {total_quantity + space_needed}/{self.current_storage_container.capacity}\n\n"
                "Continue anyway? (Game may reject excess items)",
                QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
            )
            if reply != QMessageBox.StandardButton.Yes:
                return
        
        self.logger.info(f"Resupplying with preset '{preset_type}' x{quantity}")

        # Add each item
        from models import StorageItem
        for item_id, item_name in items_to_add:
            # Check if item already exists
            existing_item = None
            for item in self.current_storage_container.items:
                if item.item_id == str(item_id):
                    existing_item = item
                    break
            
            if existing_item:
                existing_item.quantity += quantity
                self.logger.debug(f"  Updated {item_name}: +{quantity} (now {existing_item.quantity})")
            else:
                new_item = StorageItem()
                new_item.item_id = str(item_id)
                new_item.item_name = item_name
                new_item.quantity = quantity
                self.current_storage_container.items.append(new_item)
                self.logger.debug(f"  Added {item_name}: {quantity}")
        
        # Refresh display and mark modified
        self.display_storage_items(self.current_storage_container)
        self.mark_as_modified()
        
        self.logger.info(f"Resupply complete: added {space_needed} items")
    
    def on_storage_item_changed(self, item):
        """Handle manual quantity edits in the storage table"""
        if not item:
            return
        
        # Only handle quantity column changes (column 2)
        if item.column() != 2:
            return
        
        # Get the StorageItem reference
        storage_item = item.data(256)
        if not storage_item:
            return
        
        try:
            new_quantity = int(item.text())
            if new_quantity < 0:
                raise ValueError("Quantity cannot be negative")
            
            old_quantity = storage_item.quantity
            storage_item.quantity = new_quantity
            
            self.logger.info(f"Updated {storage_item.item_name} quantity: {old_quantity} → {new_quantity}")
            self.mark_as_modified()
            
            # Update info label
            if self.current_storage_container:
                total_quantity = sum(i.quantity for i in self.current_storage_container.items)
                self.storage_info_label.setText(f"Total items in storage: {total_quantity}")
        
        except ValueError as e:
            self.logger.error(f"Invalid quantity entered: {e}")
            # Restore original value
            item.setText(str(storage_item.quantity))
    
    def delete_storage_item(self, item: 'StorageItem'):
        """Delete an item from the current storage container"""
        if not self.current_storage_container:
            return
        
        self.logger.info(f"Deleting {item.item_name} from storage")
        
        # Remove item from container
        self.current_storage_container.items.remove(item)
        
        # Refresh display
        self.display_storage_items(self.current_storage_container)
        self.mark_as_modified()
            
    def add_storage_item(self):
        """Add a new storage item"""
        QMessageBox.information(
            self,
            "Not Implemented",
            "Adding storage items is not yet implemented in this version"
        )
        
    def remove_storage_item(self):
        """Remove a storage item"""
        current_row = self.storage_table.currentRow()
        if current_row >= 0:
            self.storage_table.removeRow(current_row)
            
    def reset_application_state(self):
        """Reset the application to initial state"""
        self.current_file_path = ""
        self.xml_tree = None
        self.xml_root = None
        self.characters.clear()
        self.ships.clear()
        self.current_save_info = None

        self.version_label.setText("Unknown")
        self.credits_input.setText("")
        self.prestige_input.setText("")
        self.sandbox_check.setChecked(False)
        self.ship_combo.clear()
        self.crew_list.clear()
        
        # Clear crew editors
        self.current_character = None
        self.attribute_editors.clear()
        self.skill_editors.clear()
        self.clear_editor_layout(self.attributes_layout)
        self.clear_editor_layout(self.skills_layout)
        self.clear_editor_layout(self.traits_layout)
        self.clear_editor_layout(self.conditions_layout)
        self.clear_editor_layout(self.relationships_layout)
        self.clear_editor_layout(self.job_priorities_layout)
        self.clear_editor_layout(self.appearance_layout)
        self.clear_editor_layout(self.equipment_layout)
        self.crew_name_edit.clear()
        self.crew_name_edit.setPlaceholderText("Select a crew member")
        self.crew_name_edit.setReadOnly(True)
        
        # Clear storage
        self.container_combo.clear()
        self.storage_table.setRowCount(0)

        self.setWindowTitle("Space Haven Save Editor - Python Edition")
        
    def show_first_run_setup(self):
        """Show first-run setup dialog"""
        from PyQt6.QtWidgets import QDialog, QVBoxLayout, QLabel, QCheckBox, QRadioButton, QButtonGroup, QPushButton, QGroupBox

        dialog = QDialog(self)
        dialog.setWindowTitle("Welcome to Space Haven Save Editor")
        dialog.setMinimumWidth(500)

        layout = QVBoxLayout(dialog)

        # Welcome message
        welcome_label = QLabel(
            "<h2>Welcome!</h2>"
            "<p>This appears to be your first time running the Space Haven Save Editor.</p>"
            "<p>Let's configure a few settings to get started.</p>"
        )
        welcome_label.setWordWrap(True)
        layout.addWidget(welcome_label)

        # Steam folder detection
        steam_group = QGroupBox("Save Folder Location")
        steam_layout = QVBoxLayout(steam_group)

        steam_folder = self.save_config.get_steam_saves_folder()
        if steam_folder:
            steam_check = QCheckBox(f"Use Steam saves folder")
            steam_check.setChecked(True)
            steam_layout.addWidget(steam_check)

            steam_path_label = QLabel(f"Detected: {steam_folder}")
            steam_path_label.setStyleSheet("color: green; margin-left: 20px; font-size: 10px;")
            steam_layout.addWidget(steam_path_label)

            steam_info = QLabel("You can change this later in Settings.")
            steam_info.setStyleSheet("color: gray; font-size: 10px; margin-left: 20px;")
            steam_layout.addWidget(steam_info)
        else:
            no_steam_label = QLabel("Steam saves folder not detected on this system.")
            no_steam_label.setStyleSheet("color: orange;")
            steam_layout.addWidget(no_steam_label)

            manual_label = QLabel("You'll use the file browser to select saves manually.")
            manual_label.setStyleSheet("font-size: 10px; color: gray;")
            steam_layout.addWidget(manual_label)
            steam_check = None

        layout.addWidget(steam_group)

        # Backup preferences
        backup_group = QGroupBox("Backup Preferences")
        backup_layout = QVBoxLayout(backup_group)

        backup_label = QLabel("Protect your saves with automatic backups:")
        backup_layout.addWidget(backup_label)

        backup_button_group = QButtonGroup(dialog)

        auto_backup_radio = QRadioButton("Automatic - Create backups automatically when loading saves (Recommended)")
        auto_backup_radio.setChecked(True)
        backup_button_group.addButton(auto_backup_radio, 1)
        backup_layout.addWidget(auto_backup_radio)

        manual_backup_radio = QRadioButton("Manual - Ask me before creating backups")
        backup_button_group.addButton(manual_backup_radio, 2)
        backup_layout.addWidget(manual_backup_radio)

        no_backup_radio = QRadioButton("None - Don't create backups")
        backup_button_group.addButton(no_backup_radio, 3)
        backup_layout.addWidget(no_backup_radio)

        backup_info = QLabel("Backups are stored as ZIP files. You can manage them in Settings.")
        backup_info.setStyleSheet("color: gray; font-size: 10px; margin-top: 5px;")
        backup_layout.addWidget(backup_info)

        layout.addWidget(backup_group)

        # Buttons
        button_layout = QHBoxLayout()
        button_layout.addStretch()

        ok_btn = QPushButton("Get Started")
        ok_btn.setDefault(True)
        ok_btn.clicked.connect(dialog.accept)
        button_layout.addWidget(ok_btn)

        layout.addLayout(button_layout)

        # Show dialog
        if dialog.exec():
            # Save Steam folder preference
            if steam_check and steam_check.isChecked():
                self.save_config.set_use_steam_folder(True)
                self.logger.info("First-run: Steam folder enabled")

            # Save backup preference
            if auto_backup_radio.isChecked():
                self.save_config.set_auto_backup(True)
                self.logger.info("First-run: Automatic backups enabled")
            elif manual_backup_radio.isChecked():
                self.save_config.set_auto_backup(False, manual_ok=True)
                self.logger.info("First-run: Manual backups enabled")
            else:
                self.save_config.set_auto_backup(False, manual_ok=False)
                self.logger.info("First-run: Backups disabled")

            # Save config (marks first run as complete)
            self.save_config.save_config()

            # Show welcome message
            QMessageBox.information(
                self,
                "Setup Complete",
                "Setup complete! You can change these settings anytime from the Settings menu.\n\n"
                "To get started, click File → Open Save File to load a Space Haven save."
            )

    def show_settings(self):
        """Show settings dialog"""
        from settings_dialog import SettingsDialog

        dialog = SettingsDialog(self.save_config, self)
        if dialog.exec():
            # Reload backup manager with new settings
            from pathlib import Path
            self.backup_manager = BackupManager(
                Path(self.save_config.config["backup_folder"]),
                max_days=self.save_config.config["backup_count"]
            )
            self.logger.info("Settings updated")
            
    def show_about(self):
        """Show about dialog"""
        QMessageBox.about(
            self,
            "About Space Haven Save Editor",
            "<h3>Space Haven Save Editor - Python Edition</h3>"
            "<p>A cross-platform save editor for Space Haven</p>"
            "<p>Original VB.NET version by <a href='https://github.com/moragar360'>Moragar</a></p>"
            "<p>Python port for Steam Deck compatibility</p>"
            "<p>Supports Space Haven Alpha 20</p>"
            "<p>License: MIT</p>"
        )
        
    def closeEvent(self, event):
        """Handle application close"""
        self.settings.setValue("backup_on_open", self.backup_enabled)
        event.accept()


def main():
    """Main entry point"""
    # Setup logging first
    logger = setup_logging()
    logger.info("Starting Space Haven Save Editor")
    
    try:
        app = QApplication(sys.argv)
        app.setApplicationName("Space Haven Save Editor")
        app.setOrganizationName("SpaceHavenEditor")
        
        logger.info("Creating main window")
        window = SpaceHavenEditor()
        window.show()
        
        logger.info("Application ready, entering event loop")
        exit_code = app.exec()
        logger.info(f"Application exiting with code {exit_code}")
        sys.exit(exit_code)
    except Exception as e:
        logger.error(f"Fatal error: {str(e)}", exc_info=True)
        raise


if __name__ == "__main__":
    main()
