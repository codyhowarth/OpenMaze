import tkinter as tk
from loader import *

class BlockView(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.window = tk.Toplevel(master)
        self.window.title("Block Configuration")

        self.window.grid()

        for r in range(5):
            self.window.rowconfigure(r, weight=1)
        tk.Button(self.window, text="New Block", command=self.add_block).grid(row=6)

        for c in range(5):
            self.window.columnconfigure(c, weight=1)
        tk.Button(self.window, text="Save").grid(row=6, column=2)
        self.LeftFrame = tk.Frame(self.window)
        self.RightFrame = tk.Frame(self.window)
        self.LeftFrame.grid(row=0, column=0, rowspan=6, columnspan=2, sticky=tk.W + tk.E + tk.N + tk.S)
        self.RightFrame.grid(row=0, column=2, rowspan=6, columnspan=3, sticky=tk.W + tk.E + tk.N + tk.S)

        self.list_box = self.create_listbox()

        self.notes_label = tk.Label(self.RightFrame, width=20, text="Notes", anchor=tk.W)
        self.function_label = tk.Label(self.RightFrame, width=20, text="EndFunction", anchor=tk.W)
        self.goal_label = tk.Label(self.RightFrame, width=20, text="EndGoal", anchor=tk.W)

        self.notes_var = tk.StringVar()
        self.function_var = tk.StringVar()
        self.goal_var1 = tk.StringVar()
        self.goal_var2 = tk.StringVar()

        self.notes_label.grid(row=0, column=0)
        self.function_label.grid(row=1, column=0)
        self.goal_label.grid(row=2, column=0)

        self.notes_entry = tk.Entry(self.RightFrame, width=15, textvariable=self.notes_var)
        self.function_entry = tk.Entry(self.RightFrame, width=15, textvariable=self.function_var)
        self.goal_entry1 = tk.Entry(self.RightFrame, width=5, textvariable=self.goal_var1)
        self.goal_entry2 = tk.Entry(self.RightFrame, width=5, textvariable=self.goal_var2)

        self.notes_entry.grid(row=0, column=1, columnspan=2)
        self.function_entry.grid(row=1, column=1, columnspan=2)
        self.goal_entry1.grid(row=2, column=1)
        self.goal_entry1.grid(row=2, column=2)


    def create_listbox(self):

        pickups = []
        for pickup in data["BlockList"]:
            pickups.append(pickup["BlockName"])

        list_box = tk.Listbox(self.LeftFrame)
        list_box.pack()

        for pickup in pickups:
            list_box.insert(tk.END, pickup)

        if len(pickups) > 0:
            list_box.select_set(0)

        return list_box

    def add_block(selfs):
        pass