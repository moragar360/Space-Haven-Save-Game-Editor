"""
Data models for Space Haven Save Editor
"""

from typing import List, Optional


class DataProp:
    """Represents a data property with ID, name, and value"""
    def __init__(self, id: int = 0, name: str = "", value: int = 0, max_value: int = 0):
        self.id = id
        self.name = name
        self.value = value
        self.max_value = max_value


class RelationshipInfo:
    """Represents a relationship between characters"""
    def __init__(self, target_entity_id: int = 0, target_name: str = "", 
                 friendship: int = 0, attraction: int = 0, compatibility: int = 0,
                 best_friends: bool = False, lovers: bool = False):
        self.target_entity_id = target_entity_id
        self.target_name = target_name
        self.friendship = friendship
        self.attraction = attraction
        self.compatibility = compatibility
        self.best_friends = best_friends
        self.lovers = lovers
    
    def get_relationship_type(self) -> str:
        """Get human-readable relationship type"""
        if self.lovers:
            return "Lovers"
        elif self.best_friends and self.friendship >= 70:
            return "Best Friends"
        elif self.friendship >= 40:
            return "Friends"
        elif self.friendship >= 20:
            return "Acquaintance"
        elif self.friendship < 0:
            return "Enemies"
        elif self.friendship < 20:
            return "Dislike"
        else:
            return "Neutral"


class Character:
    """Represents a character/crew member"""
    def __init__(self):
        self.character_name: str = ""
        self.character_last_name: str = ""  # Last name
        self.character_entity_id: int = 0
        self.ship_sid: int = 0  # Current ship location
        self.homeship_sid: int = 0  # Assigned homeship for sleeping/quarters
        self.character_stats: List[DataProp] = []
        self.character_attributes: List[DataProp] = []
        self.character_skills: List[DataProp] = []
        self.character_traits: List[DataProp] = []
        self.character_conditions: List[DataProp] = []
        self.character_relationships: List[RelationshipInfo] = []
        # Job priorities (profession -> priority mapping)
        self.job_priorities: dict = {}
        # Appearance data
        self.appearance: dict = {}  # bb, bs, bh, bp, orgColor, colorSet
        self.colors: dict = {}  # from <colors> element
        # Loadout/Equipment
        self.loadout: dict = {}  # headgear, armor, primary, attachment, secondary, pocket1-3
        self.augmentations: dict = {}  # primary, secondary augs
        self.inventory_items: list = []  # items in personal inventory


class StorageItem:
    """Represents an item in storage"""
    def __init__(self, item_id: str = "", item_name: str = "", quantity: int = 0):
        self.item_id = item_id
        self.item_name = item_name
        self.quantity = quantity


class StorageContainer:
    """Represents a storage container on a ship"""
    def __init__(self, container_id: int = 0, container_name: str = ""):
        self.container_id = container_id
        self.container_name = container_name
        self.items: List[StorageItem] = []
        self.capacity: int = 250  # Default to large storage capacity


class Ship:
    """Represents a ship"""
    def __init__(self, sid: int = 0, sname: str = "", sx: int = 0, sy: int = 0):
        self.sid = sid
        self.sname = sname
        self.sx = sx
        self.sy = sy
        self.storage_items: List[StorageItem] = []
        self.storage_containers: List[StorageContainer] = []

    def __str__(self):
        return f"{self.sname} ({self.sx}x{self.sy})"
