import json
import jsonpickle

class Data:

    """
    Model for config

    args:
        character_data (Character) : contains data available to main character
        pickup_items (List (PickupItem)) : all available pickup items
        pillars (List (Pillar)): all of the pillars to be placed
        trial_data (List (Trial)) : all pre-defined trials
        wall_height (float) : wall height (:
        block_list (List (BlockData)) : list of all blocks
        block_order (List (int)) : order of the blocks are defined here

    """

    def __init__(self,
                 character_data=None,
                 pickup_items=[],
                 pillars=[],
                 trial_data=[],
                 wall_height=0.0,
                 block_list=[],
                 block_order=[]):

        self.CharacterData = character_data
        self.PickupItems = pickup_items
        self.Pillars = pillars
        self.TrialData = trial_data
        self.WallHeight = wall_height
        self.BlockList = block_list
        self.BlockOrder = block_order

    class BlockData:
        """
        Model for blocks

        args:
            end_goal (string) : percentage __SPACE__ number, arbitrary
            pickup_items (string) : function name
            block_name (string):
            notes (string) :
            replacement (int) : int value representing replacement
            block_list (List (int)) : list of all possible random values
            block_order (List (int)) : trial order (-1 means random)

        """

        def __init__(self,
                     end_goal="",
                     end_function="",
                     block_name="",
                     notes="",
                     replacement=0,
                     random_trial_type=[],
                     trial_order=[]):

            self.EndGoal = end_goal
            self.EndFunction = end_function
            self.BlockName = block_name
            self.Notes = notes
            self.Replacement = replacement
            self.RandomTrialType = random_trial_type
            self.TrialOrder = trial_order

    class Trial:
        """
        Model for trials

        args:
            two_dimensional (int) : 1 iff trial is two dimensional
            file_location (string) : null if not an image trial
            environment_type (int): environment type by id
            sides (int) : number of sides present in the trial
            color (string) : hex color of walls
            radius (int) : radius of walls
            pickup_type (int) : pickup type associated with the block
            time_allotted (int) :
            pillar_color (string) :
            has_recursive_trial (int) : 1 iff there is a recursive trial referenced
            random_loc (int) : whether or not the pickup has a random location
            pickup_visible (int) : visibility of the pikcup
            note (string) :
        """
        def __init__(self,
                     two_dimensional=0,
                     file_location="",
                     environment_type=0,
                     sides=0,
                     color="",
                     radius=0,
                     pickup_type=0,
                     time_allotted=0,
                     pillar_color="",
                     has_recursive_trial=0,
                     recursive_reference=0,
                     random_loc=0,
                     pickup_visible=0,
                     note=""):
            self.TwoDimensional = two_dimensional
            self.FileLocation = file_location,
            self.EnvironmentType = environment_type
            self.Sides = sides
            self.Color = color
            self.Radius = radius
            self.PickupType = pickup_type
            self.TimeAllotted = time_allotted
            self.PillarColor = pillar_color
            self.HasRecursiveTrial = has_recursive_trial
            self.RecursiveTrialReference = recursive_reference
            self.RandomLoc = random_loc
            self.PickupVisible = pickup_visible
            self.Note = note

    class PickupItem:
        """
        Model for pickup item

        args:
            count (int) : number of pickups to generate (usually 1)
            tag (string) : name of pickup item
            color (int) : the color in hex, without #
            sound_location (int) : the file path of the sound
            file (string) : python file that will generate the position
            size (int) : size of the object
        """
        def __init__(self,
                     count=0,
                     tag="",
                     color="",
                     sound="",
                     file="",
                     size=0.0):
            self.Count = count
            self.Tag = tag
            self.Color = color
            self.SoundLocation = sound
            self.PythonFile = file
            self.Size = size

    class Character:
        """
            Model for point

            args:
                cam_rotation (float) : rotation of the initial pan of the field
                height (float) : height of the camera
                movement_speed (float) : movement speed of player
                rotation_speed (float) : rotation speed of character
                time_to_rotate (float) : how long the delay can last in the rotation
                output_file (string) : output file of the player movement during an experiment
                character_start_pos (Point) : start position of the character
                character_bound (float) :
                distance_pickup (float) :
        """
        def __init__(self,
                     cam_rotation=0.0,
                     height=0.0,
                     movement_speed=0.0,
                     rotation_speed=0.0,
                     time_to_rotate=0,
                     output_file="",
                     character_start_pos=None,
                     character_bound=0.0,
                     distance_pickup=0.0):

            self.CamRotation = cam_rotation
            self.Height = height
            self.MovementSpeed = movement_speed
            self.RotationSpeed = rotation_speed
            self.TimeToRotate = time_to_rotate
            self.OutputFile = output_file
            self.CharacterStartPos = character_start_pos
            self.CharacterBound = character_bound
            self.DistancePickup = distance_pickup

    class Point:
        """
        Model for point

        args:
            x (float) :
            y (float) :
        """

        def __init__(self, x=0.0, y=0.0):
            self.X = x
            self.Y = y

    class Pillar:
        """
        Model for pilar

        args:
            x (float) :
            y (float) :
            radius (float):
            height (float) :
        """

        def __init__(self, x=0.0, y=0.0, radius=0.0, height=0.0):
            self.X = x
            self.Y = y
            self.Radius = radius
            self.Height = height


def jdefault(o):
    return o.__dict__


p = Data.Pillar(1.0, 1.0, 3.0, 2.0)
d = Data(character_data=None,
         pickup_items=[],
         pillars=[p],
         trial_data=[],
         wall_height=1.0,
         block_list=[],
         block_order=[])

frozen = jsonpickle.encode(d)

print(frozen)