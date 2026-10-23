"""Draws the kite sprites for Paddle Out (pickup icon + flying kite with string)."""
import sys
import math
from PIL import Image, ImageDraw

OUT = sys.argv[1]
S = 4  # supersampling factor for smooth edges

OUTLINE = (58, 40, 40, 255)
RED = (232, 69, 60, 255)
YELLOW = (247, 201, 72, 255)
TEAL = (43, 181, 176, 255)
WHITE = (250, 246, 236, 255)
STRING = (70, 60, 55, 255)


def draw_kite(d, cx, cy, w, h):
    """Diamond kite centered at (cx, cy); returns the bottom tip position."""
    top, right, bottom, left = (cx, cy - h * 0.38), (cx + w / 2, cy - h * 0.08), (cx, cy + h * 0.62), (cx - w / 2, cy - h * 0.08)
    mid = (cx, cy - h * 0.08)
    for poly, col in [((top, right, mid), RED), ((right, bottom, mid), YELLOW),
                      ((bottom, left, mid), RED), ((left, top, mid), TEAL)]:
        d.polygon(poly, fill=col)
    ow = int(3.2 * S)
    d.line([top, right, bottom, left, top], fill=OUTLINE, width=ow, joint="curve")
    d.line([top, bottom], fill=OUTLINE, width=int(2.2 * S))
    d.line([left, right], fill=OUTLINE, width=int(2.2 * S))
    return bottom


def draw_tail(d, start, length, amp, direction=(0.35, 1.0)):
    dx, dy = direction
    n = math.hypot(dx, dy)
    dx, dy = dx / n, dy / n
    px, py = -dy, dx  # perpendicular, for the wave
    pts = []
    for i in range(41):
        t = i / 40
        wave = math.sin(t * math.pi * 3) * amp
        pts.append((start[0] + dx * length * t + px * wave, start[1] + dy * length * t + py * wave))
    d.line(pts, fill=OUTLINE, width=int(2.6 * S), joint="curve")
    for k, col in zip((10, 22, 34), (YELLOW, TEAL, RED)):
        x, y = pts[k]
        r = 9 * S
        d.polygon([(x - r, y - r * 0.6), (x, y), (x - r, y + r * 0.6)], fill=col, outline=OUTLINE)
        d.polygon([(x + r, y - r * 0.6), (x, y), (x + r, y + r * 0.6)], fill=col, outline=OUTLINE)


def save(img, name, size):
    img.resize(size, Image.LANCZOS).save(f"{OUT}/{name}.png")
    print(name, size)


# Pickup icon: kite with a short tail.
W, H = 128, 160
img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
tip = draw_kite(d, 64 * S, 58 * S, 96 * S, 100 * S)
draw_tail(d, tip, 40 * S, 6 * S, (0.3, 1))
save(img, "kite_pickup", (W, H))

# Flying kite: kite up and to the right, string running down to the bottom-left corner (the surfer's hands).
W, H = 256, 320
img = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
kx, ky = 180 * S, 92 * S
tip = (kx, ky + 100 * S * 0.62)
# String: a slight sag from the kite's center down to the corner.
end = (6 * S, (H - 6) * S)
sag = []
for i in range(41):
    t = i / 40
    x = kx + (end[0] - kx) * t
    y = (ky - 8 * S) + (end[1] - (ky - 8 * S)) * t + math.sin(t * math.pi) * 18 * S
    sag.append((x, y))
d.line(sag, fill=STRING, width=int(1.6 * S), joint="curve")
draw_tail(d, tip, 70 * S, 8 * S, (0.55, 1))
draw_kite(d, kx, ky, 110 * S, 112 * S)
save(img, "kite_flying", (W, H))
