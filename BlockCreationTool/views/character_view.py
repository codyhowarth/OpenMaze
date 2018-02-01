import tkinter as tk

class CharacterView(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.window = tk.Toplevel(master)
        self.window.title("Character Configuration")