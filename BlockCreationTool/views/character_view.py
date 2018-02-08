import tkinter as tk

class CharacterView(tk.Frame):



    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.master = master
        self.fields = "cam_rotation", "height", "movement_speed", "rotation_speed", \
            "time_to_rotate", "output_file", "character_start_pos", "character_bound", \
            "distance_pickup"
        self.window = tk.Toplevel(master, padx=20, pady=20)
        self.window.title("Character Configuration")
        self.window.grid()
        self.entries = self.createWidgets()
        print(self.entries)

    def createWidgets(self):
        entries = dict()
        i = 0
        for field in self.fields:
            label = tk.Label(self.window, width=20, text=field, anchor=tk.W)
            #entry = tk.Entry(self.window)
            entry_var = tk.StringVar()
            entry = tk.Entry(self.window, width=4, text=field, textvariable=entry_var)
            scale = tk.Scale(self.window, command= lambda v, field=field: self.u(field, v), showvalue=0, from_=0, to=200, length=200, orient=tk.HORIZONTAL)
            scale.grid(row=i, column=1)
            label.grid(row=i, column=0)
            entry.grid(row=i, column=2)
            entries[field] = (scale, entry, entry_var)
            i+= 1
        return entries

    def u(self, field, val):
        print(field)
        self.entries[field][2].set(val)
