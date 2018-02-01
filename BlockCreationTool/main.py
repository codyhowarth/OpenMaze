import tkinter as tk
from view import *
from controller import Controller

def main():

    controller = Controller()

    root = tk.Tk()
    root.resizable(width=False, height=False)
    root.geometry()
    root.title("Block Creation Tool")

    controller.init_view(root)


if __name__ == "__main__":
    main()