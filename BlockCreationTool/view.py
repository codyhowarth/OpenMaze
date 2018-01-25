import tkinter as tk
import webbrowser


class View(tk.Frame):
    """Encapsulates of all the GUI logic.

    Attributes:
        master: where to open the Frame, by deafult root window
        title: Main Label

        one: Button
        two: Button
        three: Button
    """

    def start_gui(self, ok=True):
        """Starts the GUI, if everything ok , to change
        """

        if ok:
            self.mainloop()
        else:
            self.master.destroy()

    def createWidgets(self):
        """Create the set of initial widgets.

        """

        # Create the label

        self.title = tk.Label(
            self, text=" Block Creation Tool")
        self.title.grid(
            row=0, column=0, columnspan=4, sticky=tk.N + tk.S)

        # Create the three buttons

        self.one = tk.Button(self)
        self.one["text"] = "Character"
        self.one.grid(row=1, column=0)

        self.two = tk.Button(self)
        self.two["text"] = "Blocks"
        self.two.grid(row=2, column=0)

        self.three = tk.Button(self)
        self.three["text"] = "Trials"
        self.three.grid(row=3, column=0)

        self.four = tk.Button(self)
        self.four["text"] = "Pickups"
        self.four.grid(row=4, column=0)

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.grid()
        # option is needed to put the main label in the window
        self.createWidgets()
