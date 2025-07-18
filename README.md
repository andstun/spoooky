# THEMUSEUM EYEPOOL Setup

This repo contains a starter Unity scene to which developers can add to create immersive scenes in THEMUSEUM's EYEPOOL exhibit.

There are a few main components:

1. The SceneViewer is a collection of cameras that see the main scene. You can think of it as a 360-degree camera; move the cameras
   as you like to view the scene you've created.
2. The Eyepool collection represents the EYEPOOL exhibit room itself. The Sceneviewer collection cameras map to the Eyepool collection cameras, which in turn
   map to real projectors. Where the room is placed doesn't matter.

To build on top of the scene, try creating a 3D environment, and place the SceneViewer collection somewhere in that 3D environment to capture it. When you hit play, the Unity output will simulate rendering to multiple displays, and split the viewports of each display into quadrants. There are 4 displays in total, and
only the top 2/4 quadrants on Display 4 are used by EYEPOOL, for a total of 14 quadrants (one for each projector in EYEPOOL). It might take some time to understand how to interpret this output format for local development. If you happen to be developing onsite at EYEPOOL, you can build the Unity scene into an exetuable and load it onto the system directly. This output will be much more intuitive.



## Background and Theory

This section is unnecessary to use EYEPOOL. It contains background information that may help when trying to troubleshoot more
severe EYEPOOL errors, or gain the context required to build on top of the existing system.

There are 14 projectors in EYEPOOL: 2 projectors for each wall (labelled A, B, C, D) and 6 projectors for the floor.

Each projector displays a resolution of 1920 x 1080 (w x h). Each wall has a resolution of 3360 x 1080. You might notice that
the width of the wall is not exactly double that of the project resolution. This is because the projectors blend at the middle.
If you divide the width of each projector display into four panels, EYEPOOL will combine the two into eight panels and then
make the middle two overlap. There are testing patterns available under Assets -> Scenes -> Materials -> Calibration which
demonstrate the blending concept. If the blending is slightly off, the pattern will look clearly disjoint.

The floor is a similar design, but slightly different overlap since there are 6 projectors for a 3360 x 3360 display space as opposed to 2 projectors for a 3360 x 1080 display space.

The walls all blend together seamlessly. If the cameras are designed properly, they all have continuity.
The floor does not blend with the walls, though some continuity is likely still possible.

## Projectors and Display

Currently, the project / wall setup is hardcoded. There's also a script to automatically generate parameters at runtime if you'd like. This is nicer if you want to tweak the parameters of the room. For most use cases, the hard-coded version is likely sufficient.

## Augmenta (User Interaction)

To use Augmenta, you can refer to the AugmentaTech library in this repo as well as the following web resource: https://augmenta.tech/downloads/. You need to download the Augmenta Simulator, which is a separate app that can simulate people walking through EYEPOOL. When you open the Augmenta Simulator, there will be a checkerboard with a side panel to change settings. The ones of greatest interest are probbaly the width and height of the checkerboard; set these to the EYEPOOL dimensions. To use the simulator, you can left click the checkerboard to add a moving object, and right click the object to remove it. The Unity scene already contains an Augmenta object which maps the Augmenta Simulator data in real time.



