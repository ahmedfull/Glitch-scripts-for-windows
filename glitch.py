import mss
import numpy as np
import tkinter as tk
from PIL import Image, ImageTk
from pynput import keyboard
import time
import random

# ---------- SETTINGS ----------
FPS = 30
GLITCH_DURATION = 0.05
MIN_GLITCH_INTERVAL = 15.0
MAX_GLITCH_INTERVAL = 45.0
INTENSITY = 0.8
# ------------------------------

rng = np.random.default_rng()

def apply_glitch(arr, intensity):
    out = arr.copy()
    h, w = arr.shape[:2]

    n_slices = int(h * 0.1)
    for _ in range(n_slices):
        y0 = int(rng.integers(0, max(2, h - 5)))
        y1 = min(y0 + int(rng.integers(2, 6)), h)
        shift = int(rng.integers(-20 * intensity, 20 * intensity + 1))
        out[y0:y1] = np.roll(out[y0:y1], shift, axis=1)

    ch = int(rng.choice(3))
    off = int(rng.integers(2, int(10 * intensity) + 2))
    out[:, :, ch] = np.roll(out[:, :, ch], off, axis=1)

    if rng.random() < 0.8:
        y0 = int(rng.integers(0, h - 5))
        y1 = min(y0 + int(rng.integers(10, 30)), h)
        out[y0:y1] = 255 - out[y0:y1]

    return out

def main():
    root = tk.Tk()
    root.attributes("-fullscreen", True)
    root.attributes("-topmost", True)
    root.config(bg="black")
    root.title("GLITCH")
    root.protocol("WM_DELETE_WINDOW", lambda: root.destroy())

    label = tk.Label(root, bg="black")
    label.pack(fill=tk.BOTH, expand=True)

    running = True

    def on_press(key):
        nonlocal running
        if key == keyboard.Key.esc:
            running = False

    keyboard.Listener(on_press=on_press).start()

    glitch_end_time = -1.0

    with mss.MSS() as sct:
        monitor = sct.monitors[1]

        def tick():
            nonlocal glitch_end_time, running
            if not running:
                root.destroy()
                return

            shot = sct.grab(monitor)
            arr = np.array(shot)[:, :, :3]
            now = time.time()

            # Check if we should glitch RIGHT NOW
            should_glitch = now < glitch_end_time

            if should_glitch:
                arr = apply_glitch(arr, INTENSITY)
            else:
                # We just exited a glitch, schedule the next one
                if glitch_end_time > 0 and now >= glitch_end_time:
                    glitch_end_time = now + random.uniform(MIN_GLITCH_INTERVAL, MAX_GLITCH_INTERVAL)

            img = Image.fromarray(arr)
            img.thumbnail((monitor["width"], monitor["height"]))
            photo = ImageTk.PhotoImage(img)
            label.config(image=photo)
            label.image = photo

            root.after(int(1000 / FPS), tick)

        # Initialize: schedule first glitch
        glitch_end_time = time.time() + random.uniform(MIN_GLITCH_INTERVAL, MAX_GLITCH_INTERVAL)
        tick()
        root.mainloop()

if __name__ == "__main__":
    main()