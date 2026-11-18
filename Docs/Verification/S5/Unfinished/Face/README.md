# Possession face clearance

Blender source now clears the mouth in the beard shell, places the dark mouth interior behind the closed lips, and raycasts eye runoff to the actual cheek surface. Wider dark wet trails remain visible when the eyes roll back. The normal face restores after revival. The rig contract, first-person body, hands and locomotion are retained. Inspected all three adjacent renders; 16 focused avatar checks pass. These are lit inspection renders, not headset readability or comfort acceptance. Blood remains stylized geometry and needs a worn F2 lighting check.

Rebuild: Blender background with Tools/Avatars/build_harbor_keeper.py; refresh the stopped muted Editor, then artifacts/s5/possession/FacePreview.cs. Same 56-bone contract; no new network fields.
