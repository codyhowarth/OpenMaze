import tkinter as tk
from loader import *

class PickupView(tk.Frame):


    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.window = tk.Toplevel(master)
        self.window.title("Pickup Configuration")

        self.window.grid()

        for r in range(5):
            self.window.rowconfigure(r, weight=1)
        tk.Button(self.window, text="New Pickup", command=self.add_pickup).grid(row=6)

        for c in range(5):
            self.window.columnconfigure(c, weight=1)
        tk.Button(self.window, text="Save", command=self.update).grid(row=6, column=2)

        self.LeftFrame = tk.Frame(self.window)
        self.RightFrame = tk.Frame(self.window)
        self.LeftFrame.grid(row=0, column=0, rowspan=6, columnspan=2, sticky=tk.W+tk.E+tk.N+tk.S)
        self.RightFrame.grid(row=0, column=2, rowspan=6, columnspan=3, sticky=tk.W+tk.E+tk.N+tk.S)

        self.list_box = self.create_listbox()

        self.fields = \
            {"Color": tk.Entry, "SoundLocation": tk.Entry, "PythonFile": tk.Entry,
             "Size": tk.Scale, "PrefabName": tk.Entry, "Loc": tk.Entry}

        self.current = self.list_box.curselection()

        self.RightFrame.grid()

        self.color_label = tk.Label(self.RightFrame, width=20, text="Color", anchor=tk.W)
        self.sound_label = tk.Label(self.RightFrame, width=20, text="SoundLocation", anchor=tk.W)
        self.file_label = tk.Label(self.RightFrame, width=20, text="PythonFile", anchor=tk.W)
        self.size_label = tk.Label(self.RightFrame, width=20, text="Size", anchor=tk.W)
        self.prefab_label = tk.Label(self.RightFrame, width=20, text="PrefabName", anchor=tk.W)
        self.loc_label = tk.Label(self.RightFrame, width=20, text="Loc", anchor=tk.W)

        self.color_label.grid(row=0, column=0)
        self.sound_label.grid(row=1, column=0)
        self.file_label.grid(row=2, column=0)
        self.size_label.grid(row=3, column=0)
        self.prefab_label.grid(row=4, column=0)
        self.loc_label.grid(row=5, column=0)

        self.color_var = tk.StringVar()
        self.sound_var = tk.StringVar()
        self.file_var = tk.StringVar()
        self.prefab_var = tk.StringVar()
        self.loc_entry1_var = tk.StringVar()
        self.loc_entry2_var = tk.StringVar()

        self.color_entry = tk.Entry(self.RightFrame, width=7, text="#FF0000", textvariable=self.color_var)
        self.sound_entry = tk.Entry(self.RightFrame, width=15, text="", textvariable=self.sound_var)
        self.file_entry = tk.Entry(self.RightFrame, width=15, text="", textvariable=self.file_var)
        self.size_scale = tk.Scale(self.RightFrame, length=150, orient=tk.HORIZONTAL, resolution=.05, from_=0.5, to=2.0)
        self.prefab_entry = tk.Entry(self.RightFrame, width=15, text="", textvariable=self.prefab_var)
        self.loc_entry1 = tk.Entry(self.RightFrame, width = 5, text="", textvariable=self.loc_entry1_var)
        self.loc_entry2 = tk.Entry(self.RightFrame, width=5, text="", textvariable=self.loc_entry2_var)

        self.color_entry.grid(row=0, column=1, columnspan=2)
        self.sound_entry.grid(row=1, column=1, columnspan=2)
        self.file_entry.grid(row=2, column=1, columnspan=2)
        self.size_scale.grid(row=3, column=1, columnspan=2)
        self.prefab_entry.grid(row=4, column=1, columnspan=2)
        self.loc_entry1.grid(row=5, column=1, columnspan=1)
        self.loc_entry2.grid(row=5, column=2, columnspan=1)

        self.poll()

    def poll(self):
        now = self.list_box.curselection()

        if now != self.current:
            self.current = now
            pickup_data = data["PickupItems"][self.current[0]]
            self.color_var.set(pickup_data["Color"])
            self.sound_var.set(pickup_data["SoundLocation"])
            self.file_var.set(pickup_data["PythonFile"])
            self.size_scale.set(pickup_data["Size"])
            self.prefab_var.set(pickup_data["PrefabName"])
            self.loc_entry1_var.set(pickup_data["Loc"][0])
            self.loc_entry2_var.set(pickup_data["Loc"][1])

        self.after(100, self.poll)

    def create_listbox(self):

        pickups = []
        for pickup in data["PickupItems"]:
            pickups.append(pickup["Tag"])

        list_box = tk.Listbox(self.LeftFrame)
        list_box.pack()

        for pickup in pickups:
            list_box.insert(tk.END, pickup)

        if len(pickups) > 0:
            list_box.select_set(0)

        return list_box

    def add_pickup(self):
        pickup_name = input("Enter pickup name: ")
        self.list_box.insert(tk.END, pickup_name)
        data["PickupItems"].append({
            "Tag": pickup_name,
            "Color": "ff0000",
            "SoundLocation": "eat",
            "PythonFile": "Example.py",
            "Size": 1.0,
            "PrefabName": "Pickup",
            "Loc": [5, 5]
        })

    def update(self):

        pickup_data = data["PickupItems"][self.current[0]]
        pickup_data["Color"] = self.color_var.get()
        pickup_data["SoundLocation"] = self.sound_var.get()
        pickup_data["PythonFile"] = self.file_var.get()
        pickup_data["Size"] = self.size_scale.get()
        pickup_data["PrefabName"] = self.prefab_var.get()
        pickup_data["Loc"] = [self.loc_entry1_var.get(), self.loc_entry2_var.get()]