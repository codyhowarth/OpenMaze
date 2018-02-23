import tkinter as tk

class TrialView(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self.window = tk.Toplevel(master)
        self.window.title("Trial Configuration")

        self.window.grid()

        for r in range(6):
            self.window.rowconfigure(r, weight=1)
        for c in range(5):
            self.window.columnconfigure(c, weight=1)


        self.LeftFrame = tk.Frame(self.window, bg="red")
        self.RightFrame = tk.Frame(self.window, bg="blue")

        self.LeftFrame.grid(row=0, column=0, rowspan=6, columnspan=2, sticky=tk.W+tk.E+tk.N+tk.S)
        self.RightFrame.grid(row=0, column=2, rowspan=6, columnspan=3, sticky=tk.W+tk.E+tk.N+tk.S)

        self.create_listbox()

    def create_listbox(self):

        list_box = tk.Listbox(self.LeftFrame, height=10)
        list_box.pack()

