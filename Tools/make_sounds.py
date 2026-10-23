"""Synthesizes all Paddle Out sound effects as 44.1 kHz 16-bit mono WAV files."""
import os
import sys
import numpy as np
from scipy.io import wavfile
from scipy.signal import butter, sosfilt

SR = 44100
OUT = sys.argv[1]
os.makedirs(OUT, exist_ok=True)
rng = np.random.default_rng(7)


def t_axis(dur):
    return np.arange(int(dur * SR)) / SR


def noise(dur):
    return rng.uniform(-1, 1, int(dur * SR))


def brown(dur):
    w = rng.normal(0, 1, int(dur * SR))
    b = np.cumsum(w)
    b -= np.convolve(b, np.ones(2000) / 2000, mode="same")  # remove drift
    return b / (np.max(np.abs(b)) + 1e-9)


def bp(x, lo, hi, order=4):
    return sosfilt(butter(order, [lo, hi], btype="band", fs=SR, output="sos"), x)


def lp(x, f, order=4):
    return sosfilt(butter(order, f, btype="low", fs=SR, output="sos"), x)


def hp(x, f, order=4):
    return sosfilt(butter(order, f, btype="high", fs=SR, output="sos"), x)


def sweep(f0, f1, dur, shape="sine", curve="exp"):
    t = t_axis(dur)
    if curve == "exp":
        f = f0 * (f1 / f0) ** (t / dur)
    else:
        f = f0 + (f1 - f0) * t / dur
    ph = 2 * np.pi * np.cumsum(f) / SR
    if shape == "sine":
        return np.sin(ph)
    if shape == "saw":
        return 2 * ((ph / (2 * np.pi)) % 1) - 1
    if shape == "square":
        return np.sign(np.sin(ph))
    if shape == "tri":
        return 2 * np.abs(2 * ((ph / (2 * np.pi)) % 1) - 1) - 1


def tone(f, dur, shape="sine"):
    return sweep(f, f, dur, shape, curve="lin")


def env(n, attack, decay_tau=None, release=0.01, hold=None):
    """Attack ramp, then exponential decay (or flat hold), then short release."""
    t = np.arange(n) / SR
    e = np.ones(n)
    a = int(attack * SR)
    if a > 0:
        e[:a] = np.linspace(0, 1, a)
    if decay_tau is not None:
        e[a:] = np.exp(-(t[a:] - attack) / decay_tau)
    r = int(release * SR)
    if r > 0:
        e[-r:] *= np.linspace(1, 0, r)
    return e


def place(buf, clip, at):
    i = int(at * SR)
    j = min(len(buf), i + len(clip))
    buf[i:j] += clip[: j - i]
    return buf


def finish(x, peak=0.9):
    x = x - np.mean(x)
    x = x / (np.max(np.abs(x)) + 1e-9) * peak
    f = int(0.003 * SR)
    x[:f] *= np.linspace(0, 1, f)
    x[-f:] *= np.linspace(1, 0, f)
    return x


def loopify(make, dur, xfade):
    """Builds a seamless loop by crossfading the tail back into the head."""
    a = make(dur + xfade)
    n, x = int(dur * SR), int(xfade * SR)
    out = a[:n].copy()
    ramp = np.linspace(0, 1, x)
    out[:x] = a[:x] * ramp + a[n:n + x] * (1 - ramp)
    out = out - np.mean(out)
    return out / (np.max(np.abs(out)) + 1e-9) * 0.9


def save(name, x):
    wavfile.write(os.path.join(OUT, name + ".wav"), SR, (np.clip(x, -1, 1) * 32767).astype(np.int16))
    print(f"{name:16s} {len(x) / SR:5.2f}s")


# 1. dive_splash: noisy splash + low plunge + a few bubbles
d = 0.8
x = np.zeros(int(d * SR))
s = bp(noise(0.5), 400, 6000) * env(int(0.5 * SR), 0.004, 0.09)
place(x, s, 0)
place(x, 0.8 * sweep(260, 70, 0.25) * env(int(0.25 * SR), 0.002, 0.08), 0.01)
for k in range(6):
    f0 = rng.uniform(350, 700)
    b = sweep(f0, f0 * 2.2, 0.05) * env(int(0.05 * SR), 0.002, 0.015)
    place(x, 0.25 * b, 0.18 + k * rng.uniform(0.05, 0.09))
save("dive_splash", finish(x))

# 2. gasp: quick sharp inhale through two "mouth" resonances
d = 0.55
n = noise(d)
g = bp(n, 1100, 1700, 2) + 0.7 * bp(n, 2300, 3200, 2) + 0.3 * bp(n, 500, 900, 2)
e = np.interp(t_axis(d), [0, 0.06, 0.22, 0.4, d], [0, 1, 0.85, 0.25, 0])
save("gasp", finish(g * e))

