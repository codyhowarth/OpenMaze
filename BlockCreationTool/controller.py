from view import View
import tkinter as tk
from views.character_view import *
from views.block_view import *
from views.trial_view import *
from views.pickup_view import *


class Controller(object):

    def open_settings(self, v_type, args=None):
        return {
            1: lambda: CharacterView(args['root']),
            2: lambda: BlockView(args['root']),
            3: lambda: TrialView(args['root']),
            4: lambda: PickupView(args['root'])
        }.get(v_type)()

    def init_view(self, root):
        """Initializes GUI view
            In addition it bindes the Buttons with the callback methods.

        """
        self.view = View(master=root)
        args = {
            "root": root
        }
        # Bind buttons with callback methods
        self.view.character_btn["command"] = lambda args=args: self.open_settings(1, args)
        self.view.block_btn["command"] = lambda args=args: self.open_settings(2, args)
        self.view.trial_btn["command"] = lambda args=args: self.open_settings(3, args)
        self.view.pickup_btn["command"] = lambda args=args: self.open_settings(4, args)

        # Start the gui
        self.view.start_gui()
