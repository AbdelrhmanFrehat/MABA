MABA Control Center - Other PC Kit

This kit has two folders:

1. APP
   - Copy this whole folder to the other PC.
   - Run MabaControlCenter.exe from inside APP.

2. UPDATES
   - Keep this folder somewhere the app on the other PC can access.
   - In the app on the other PC, go to Settings.
   - Set "Update Manifest Path or URL" to:
     <path-to-UPDATES>\manifest.json

Example:
If you copy this kit to:
D:\MabaOtherPcKit

Then:
- Run:
  D:\MabaOtherPcKit\APP\MabaControlCenter.exe

- Set update manifest path to:
  D:\MabaOtherPcKit\UPDATES\manifest.json

After future fixes:
- replace only the contents of the UPDATES folder with the new manifest.json and zip
- on the other PC open the app and click:
  Dashboard -> Check for Updates -> Install Update