# 3. heartbeat: "lub-dub" loop (~75 bpm)
def thump(f0, dur, amp):
    return amp * sweep(f0, f0 * 0.55, dur) * env(int(dur * SR), 0.004, dur / 3.5)
d = 0.8
x = np.zeros(int(d * SR))
place(x, thump(70, 0.18, 1.0), 0.0)
place(x, thump(60, 0.16, 0.7), 0.22)
x = lp(x, 400)
save("heartbeat", finish(x))

# 4. hit: punchy thud
d = 0.35
x = sweep(140, 45, d) * env(int(d * SR), 0.002, 0.07)
x += 0.6 * lp(noise(d), 900) * env(int(d * SR), 0.001, 0.03)
save("hit", finish(x))

# 5. wipeout: big wave crash + sad descending slide
d = 1.8
x = np.zeros(int(d * SR))
crash = lp(noise(1.6), 2500) * np.interp(t_axis(1.6), [0, 0.03, 0.5, 1.6], [0, 1, 0.5, 0])
place(x, crash, 0)
for i, (f0, f1) in enumerate([(392, 370), (370, 349), (349, 262)]):
    dur = 0.28 if i < 2 else 0.7
    note = lp(sweep(f0, f1, dur, "saw"), 1800) * env(int(dur * SR), 0.02, None, 0.08)
    place(x, 0.45 * note, 0.25 + i * 0.3)
save("wipeout", finish(x))

# 6. wave_roar: loopable rumbling whitewash
def roar(dur):
    t = t_axis(dur)
    body = bp(brown(dur), 80, 1500) * 1.0
    fizz = bp(noise(dur), 1500, 6000) * 0.25
    flutter = 0.75 + 0.25 * np.sin(2 * np.pi * 0.7 * t + 1.3) * np.sin(2 * np.pi * 0.23 * t)
    return (body + fizz) * flutter
save("wave_roar", loopify(roar, 3.0, 0.4))

# 7. seagull: two harsh "kyaa" calls
def squawk(dur, f_start, f_peak):
    t = t_axis(dur)
    f = np.interp(t, [0, dur * 0.25, dur], [f_start, f_peak, f_start * 0.8])
    f *= 1 + 0.03 * np.sin(2 * np.pi * 28 * t)
    ph = 2 * np.pi * np.cumsum(f) / SR
    saw = 2 * ((ph / (2 * np.pi)) % 1) - 1
    v = bp(saw, 1200, 4500, 2) + 0.15 * bp(noise(dur), 2000, 5000, 2)
    return v * np.interp(t, [0, 0.02, dur * 0.6, dur], [0, 1, 0.8, 0])
d = 0.75
x = np.zeros(int(d * SR))
place(x, squawk(0.28, 700, 1050), 0)
place(x, 0.8 * squawk(0.3, 650, 980), 0.36)
save("seagull", finish(x))

# 8. puffer_hit: zappy downward buzz
d = 0.3
x = sweep(1400, 180, d, "square") * env(int(d * SR), 0.002, 0.08)
x = lp(x, 5000) + 0.3 * sweep(900, 120, d) * env(int(d * SR), 0.002, 0.1)
save("puffer_hit", finish(x))

# 9. set_horn: two warning blasts (two-note horn chord)
d = 1.3
x = np.zeros(int(d * SR))
for start in (0.0, 0.62):
    dur = 0.5
    chord = tone(196, dur, "saw") + 0.8 * tone(247, dur, "saw") + 0.5 * tone(98, dur, "saw")
    chord = lp(chord, 1400) * env(int(dur * SR), 0.04, None, 0.08)
    place(x, chord, start)
save("set_horn", finish(x))

# 10. shell_pickup: bright two-note chime
def bell(f, dur):
    e = env(int(dur * SR), 0.002, dur / 4)
    return (tone(f, dur) + 0.4 * tone(f * 2.01, dur) + 0.15 * tone(f * 3.02, dur)) * e
d = 0.5
x = np.zeros(int(d * SR))
place(x, bell(1319, 0.45), 0)
place(x, bell(1976, 0.42), 0.07)
save("shell_pickup", finish(x))

# 11. breath_pickup: rising bubbles + airy inhale
d = 0.6
x = np.zeros(int(d * SR))
for k, f0 in enumerate([300, 380, 470, 560]):
    b = sweep(f0, f0 * 2.5, 0.07) * env(int(0.07 * SR), 0.002, 0.025)
    place(x, b, k * 0.07)
