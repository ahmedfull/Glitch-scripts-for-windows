import mss
import numpy as np
import tkinter as tk
from PIL import Image, ImageTk
from pynput import keyboard

# ---------- SETTINGS ----------
FPS = 30            # higher = smoother, more CPU
GLITCH_CHANCE = 0.35  # chance a frame gets glitched (rest stay clean = feels real)
INTENSITY = 0.6     # 0.0 - 1.0
# ------------------------------

rng = np.random.default_rng()

def slice_shift(im):
    out = im.copy()
    h = out.shape[0]
    for _ in range(int(h * 0.12)):
        y0 = int(rng.integers(0, h - 10))
        y1 = min(y0 + int(rng.integers(3, 25)), h)
        shift = int(rng.integers(int(-60 * INTENSITY), int(60 * INTENSITY) + 1))
        out[y0:y1] = np.roll(out[y0:y1], shift, axis=1)
    return out

def channel_split(im):
    out = im.copy()
    ch = int(rng.choice(3))
    off = int(rng.integers(2, int(20 * INTENSITY) + 2))
    out[:, :, ch] = np.roll(out[:, :, ch], off, axis=1)
    return out

def invert_bands(im):
    out = im.copy()
    h = out.shape[0]
    y0 = int(rng.integers(0, h - 5))
    y1 = min(y0 + int(rng.integers(2, 12)), h)
    out[y0:y1] = 255 - out[y0:y1]
    return out

def scanlines(im):
    out = im.copy()
    out[::2] = (out[::2] * 0.6).astype(np.uint8)
    return out

def apply_glitch(arr):
    if rng.random() < GLITCH_CHANCE:
        arr = slice_shift(arr)
        arr = channel_split(arr)
        if rng.random() < 0.5:
            arr = invert_bands(arr)
    return scanlines(arr)

# ---------- QUIT WITH ESC (works in background) ----------
running = True
def on_press(key):
    global running
    if key == keyboard.Key.esc:
        running = False
keyboard.Listener(on_press=on_press).start()

# ---------- SINGLE WINDOW, UPDATED IN PLACE ----------
root = tk.Tk()
root.attributes("-fullscreen", True)
root.attributes("-topmost", True)
root.config(bg="black")
root.title("GLITCH")
label = tk.Label(root, bg="black")
label.pack(fill=tk.BOTH, expand=True)

with mss.mss() as sct:
    monitor = sct.monitors[1]  # first monitor; use sct.monitors[0] for all monitors
    def tick():
        if not running:
            root.destroy()
            return
        shot = sct.grab(monitor)
        arr = np.array(shot)[:, :, :3]  # BGRA -> RGB-ish, drop alpha
        glitched = apply_glitch(arr)
        img = Image.fromarray(glitched)
        img.thumbnail((monitor["width"], monitor["height"]))
        photo = ImageTk.PhotoImage(img)
        label.config(image=photo)
        label.image = photo  # keep a reference so it isn't garbage collected
        root.after(int(1000 / FPS), tick)

    tick()
    root.mainloop()

print("Stopped.")