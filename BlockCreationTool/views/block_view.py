import tkinter as tk

class BlockView(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.window = tk.Toplevel(master)
        self.window.title("Block Configuraton")