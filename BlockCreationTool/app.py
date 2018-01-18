import tkinter as tk


class Window(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self._master = master
        self.pack()
        self._init_window()
        self._menu_bar = MenuBar(self._master)

    def _init_window(self):
        self.master.title("Block Creation Tool")
        self.quit_button = tk.Button(self, text="Exit", command=self._client_exit, fg="red")
        self.quit_button.pack(side="bottom")

    def _client_exit(self):
        exit()


class MenuBar(tk.Frame):

    def __init__(self, master=None):
        tk.Frame.__init__(self, master)
        self._master = master
        self._init_menu()

    def _init_menu(self):
        self._menu = tk.Menu(self._master)
        self._master.config(menu=self._menu)
        file = tk.Menu(self._menu)
        file.add_command(label="Load Trials", command=self._load_trials)
        self._menu.add_cascade(label="File", menu=file)

    def _load_trials(self):
        print("HI")

if __name__ == '__main__':
    root = tk.Tk()
    root.geometry("480x640")
    app = Window(root)
    root.mainloop()