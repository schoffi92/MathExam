MathExam - math practice app (Linux)
====================================

Which archive?
--------------
- MathExam-linux-x64.tar.gz    most PCs and laptops (64-bit Intel/AMD)
- MathExam-linux-arm64.tar.gz  64-bit ARM, e.g. Raspberry Pi 3/4/5 with 64-bit Raspberry Pi OS
- MathExam-linux-arm.tar.gz    32-bit ARM, e.g. Raspberry Pi 3 with 32-bit Raspberry Pi OS

Not sure? Run "uname -m": x86_64 means x64, aarch64 means arm64, armv7l means arm.
The Raspberry Pi Zero and Pi 1 (armv6l) are not supported.

Start
-----
    tar -xzf MathExam-linux-<arch>.tar.gz
    cd MathExam
    ./MathExam

No .NET installation is needed: everything the app needs is inside the MathExam file.

Requirements
------------
- A Linux desktop with X11, or Wayland with XWayland (the default on GNOME, KDE and
  Raspberry Pi OS). It does not run without a desktop, e.g. on Raspberry Pi OS Lite.
- The usual desktop libraries: libX11, libICE, libSM and fontconfig. They are already
  installed on most desktops; if the app does not start, on Debian/Ubuntu/Raspberry Pi OS run:
      sudo apt install libx11-6 libice6 libsm6 libfontconfig1
- On a Raspberry Pi 3 the first start can take a few seconds.

Your data
---------
Progress history and display settings are stored in ~/.local/share/MathExam/.
Delete history.json there to clear the history.
