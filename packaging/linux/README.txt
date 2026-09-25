MathExam - math practice app (Linux, 64-bit)
============================================

Start
-----
    tar -xzf MathExam-linux-x64.tar.gz
    cd MathExam
    ./MathExam

No .NET installation is needed: everything the app needs is inside the MathExam file.

Requirements
------------
- A 64-bit (x86-64) Linux desktop with X11, or Wayland with XWayland (the default on
  GNOME and KDE).
- The usual desktop libraries: libX11, libICE, libSM and fontconfig. They are already
  installed on most desktops; if the app does not start, on Debian/Ubuntu run:
      sudo apt install libx11-6 libice6 libsm6 libfontconfig1

Your data
---------
Progress history and display settings are stored in ~/.local/share/MathExam/.
Delete history.json there to clear the history.
