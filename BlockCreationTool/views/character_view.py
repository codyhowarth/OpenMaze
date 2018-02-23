import tkinter as tk
from loader import *


class CharacterView(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.master = master
        self.scale_fields = \
            {"CamRotation": (0., 45.), "Height": (0.4, 2.), "MovementSpeed": (5., 100.), "RotationSpeed": (10., 500.),
            "TimeToRotate": (.5, 10.), "CharacterBound": (.25, .75), "DistancePickup": (0., 5.)}

        self.window = tk.Toplevel(master, padx=20, pady=20)
        self.window.title("Character Configuration")
        self.window.grid()
        self.i = 1
        self.back_button = tk.Button(self.window, text="")
        self.back_button.grid(row=0, column=0)
        self.entries = self.createScales()
        self.entries["CharacterStartPos"] = None



        print(self.entries)


    def createScales(self):
        entries = dict()

        for field in self.scale_fields:
            label = tk.Label(self.window, width=20, text=field, anchor=tk.W)
            scale = tk.Scale(self.window, command=lambda v, field=field: self.u(field, v),
                             showvalue=0, from_=self.scale_fields[field][0], to=self.scale_fields[field][1], length=200, orient=tk.HORIZONTAL,
                             resolution=.05)
            entry_var = tk.StringVar()
            entry_var.trace("w", lambda name, index, mode, entry_var=entry_var, field=field,
                                        scale=scale: self.e_u(entry_var, field, scale))
            entry = tk.Entry(self.window, width=4, text=field, textvariable=entry_var)

            scale.set(data["CharacterData"][field])
            #scale.bind("<ButtonRelease-1>", lambda _: write_to_file(data)) # WRITES TO FILE ON MOUSE RELEASE

            scale.grid(row=self.i, column=1)
            label.grid(row=self.i, column=0)
            entry.grid(row=self.i, column=2)
            entries[field] = (scale, entry, entry_var)
            self.i += 1
        return entries

    '''
    Callbacks for updating data
    '''
    def e_u(self, val, field, scale):
        print(val.get(), field)
        data["CharacterData"][field] = val.get()
        scale.set(data["CharacterData"][field])

    def u(self, field, val):
        self.entries[field][2].set(val)