air = bp(noise(0.45), 900, 3000, 2) * np.interp(t_axis(0.45), [0, 0.3, 0.45], [0, 0.35, 0])
place(x, air, 0.1)
save("breath_pickup", finish(x))

# 12. shield_pickup: sparkly rising arpeggio
d = 0.7
x = np.zeros(int(d * SR))
for k, f in enumerate([523, 659, 784, 1047, 1319]):
    dur = 0.25
    note = (tone(f, dur, "tri") + 0.3 * tone(f * 2, dur)) * env(int(dur * SR), 0.003, 0.08)
    place(x, note, k * 0.07)
save("shield_pickup", finish(x))

# 13. shield_pop: bubble pop + tiny sparkle
d = 0.35
x = np.zeros(int(d * SR))
place(x, sweep(500, 1800, 0.06) * env(int(0.06 * SR), 0.001, 0.02), 0)
place(x, 0.6 * hp(noise(0.03), 2000) * env(int(0.03 * SR), 0.0005, 0.006), 0)
for k, f in enumerate([2093, 2637]):
    place(x, 0.25 * tone(f, 0.15) * env(int(0.15 * SR), 0.002, 0.04), 0.06 + k * 0.05)
save("shield_pop", finish(x))

# 14. ui_click: short soft click
d = 0.07
x = tone(1100, d) * env(int(d * SR), 0.001, 0.012) + 0.4 * hp(noise(d), 3000) * env(int(d * SR), 0.0005, 0.003)
save("ui_click", finish(x, 0.7))

# 15. ocean_ambience: loopable calm surf (one swell per 8 s loop)
LOOP = 8.0
def ocean(dur):
    t = t_axis(dur)
    swell = 0.55 + 0.45 * np.sin(2 * np.pi * t / LOOP - np.pi / 2) ** 2
    wash = np.clip(np.sin(2 * np.pi * t / LOOP - 0.6), 0, None) ** 3
    low = lp(brown(dur), 500) * swell
    hiss = bp(noise(dur), 1200, 6000) * 0.18 * (0.3 + wash)
    return low + hiss
save("ocean_ambience", loopify(ocean, LOOP, 1.0))

# 16. shark_warning: ominous low pulses with a rising swell of water noise
d = 1.6
x = np.zeros(int(d * SR))
for k, start in enumerate([0.0, 0.42, 0.74, 0.98]):
    dur = 0.3
    pulse = lp(tone(49, dur, "saw") + 0.6 * tone(98, dur, "saw"), 500)
    pulse *= env(int(dur * SR), 0.03, None, 0.12) * (0.7 + 0.1 * k)
    place(x, pulse, start)
swell = lp(brown(d), 300) * np.interp(t_axis(d), [0, 1.2, d], [0, 0.6, 0])
x += swell
save("shark_warning", finish(x))

# 17. shark_bite: crunchy chomp with a heavy thump
d = 0.45
x = np.zeros(int(d * SR))
for k in range(3):
    c = bp(noise(0.05), 500, 3500) * env(int(0.05 * SR), 0.001, 0.012)
    place(x, c, 0.03 + k * 0.035)
place(x, 0.9 * sweep(110, 40, 0.3) * env(int(0.3 * SR), 0.002, 0.08), 0.0)
save("shark_bite", finish(x))

# 18. kite_launch: rising gust of wind + bright lift-off sparkle
d = 1.0
t = t_axis(d)
gust = noise(d)
lo = bp(gust, 300, 1200, 2) * np.interp(t, [0, 0.5, 1.0], [0.2, 1, 0])
hi = bp(gust, 1500, 5000, 2) * np.interp(t, [0, 0.6, 1.0], [0, 0.5, 0])
x = lo + hi
for k, f in enumerate([392, 523, 659, 784]):
    note = (tone(f, 0.3, "tri") + 0.3 * tone(f * 2, 0.3)) * env(int(0.3 * SR), 0.003, 0.09)
    place(x, 0.5 * note, 0.15 + k * 0.08)
save("kite_launch", finish(x))

# 19. kite_wind: loopable whistling wind while flying
def wind(dur):
    t = t_axis(dur)
    n = noise(dur)
    gusts = 0.6 + 0.4 * np.sin(2 * np.pi * t / 2.0) ** 2
    whistle = bp(n, 700, 900, 2) * 0.8 + bp(n, 1400, 1700, 2) * 0.3
    rush = bp(n, 200, 2500, 2) * 0.5
    return (whistle + rush) * gusts
save("kite_wind", loopify(wind, 4.0, 0.5))
