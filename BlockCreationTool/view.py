import tkinter as tk
import webbrowser


class View(tk.Frame):
    """Encapsulates of all the GUI logic.

    Attributes:
        master: where to open the Frame, by deafult root window
        title: Main Label

        character_btn: Button
        block_btn: Button
        trial_btn: Button
        pickup_btn: Button
    """

    def start_gui(self, ok=True):
        """Starts the GUI, if everything ok , to change
        """

        if ok:
            self.mainloop()
        else:
            self.master.destroy()

    def create_button(self):
        pass

    def createWidgets(self):
        """Create the set of initial widgets.

        """

        # Create the label

        self.title = tk.Label(
            self, text=" Block Creation Tool")
        self.title.grid(
            row=0, column=0, sticky=tk.E + tk.W)

        # Create the three buttons

        self.character_btn = tk.Button(self, width=20, height=2)
        self.character_btn["text"] = "Configure Character"
        self.character_btn.grid(row=1, column=0, sticky=tk.E + tk.W)

        self.block_btn = tk.Button(self, height=2)
        self.block_btn["text"] = "Manage Blocks"
        self.block_btn.grid(row=2, column=0, sticky=tk.E + tk.W)

        self.trial_btn = tk.Button(self, height=2)
        self.trial_btn["text"] = "Manage Trials"
        self.trial_btn.grid(row=3, column=0, sticky=tk.E + tk.W)

        self.pickup_btn = tk.Button(self, height=2)
        self.pickup_btn["text"] = "Configure Pickups"
        self.pickup_btn.grid(row=4, column=0, sticky=tk.E + tk.W)

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.grid(padx=50, pady=50, sticky=tk.N + tk.S)
        # option is needed to put the main label in the window
        self.createWidgets()
